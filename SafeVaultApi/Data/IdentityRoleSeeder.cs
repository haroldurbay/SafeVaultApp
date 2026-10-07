using Microsoft.AspNetCore.Identity;
using SafeVaultApi.Identity;

namespace SafeVaultApi.Data;

public static class IdentityRoleSeeder
{
    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var roleName in new[] { IdentityRoles.Admin, IdentityRoles.Users })
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not create the {roleName} role: {errors}");
            }
        }
    }
}
