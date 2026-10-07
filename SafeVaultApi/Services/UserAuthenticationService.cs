using Microsoft.AspNetCore.Identity;
using SafeVaultApi.Contracts;

namespace SafeVaultApi.Services;

public sealed class UserAuthenticationService(
    UserManager<IdentityUser> userManager,
    ITokenService tokenService) : IUserAuthenticationService
{
    public async Task<LoginResponse?> LoginAsync(
        LoginUserRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByNameAsync(request.Username);

        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        cancellationToken.ThrowIfCancellationRequested();
        return tokenService.CreateLoginResponse(user, roles.ToArray());
    }

    public async Task<AuthenticatedUserResponse?> GetUserAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        cancellationToken.ThrowIfCancellationRequested();

        return new AuthenticatedUserResponse(
            user.Id,
            user.UserName!,
            user.Email!,
            roles.ToArray());
    }
}
