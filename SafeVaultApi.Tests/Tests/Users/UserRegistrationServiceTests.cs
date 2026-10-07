using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SafeVaultApi.Contracts;
using SafeVaultApi.Data;
using SafeVaultApi.Identity;
using SafeVaultApi.Services;

namespace SafeVaultApi.Tests.Tests.Users;

public class UserRegistrationServiceTests
{
    [Test]
    public async Task RegisterAsync_ReturnsUser_WhenUsernameEmailAndPasswordAreValid()
    {
        await using var serviceProvider = CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        await SeedRolesAsync(scope.ServiceProvider);
        var service = CreateService(scope.ServiceProvider);

        var result = await service.RegisterAsync(
            new RegisterUserRequest
            {
                Username = "harold.smith",
                Email = "harold.smith@example.com",
                Password = "Str0ng!Password"
            },
            CancellationToken.None);

        Assert.That(result.User, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.User!.Username, Is.EqualTo("harold.smith"));
            Assert.That(result.User.Email, Is.EqualTo("harold.smith@example.com"));
            Assert.That(result.Errors, Is.Empty);
        });

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await userManager.FindByNameAsync("harold.smith");

        Assert.That(await userManager.IsInRoleAsync(user!, IdentityRoles.Users), Is.True);
    }

    [Test]
    public async Task RegisterAsync_ReturnsDuplicateError_WhenUsernameOrEmailAlreadyExists()
    {
        await using var serviceProvider = CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        await SeedRolesAsync(scope.ServiceProvider);
        var service = CreateService(scope.ServiceProvider);
        var firstRegistration = new RegisterUserRequest
        {
            Username = "harold.smith",
            Email = "harold.smith@example.com",
            Password = "Str0ng!Password"
        };

        await service.RegisterAsync(firstRegistration, CancellationToken.None);
        var duplicateUsername = await service.RegisterAsync(
            firstRegistration with { Email = "other@example.com" },
            CancellationToken.None);
        var duplicateEmail = await service.RegisterAsync(
            firstRegistration with { Username = "other.user" },
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(duplicateUsername.User, Is.Null);
            Assert.That(duplicateUsername.Errors, Is.Not.Empty);
            Assert.That(duplicateEmail.User, Is.Null);
            Assert.That(duplicateEmail.Errors, Is.Not.Empty);
        });
    }

    [Test]
    public async Task IdentityAdminSeeder_CreatesAdminWithAdminRole()
    {
        await using var serviceProvider = CreateServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        await SeedRolesAsync(scope.ServiceProvider);
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await IdentityAdminSeeder.SeedAsync(userManager, "Admin.SafeVault.2026");

        var admin = await userManager.FindByNameAsync(IdentityAdminSeeder.Username);

        Assert.That(admin, Is.Not.Null);
        Assert.That(admin!.Email, Is.EqualTo(IdentityAdminSeeder.Email));
        Assert.That(await userManager.CheckPasswordAsync(admin, "Admin.SafeVault.2026"), Is.True);
        Assert.That(await userManager.IsInRoleAsync(admin, IdentityRoles.Admin), Is.True);
    }

    [TestCase("script.user", "<script>alert(1)</script>@example.com")]
    [TestCase("script.user", "user@example.com\r\nInjected-Header: value")]
    [TestCase("user'; DROP TABLE Users;--", "safe@example.com")]
    public void RegisterUserRequest_FailsValidation_WhenInputContainsHarmfulCharacters(
        string username,
        string email)
    {
        var request = new RegisterUserRequest
        {
            Username = username,
            Email = email,
            Password = "Str0ng!Password"
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        Assert.That(isValid, Is.False);
        Assert.That(validationResults, Is.Not.Empty);
    }

    private static UserRegistrationService CreateService(IServiceProvider serviceProvider)
    {
        return new UserRegistrationService(
            serviceProvider.GetRequiredService<UserManager<IdentityUser>>());
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<SafeVaultDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services
            .AddIdentityCore<IdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<SafeVaultDbContext>();

        return services.BuildServiceProvider();
    }

    private static Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        return IdentityRoleSeeder.SeedAsync(
            serviceProvider.GetRequiredService<RoleManager<IdentityRole>>());
    }
}
