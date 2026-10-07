namespace SafeVaultApi.Contracts;

/// <summary>Represents summary metrics available to administrators.</summary>
public sealed record AdminDashboardResponse(
    int TotalUsers,
    int TotalAdminUsers,
    int TotalRoles);
