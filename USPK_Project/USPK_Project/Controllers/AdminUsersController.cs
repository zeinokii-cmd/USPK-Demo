using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USPK_Project.IServices;
using USPK_Project.Models.Authentication;

namespace USPK_Project.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = "CreateUsers")]
public class AdminUsersController(IServiceProvider services) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var store = services.GetService<IUserStore>();
        if (store is null) return StatusCode(503, new ProblemDetails { Status = 503, Title = "Хранилище пользователей пока недоступно." });
        var users = await store.ListAsync(cancellationToken);
        return Ok(users.Select(UserResponse.From));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (services.GetService<IUserStore>() is null || services.GetService<IRoleCatalog>() is null)
            return StatusCode(503, new ProblemDetails { Status = 503, Title = "Хранилище пользователей пока недоступно." });

        var result = await services.GetRequiredService<IUserService>().CreateAsync(
            request.Email, request.Username, request.Role, request.InitialPassword, cancellationToken);

        return result.Status switch
        {
            CreateUserStatus.InvalidRole => BadRequest(new ProblemDetails { Status = 400, Title = "Недопустимая роль." }),
            CreateUserStatus.DuplicateEmail => Conflict(new ProblemDetails { Status = 409, Title = "Пользователь с таким Email уже существует." }),
            CreateUserStatus.Created when result.User is not null => StatusCode(201, UserResponse.From(result.User)),
            _ => StatusCode(500)
        };
    }
}
