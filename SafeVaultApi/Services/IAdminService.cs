using SafeVaultApi.Contracts;

namespace SafeVaultApi.Services;

public interface IAdminService
{
    Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminUserResponse>> GetUsersAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminRoleResponse>> GetRolesAsync(CancellationToken cancellationToken);

    Task<AdminOperationResult> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken);

    Task<AdminOperationResult> DeleteRoleAsync(string roleName, CancellationToken cancellationToken);

    Task<AdminOperationResult> AddUserToRoleAsync(
        string userId,
        string roleName,
        CancellationToken cancellationToken);

    Task<AdminOperationResult> RemoveUserFromRoleAsync(
        string userId,
        string roleName,
        CancellationToken cancellationToken);

    Task<AdminOperationResult> DeleteUserAsync(string userId, string requestingUserId, CancellationToken cancellationToken);
}
