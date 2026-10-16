using Npgsql;
using USPK_Project.IServices;
using USPK_Project.Models;

namespace USPK_Project.Services;

// Isolated adapter for the disposable demonstration database. It does not use team tables.
public sealed class DemoPostgresStore(NpgsqlDataSource dataSource) : IUserStore, IRoleCatalog
{
    private const string Columns = "u.id, u.username, u.email, u.password_hash, r.name, u.created_at";

    public async Task<User?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand($"SELECT {Columns} FROM uspk_demo.users u JOIN uspk_demo.roles r ON r.id = u.role_id WHERE u.normalized_email = @email");
        command.Parameters.AddWithValue("email", normalizedEmail);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadUser(reader) : null;
    }

    public async Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand($"SELECT {Columns} FROM uspk_demo.users u JOIN uspk_demo.roles r ON r.id = u.role_id WHERE u.id = @id");
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadUser(reader) : null;
    }

    public async Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand($"SELECT {Columns} FROM uspk_demo.users u JOIN uspk_demo.roles r ON r.id = u.role_id ORDER BY u.id");
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var users = new List<User>();
        while (await reader.ReadAsync(cancellationToken)) users.Add(ReadUser(reader));
        return users;
    }

    public async Task<bool> CanAssignAsync(string role, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand("SELECT EXISTS (SELECT 1 FROM uspk_demo.roles WHERE name = @role)");
        command.Parameters.AddWithValue("role", role);
        return (bool)(await command.ExecuteScalarAsync(cancellationToken))!;
    }

    public async Task<bool> CreateAsync(User user, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand("""
            INSERT INTO uspk_demo.users (username, email, normalized_email, password_hash, role_id, created_at)
            VALUES (@username, @email, @normalized_email, @password_hash,
                (SELECT id FROM uspk_demo.roles WHERE name = @role), @created_at)
            RETURNING id
            """);
        command.Parameters.AddWithValue("username", user.Username);
        command.Parameters.AddWithValue("email", user.Email);
        command.Parameters.AddWithValue("normalized_email", UserService.NormalizeEmail(user.Email));
        command.Parameters.AddWithValue("password_hash", user.PasswordHash);
        command.Parameters.AddWithValue("role", user.Role);
        command.Parameters.AddWithValue("created_at", user.CreatedAt);
        try
        {
            user.Id = (int)(await command.ExecuteScalarAsync(cancellationToken))!;
            return true;
        }
        catch (PostgresException error) when (error.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return false;
        }
    }

    private static User ReadUser(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Username = reader.GetString(1),
        Email = reader.GetString(2),
        PasswordHash = reader.GetString(3),
        Role = reader.GetString(4),
        CreatedAt = reader.GetDateTime(5)
    };
}
