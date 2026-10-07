using Microsoft.AspNetCore.Identity;
using SafeVaultApi.Contracts;

namespace SafeVaultApi.Services;

public interface ITokenService
{
    LoginResponse CreateLoginResponse(IdentityUser user, IReadOnlyList<string> roles);
}
