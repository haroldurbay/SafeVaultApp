using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafeVaultApi.Contracts;
using SafeVaultApi.Identity;

namespace SafeVaultApi.Services;

public sealed class AdminService(
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager) : IAdminService
{
    public async Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var totalUsers = await userManager.Users.CountAsync(cancellationToken);
        var totalRoles = await roleManager.Roles.CountAsync(cancellationToken);
        var adminUsers = await userManager.GetUsersInRoleAsync(IdentityRoles.Admin);
        cancellationToken.ThrowIfCancellationRequested();

        return new AdminDashboardResponse(totalUsers, adminUsers.Count, totalRoles);
    }

    public async Task<IReadOnlyList<AdminUserResponse>> GetUsersAsync(CancellationToken cancellationToken)
    {
        var users = await userManager.Users
            .OrderBy(user => user.UserName)
            .ToListAsync(cancellationToken);
        var responses = new List<AdminUserResponse>(users.Count);

        foreach (var user in users)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var roles = await userManager.GetRolesAsync(user);
            responses.Add(new AdminUserResponse(user.Id, user.UserName!, user.Email!, roles.ToArray()));
        }

        return responses;
    }

    public async Task<IReadOnlyList<AdminRoleResponse>> GetRolesAsync(CancellationToken cancellationToken)
    {
        return await roleManager.Roles
            .OrderBy(role => role.Name)
            .Select(role => new AdminRoleResponse(role.Id, role.Name!))
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminOperationResult> CreateRoleAsync(
        CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var name = request.Name.Trim();

        if (await roleManager.RoleExistsAsync(name))
        {
            return new AdminOperationResult(AdminOperationStatus.Conflict, "The role already exists.");
        }

        var role = new IdentityRole(name);
        var result = await roleManager.CreateAsync(role);
        cancellationToken.ThrowIfCancellationRequested();

        return result.Succeeded
            ? new AdminOperationResult(
                AdminOperationStatus.Success,
                Role: new AdminRoleResponse(role.Id, role.Name!))
            : new AdminOperationResult(AdminOperationStatus.Invalid, "The role could not be created.");
    }

    public async Task<AdminOperationResult> DeleteRoleAsync(
        string roleName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsDefaultRole(roleName))
        {
            return new AdminOperationResult(
                AdminOperationStatus.Invalid,
                "The default Admin and Users roles cannot be deleted.");
        }

        var role = await roleManager.FindByNameAsync(roleName);

        if (role is null)
        {
            return new AdminOperationResult(AdminOperationStatus.NotFound, "The role was not found.");
        }

        if ((await userManager.GetUsersInRoleAsync(roleName)).Count > 0)
        {
            return new AdminOperationResult(
                AdminOperationStatus.Conflict,
                "Remove the role from all users before deleting it.");
        }

        var result = await roleManager.DeleteAsync(role);
        cancellationToken.ThrowIfCancellationRequested();

        return result.Succeeded
            ? new AdminOperationResult(AdminOperationStatus.Success)
            : new AdminOperationResult(AdminOperationStatus.Invalid, "The role could not be deleted.");
    }

    public async Task<AdminOperationResult> AddUserToRoleAsync(
        string userId,
        string roleName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return new AdminOperationResult(AdminOperationStatus.NotFound, "The user was not found.");
        }

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            return new AdminOperationResult(AdminOperationStatus.NotFound, "The role was not found.");
        }

        if (await userManager.IsInRoleAsync(user, roleName))
        {
            return new AdminOperationResult(AdminOperationStatus.Conflict, "The user already has this role.");
        }

        var result = await userManager.AddToRoleAsync(user, roleName);
        cancellationToken.ThrowIfCancellationRequested();

        return result.Succeeded
            ? new AdminOperationResult(AdminOperationStatus.Success)
            : new AdminOperationResult(AdminOperationStatus.Invalid, "The role could not be assigned.");
    }

    public async Task<AdminOperationResult> RemoveUserFromRoleAsync(
        string userId,
        string roleName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return new AdminOperationResult(AdminOperationStatus.NotFound, "The user was not found.");
        }

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            return new AdminOperationResult(AdminOperationStatus.NotFound, "The role was not found.");
        }

        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            return new AdminOperationResult(AdminOperationStatus.Conflict, "The user does not have this role.");
        }

        if (roleName == IdentityRoles.Admin && await IsLastAdminAsync(user))
        {
            return new AdminOperationResult(
                AdminOperationStatus.Conflict,
                "The last Admin user cannot lose the Admin role.");
        }

        var result = await userManager.RemoveFromRoleAsync(user, roleName);
        cancellationToken.ThrowIfCancellationRequested();

        return result.Succeeded
            ? new AdminOperationResult(AdminOperationStatus.Success)
            : new AdminOperationResult(AdminOperationStatus.Invalid, "The role could not be removed.");
    }

    public async Task<AdminOperationResult> DeleteUserAsync(
        string userId,
        string requestingUserId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (userId == requestingUserId)
        {
            return new AdminOperationResult(
                AdminOperationStatus.Invalid,
                "Administrators cannot delete their own account.");
        }

        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return new AdminOperationResult(AdminOperationStatus.NotFound, "The user was not found.");
        }

        if (await userManager.IsInRoleAsync(user, IdentityRoles.Admin) && await IsLastAdminAsync(user))
        {
            return new AdminOperationResult(
                AdminOperationStatus.Conflict,
                "The last Admin user cannot be deleted.");
        }

        var result = await userManager.DeleteAsync(user);
        cancellationToken.ThrowIfCancellationRequested();

        return result.Succeeded
            ? new AdminOperationResult(AdminOperationStatus.Success)
            : new AdminOperationResult(AdminOperationStatus.Invalid, "The user could not be deleted.");
    }

    private static bool IsDefaultRole(string roleName)
    {
        return roleName is IdentityRoles.Admin or IdentityRoles.Users;
    }

    private async Task<bool> IsLastAdminAsync(IdentityUser user)
    {
        var admins = await userManager.GetUsersInRoleAsync(IdentityRoles.Admin);
        return admins.Count == 1 && admins[0].Id == user.Id;
    }
}
