using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USPK_Project.IServices;
using USPK_Project.Models.Authentication;
using USPK_Project.Services;

namespace USPK_Project.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IServiceProvider services, IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginModel request, CancellationToken cancellationToken)
    {
        if (!JwtService.IsConfigured(configuration) || services.GetService<IUserStore>() is null)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Сервис авторизации пока недоступен."
            });

        var user = await services.GetRequiredService<IUserService>()
            .AuthenticateAsync(request.Email, request.Password, cancellationToken);
        if (user is null) return Unauthorized(new ProblemDetails { Status = 401, Title = "Неверный Email или пароль." });

        var token = services.GetRequiredService<IJwtService>().GenerateToken(user);
        return Ok(new AuthResponse
        {
            Token = token,
            Expires = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo,
            Email = user.Email,
            Username = user.Username,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var users = services.GetService<IUserStore>();
        if (users is null) return StatusCode(503, new ProblemDetails { Status = 503, Title = "Хранилище пользователей пока недоступно." });
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)) return Unauthorized();
        var user = await users.FindByIdAsync(id, cancellationToken);
        return user is null ? Unauthorized() : Ok(UserResponse.From(user));
    }
}
