using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SafeVaultApi.Contracts;
using SafeVaultApi.Services;

namespace SafeVaultApi.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(
    IUserRegistrationService registrationService,
    IUserAuthenticationService authenticationService) : ControllerBase
{
    /// <summary>Registers a user using a unique username and email address.</summary>
    [HttpPost("register", Name = "RegisterUser")]
    [EnableRateLimiting("registration")]
    [ProducesResponseType(typeof(UserRegistrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserRegistrationResponse>> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await registrationService.RegisterAsync(request, cancellationToken);

        if (result.User is not null)
        {
            return Created($"/api/users/{result.User.Id}", result.User);
        }

        if (result.Errors.Any(error =>
                error.Code is "DuplicateUserName" or "DuplicateEmail"))
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Username or email already registered",
                Detail = "Choose a different username or email address."
            });
        }

        return BadRequest(new ValidationProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Password does not meet registration requirements."
        });
    }

    /// <summary>Gets the authenticated user's profile.</summary>
    [HttpGet("me", Name = "GetCurrentUser")]
    [Authorize]
    [ProducesResponseType(typeof(AuthenticatedUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticatedUserResponse>> GetCurrentUser(
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await authenticationService.GetUserAsync(userId, cancellationToken);
        return user is null ? Unauthorized() : Ok(user);
    }
}
