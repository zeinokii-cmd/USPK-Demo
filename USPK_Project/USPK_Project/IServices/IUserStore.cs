using USPK_Project.Models;

namespace USPK_Project.IServices;

// The database adapter must enforce a UNIQUE constraint on normalized email.
public interface IUserStore
{
    Task<User?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> CreateAsync(User user, CancellationToken cancellationToken);
    Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken);
}
