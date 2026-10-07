using Microsoft.AspNetCore.Identity;
using SafeVaultApi.Identity;

namespace SafeVaultApi.Data;

public static class IdentityAdminSeeder
{
    public const string Username = "Admin";
    public const string Email = "admin@safevaulttest.com";

    public static async Task SeedAsync(
        UserManager<IdentityUser> userManager,
        string password)
    {
        var user = await userManager.FindByNameAsync(Username);

        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = Username,
                Email = Email,
                EmailConfirmed = true
            };

            var creationResult = await userManager.CreateAsync(user, password);

            if (!creationResult.Succeeded)
            {
                var errors = string.Join("; ", creationResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not create the default admin user: {errors}");
            }
        }

        if (await userManager.IsInRoleAsync(user, IdentityRoles.Admin))
        {
            return;
        }

        var roleResult = await userManager.AddToRoleAsync(user, IdentityRoles.Admin);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join("; ", roleResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Could not assign the Admin role: {errors}");
        }
    }
}
