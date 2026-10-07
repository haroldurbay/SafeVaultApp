namespace SafeVaultApi.Contracts;

/// <summary>Represents a user available to an administrator.</summary>
public sealed record AdminUserResponse(
    string Id,
    string Username,
    string Email,
    IReadOnlyList<string> Roles);
