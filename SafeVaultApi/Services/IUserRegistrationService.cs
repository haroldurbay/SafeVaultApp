using SafeVaultApi.Contracts;

namespace SafeVaultApi.Services;

public interface IUserRegistrationService
{
    Task<UserRegistrationResult> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken);
}
