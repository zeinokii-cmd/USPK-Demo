using USPK_Project.Models;

namespace USPK_Project.Models.Authentication;

public record UserResponse(int Id, string Email, string Username, string Role, DateTime CreatedAt)
{
    public static UserResponse From(User user) => new(user.Id, user.Email, user.Username, user.Role, user.CreatedAt);
}
