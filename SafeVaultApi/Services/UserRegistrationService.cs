using Microsoft.AspNetCore.Identity;
using SafeVaultApi.Contracts;
using SafeVaultApi.Identity;

namespace SafeVaultApi.Services;

public sealed class UserRegistrationService(UserManager<IdentityUser> userManager)
    : IUserRegistrationService
{
    public async Task<UserRegistrationResult> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var username = request.Username.Trim();
        var email = request.Email.Trim();
        var user = new IdentityUser
        {
            UserName = username,
            Email = email
        };

        var result = await userManager.CreateAsync(user, request.Password);
        cancellationToken.ThrowIfCancellationRequested();

        if (!result.Succeeded)
        {
            return new UserRegistrationResult(null, result.Errors.ToArray());
        }

        var roleResult = await userManager.AddToRoleAsync(user, IdentityRoles.Users);
        cancellationToken.ThrowIfCancellationRequested();

        if (!roleResult.Succeeded)
        {
            var errors = string.Join("; ", roleResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Could not assign the default role to the registered user: {errors}");
        }

        return new UserRegistrationResult(
            new UserRegistrationResponse(user.Id, user.UserName!, user.Email!),
            []);
    }
}
