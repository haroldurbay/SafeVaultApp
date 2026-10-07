using System.ComponentModel.DataAnnotations;

namespace SafeVaultApi.Contracts;

/// <summary>Payload for creating an Identity role.</summary>
public sealed record CreateRoleRequest
{
    [Required]
    [RegularExpression("^[a-zA-Z0-9_-]{3,50}$")]
    public required string Name { get; init; }
}
