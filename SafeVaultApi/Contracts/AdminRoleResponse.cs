namespace SafeVaultApi.Contracts;

/// <summary>Represents an Identity role available to an administrator.</summary>
public sealed record AdminRoleResponse(
    string Id,
    string Name);
