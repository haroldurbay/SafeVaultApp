using Microsoft.AspNetCore.Mvc;
using SafeVaultApi.Contracts;
using SafeVaultApi.Services;

namespace SafeVaultApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IUserAuthenticationService authenticationService) : ControllerBase
{
    /// <summary>Authenticates a user and returns a JWT bearer token.</summary>
    [HttpPost("login", Name = "LoginUser")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginUserRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authenticationService.LoginAsync(request, cancellationToken);

        return response is null
            ? Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid username or password."
            })
            : Ok(response);
    }
}
