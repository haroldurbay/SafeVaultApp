using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SafeVaultApi.Contracts;
using SafeVaultApi.Tests.Infrastructure;

namespace SafeVaultApi.Tests.Tests.Authentication;

public class AuthenticationEndpointsTests
{
    private SafeVaultApiFactory factory = null!;

    [SetUp]
    public void SetUp()
    {
        factory = new SafeVaultApiFactory();
    }

    [TearDown]
    public void TearDown()
    {
        factory.Dispose();
    }

    [TestCase("Admin", "incorrect-password")]
    [TestCase("unknown.user", "Str0ng!Password")]
    public async Task Login_ReturnsUnauthorized_ForInvalidCredentials(string username, string password)
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { username, password });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [TestCase("/api/users/me")]
    [TestCase("/api/admin/users")]
    public async Task ProtectedEndpoint_ReturnsUnauthorized_WithoutBearerToken(string path)
    {
        using var client = CreateClient();

        var response = await client.GetAsync(path);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [TestCase("admin' OR '1'='1", "safe@example.com")]
    [TestCase("admin; DROP TABLE AspNetUsers;--", "safe@example.com")]
    [TestCase("<script>alert('xss')</script>", "safe@example.com")]
    [TestCase("safe.user", "<img src=x onerror=alert(1)>@example.com")]
    public async Task Registration_ReturnsBadRequest_ForSqlInjectionOrXssPayload(
        string username,
        string email)
    {
        using var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/users/register",
            new
            {
                username,
                email,
                password = "Str0ng!Password"
            });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task AdminRole_CanAccessAdminManagementEndpoint()
    {
        using var client = CreateClient();
        var accessToken = await LoginAsync(client, "Admin", "Admin.SafeVault.2026");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("/api/admin/users");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task AdminRole_CanAccessDashboard()
    {
        using var client = CreateClient();
        var accessToken = await LoginAsync(client, "Admin", "Admin.SafeVault.2026");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("/api/admin/dashboard");
        var dashboard = await response.Content.ReadFromJsonAsync<AdminDashboardResponse>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(dashboard, Is.Not.Null);
        Assert.That(dashboard!.TotalAdminUsers, Is.EqualTo(1));
    }

    [Test]
    public async Task UsersRole_CannotAccessAdminManagementEndpoint()
    {
        using var client = CreateClient();
        const string username = "standard.user";
        const string email = "standard.user@example.com";
        const string password = "Str0ng!Password";

        var registrationResponse = await client.PostAsJsonAsync(
            "/api/users/register",
            new { username, email, password });
        Assert.That(registrationResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var accessToken = await LoginAsync(client, username, password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("/api/admin/users");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task UsersRole_CannotAccessDashboard()
    {
        using var client = CreateClient();
        const string username = "dashboard.user";
        const string email = "dashboard.user@example.com";
        const string password = "Str0ng!Password";

        var registrationResponse = await client.PostAsJsonAsync(
            "/api/users/register",
            new { username, email, password });
        Assert.That(registrationResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var accessToken = await LoginAsync(client, username, password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("/api/admin/dashboard");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    private HttpClient CreateClient()
    {
        return factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
    }

    private static async Task<string> LoginAsync(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { username, password });
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(loginResponse, Is.Not.Null);

        return loginResponse!.AccessToken;
    }
}
