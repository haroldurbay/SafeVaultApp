using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace SafeVaultApi.Tests.Infrastructure;

public sealed class SafeVaultApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(
            (_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                [
                    new KeyValuePair<string, string?>("Jwt:Issuer", "SafeVaultApi.Tests"),
                    new KeyValuePair<string, string?>("Jwt:Audience", "SafeVaultApi.Tests.Client"),
                    new KeyValuePair<string, string?>(
                        "Jwt:SigningKey",
                        "TestJwtSigningKeyMustBeAtLeast32CharactersLong"),
                    new KeyValuePair<string, string?>("Jwt:ExpirationMinutes", "60"),
                    new KeyValuePair<string, string?>("SeedAdmin:Password", "Admin.SafeVault.2026")
                ]);
            });
    }
}
