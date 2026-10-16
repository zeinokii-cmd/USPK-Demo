using USPK_Project.Models;

namespace USPK_Project.IServices
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        int? ValidateToken(string token);
    }
}
