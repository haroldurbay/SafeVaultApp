namespace SafeVaultApi.Contracts;

/// <summary>Represents a user after successful registration.</summary>
public sealed record UserRegistrationResponse(
    string Id,
    string Username,
    string Email);
