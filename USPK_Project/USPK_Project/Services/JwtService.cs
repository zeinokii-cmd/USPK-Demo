using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using USPK_Project.IServices;
using USPK_Project.Models;

namespace USPK_Project.Services;

public class JwtService(IConfiguration configuration) : IJwtService
{
    public static bool IsConfigured(IConfiguration configuration) =>
        !string.IsNullOrWhiteSpace(configuration["Jwt:Issuer"]) &&
        !string.IsNullOrWhiteSpace(configuration["Jwt:Audience"]) &&
        Encoding.UTF8.GetByteCount(configuration["Jwt:SigningKey"] ?? string.Empty) >= 32;

    public string GenerateToken(User user)
    {
        if (!IsConfigured(configuration)) throw new InvalidOperationException("JWT configuration is unavailable.");
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ],
            notBefore: now,
            expires: now.AddMinutes(15),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SigningKey"]!)),
                SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int? ValidateToken(string token)
    {
        if (!IsConfigured(configuration)) return null;
        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, CreateValidationParameters(configuration), out _);
            return int.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
        }
        catch (SecurityTokenException) { return null; }
        catch (ArgumentException) { return null; }
    }

    public static TokenValidationParameters CreateValidationParameters(IConfiguration configuration) => new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SigningKey"]!)),
        ValidateIssuer = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = configuration["Jwt:Audience"],
        ValidateLifetime = true,
        RequireExpirationTime = true,
        RequireSignedTokens = true,
        ClockSkew = TimeSpan.Zero
    };
}
