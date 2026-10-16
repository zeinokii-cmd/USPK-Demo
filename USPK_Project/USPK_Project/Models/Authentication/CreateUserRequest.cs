using System.ComponentModel.DataAnnotations;

namespace USPK_Project.Models.Authentication;

public class CreateUserRequest
{
    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Role { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 12)]
    public string InitialPassword { get; set; } = string.Empty;
}
