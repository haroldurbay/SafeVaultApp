namespace SafeVaultApi.Authentication;

public sealed class SeedAdminOptions
{
    public const string SectionName = "SeedAdmin";

    public required string Password { get; init; }
}
