using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeVaultApi.Contracts;
using SafeVaultApi.Identity;
using SafeVaultApi.Services;

namespace SafeVaultApi.Controllers;

[ApiController]
[Authorize(Roles = IdentityRoles.Admin)]
[Route("api/admin")]
public sealed class AdminController(IAdminService adminService) : ControllerBase
{
    /// <summary>Gets user and role summary metrics for the Admin dashboard.</summary>
    [HttpGet("dashboard", Name = "GetAdminDashboard")]
    [ProducesResponseType(typeof(AdminDashboardResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminDashboardResponse>> GetDashboard(
        CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetDashboardAsync(cancellationToken));
    }

    /// <summary>Gets all users and their assigned roles.</summary>
    [HttpGet("users", Name = "GetAdminUsers")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminUserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminUserResponse>>> GetUsers(
        CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetUsersAsync(cancellationToken));
    }

    /// <summary>Gets all Identity roles.</summary>
    [HttpGet("roles", Name = "GetAdminRoles")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminRoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminRoleResponse>>> GetRoles(
        CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetRolesAsync(cancellationToken));
    }

    /// <summary>Creates a role.</summary>
    [HttpPost("roles", Name = "CreateAdminRole")]
    [ProducesResponseType(typeof(AdminRoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateRole(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await adminService.CreateRoleAsync(request, cancellationToken);

        return result.Status == AdminOperationStatus.Success
            ? Created($"/api/admin/roles/{result.Role!.Name}", result.Role)
            : ToErrorResult(result);
    }

    /// <summary>Assigns a role to a user.</summary>
    [HttpPut("users/{userId}/roles/{roleName}", Name = "AddUserToRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddUserToRole(
        string userId,
        string roleName,
        CancellationToken cancellationToken)
    {
        var result = await adminService.AddUserToRoleAsync(userId, roleName, cancellationToken);
        return result.Status == AdminOperationStatus.Success ? NoContent() : ToErrorResult(result);
    }

    /// <summary>Removes a role from a user.</summary>
    [HttpDelete("users/{userId}/roles/{roleName}", Name = "RemoveUserFromRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveUserFromRole(
        string userId,
        string roleName,
        CancellationToken cancellationToken)
    {
        var result = await adminService.RemoveUserFromRoleAsync(userId, roleName, cancellationToken);
        return result.Status == AdminOperationStatus.Success ? NoContent() : ToErrorResult(result);
    }

    /// <summary>Deletes a user other than the currently authenticated administrator.</summary>
    [HttpDelete("users/{userId}", Name = "DeleteAdminUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteUser(string userId, CancellationToken cancellationToken)
    {
        var requestingUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (requestingUserId is null)
        {
            return Unauthorized();
        }

        var result = await adminService.DeleteUserAsync(userId, requestingUserId, cancellationToken);
        return result.Status == AdminOperationStatus.Success ? NoContent() : ToErrorResult(result);
    }

    /// <summary>Deletes a custom role that is not assigned to any user.</summary>
    [HttpDelete("roles/{roleName}", Name = "DeleteAdminRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteRole(string roleName, CancellationToken cancellationToken)
    {
        var result = await adminService.DeleteRoleAsync(roleName, cancellationToken);
        return result.Status == AdminOperationStatus.Success ? NoContent() : ToErrorResult(result);
    }

    private IActionResult ToErrorResult(AdminOperationResult result)
    {
        var statusCode = result.Status switch
        {
            AdminOperationStatus.NotFound => StatusCodes.Status404NotFound,
            AdminOperationStatus.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem(
            statusCode: statusCode,
            title: "Administrative operation could not be completed.",
            detail: result.Detail);
    }
}
