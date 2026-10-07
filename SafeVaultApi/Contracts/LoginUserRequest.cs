using System.ComponentModel.DataAnnotations;

namespace SafeVaultApi.Contracts;

/// <summary>Payload for authenticating an existing user.</summary>
public sealed record LoginUserRequest
{
    [Required]
    public required string Username { get; init; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; init; }
}
