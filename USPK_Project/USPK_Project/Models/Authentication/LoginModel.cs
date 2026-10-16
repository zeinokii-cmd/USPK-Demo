using System.ComponentModel.DataAnnotations;

namespace USPK_Project.Models.Authentication
{
    public class LoginModel
    {
        [Required, EmailAddress, MaxLength(254)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(1024)]
        public string Password { get; set; } = string.Empty;
    }
}
