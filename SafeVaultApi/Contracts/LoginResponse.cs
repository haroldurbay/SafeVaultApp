namespace SafeVaultApi.Contracts;

/// <summary>Represents a successful user login.</summary>
public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt);
