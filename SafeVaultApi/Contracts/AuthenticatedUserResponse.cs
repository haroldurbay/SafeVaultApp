namespace SafeVaultApi.Contracts;

/// <summary>Represents the currently authenticated user.</summary>
public sealed record AuthenticatedUserResponse(
    string Id,
    string Username,
    string Email,
    IReadOnlyList<string> Roles);
