using SafeVaultApi.Contracts;

namespace SafeVaultApi.Services;

public interface IUserAuthenticationService
{
    Task<LoginResponse?> LoginAsync(LoginUserRequest request, CancellationToken cancellationToken);

    Task<AuthenticatedUserResponse?> GetUserAsync(string userId, CancellationToken cancellationToken);
}
