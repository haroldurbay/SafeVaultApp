using System.ComponentModel.DataAnnotations;

namespace SafeVaultApi.Contracts;

/// <summary>Payload for registering a new user.</summary>
public sealed record RegisterUserRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression("^[a-zA-Z0-9_.-]+$")]
    public required string Username { get; init; }

    [Required]
    [EmailAddress]
    [StringLength(254)]
    [RegularExpression(
        @"^[^<>\s'"";]+@[^<>\s'"";]+$",
        ErrorMessage = "Email cannot contain markup, whitespace, or query characters.")]
    public required string Email { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 12)]
    [DataType(DataType.Password)]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{12,100}$",
        ErrorMessage = "Password must include uppercase, lowercase, numeric, and non-alphanumeric characters.")]
    public required string Password { get; init; }
}
