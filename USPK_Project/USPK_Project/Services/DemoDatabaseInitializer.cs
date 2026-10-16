using Microsoft.AspNetCore.Identity;
using Npgsql;
using USPK_Project.IServices;
using USPK_Project.Models;

namespace USPK_Project.Services;

public static class DemoDatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        var password = configuration["Demo:SeedPassword"];
        if (string.IsNullOrWhiteSpace(password) || password.Length < 12)
            throw new InvalidOperationException("Demo:SeedPassword must contain at least 12 characters.");

        var dataSource = services.GetRequiredService<NpgsqlDataSource>();
        await using (var command = dataSource.CreateCommand("""
            CREATE SCHEMA IF NOT EXISTS uspk_demo;
            CREATE TABLE IF NOT EXISTS uspk_demo.roles (
                id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                name text NOT NULL UNIQUE
            );
            CREATE TABLE IF NOT EXISTS uspk_demo.users (
                id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                username varchar(100) NOT NULL,
                email varchar(254) NOT NULL,
                normalized_email varchar(254) NOT NULL UNIQUE,
                password_hash text NOT NULL,
                phone varchar(30),
                access_level integer,
                role_id integer NOT NULL REFERENCES uspk_demo.roles(id),
                created_at timestamptz NOT NULL
            );
            INSERT INTO uspk_demo.roles (name) VALUES ('admin'), ('student'), ('parent'), ('teacher')
            ON CONFLICT (name) DO NOTHING;
            """))
        {
            await command.ExecuteNonQueryAsync();
        }

        using var scope = services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IUserStore>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var demoUsers = new (string Email, string Username, string Role)[]
        {
            ("admin@example.test", "Администратор Демо", "admin"),
            ("student@example.test", "Студент Демо", "student"),
            ("parent@example.test", "Родитель Демо", "parent"),
            ("teacher@example.test", "Преподаватель Демо", "teacher")
        };
        foreach (var (email, username, role) in demoUsers)
        {
            var normalizedEmail = UserService.NormalizeEmail(email);
            var existing = await store.FindByNormalizedEmailAsync(normalizedEmail, CancellationToken.None);
            if (existing is not null)
            {
                var verification = hasher.VerifyHashedPassword(existing, existing.PasswordHash, password);
                if (verification == PasswordVerificationResult.Success) continue;

                // Only repair the four known accounts in the isolated local demo database.
                await using var update = dataSource.CreateCommand("""
                    UPDATE uspk_demo.users SET password_hash = @password_hash
                    WHERE normalized_email = @normalized_email
                    """);
                update.Parameters.AddWithValue("password_hash", hasher.HashPassword(existing, password));
                update.Parameters.AddWithValue("normalized_email", normalizedEmail);
                await update.ExecuteNonQueryAsync();
                continue;
            }
            var user = new User { Email = email, Username = username, Role = role };
            user.PasswordHash = hasher.HashPassword(user, password);
            await store.CreateAsync(user, CancellationToken.None);
        }
    }
}
