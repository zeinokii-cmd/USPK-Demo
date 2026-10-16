using USPK_Project.Models;

namespace USPK_Project.IServices
{
    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken);
        Task<CreateUserResult> CreateAsync(string email, string username, string role, string initialPassword, CancellationToken cancellationToken);
    }

    public enum CreateUserStatus { Created, DuplicateEmail, InvalidRole }
    public record CreateUserResult(CreateUserStatus Status, User? User = null);
}
