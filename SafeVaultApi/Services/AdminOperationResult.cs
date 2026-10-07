using SafeVaultApi.Contracts;

namespace SafeVaultApi.Services;

public enum AdminOperationStatus
{
    Success,
    NotFound,
    Conflict,
    Invalid
}

public sealed record AdminOperationResult(
    AdminOperationStatus Status,
    string? Detail = null,
    AdminRoleResponse? Role = null);
