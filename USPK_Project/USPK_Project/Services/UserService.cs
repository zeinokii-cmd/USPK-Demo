using Microsoft.AspNetCore.Identity;
using USPK_Project.IServices;
using USPK_Project.Models;

namespace USPK_Project.Services;

public class UserService(IUserStore users, IServiceProvider services, IPasswordHasher<User> passwordHasher) : IUserService
{
    public async Task<User?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await users.FindByNormalizedEmailAsync(NormalizeEmail(email), cancellationToken);
        if (user is null || string.IsNullOrEmpty(user.PasswordHash)) return null;

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Failed ? null : user;
    }

    public async Task<CreateUserResult> CreateAsync(string email, string username, string role, string initialPassword, CancellationToken cancellationToken)
    {
        var roles = services.GetRequiredService<IRoleCatalog>();
        if (!await roles.CanAssignAsync(role, cancellationToken))
            return new(CreateUserStatus.InvalidRole);

        var normalizedEmail = NormalizeEmail(email);
        if (await users.FindByNormalizedEmailAsync(normalizedEmail, cancellationToken) is not null)
            return new(CreateUserStatus.DuplicateEmail);

        var user = new User
        {
            Email = email.Trim(),
            Username = username.Trim(),
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = passwordHasher.HashPassword(user, initialPassword);

        // CreateAsync must atomically reject duplicate normalized emails, including races.
        return await users.CreateAsync(user, cancellationToken)
            ? new(CreateUserStatus.Created, user)
            : new(CreateUserStatus.DuplicateEmail);
    }

    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}
