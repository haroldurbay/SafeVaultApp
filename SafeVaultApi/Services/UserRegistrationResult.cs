using Microsoft.AspNetCore.Identity;
using SafeVaultApi.Contracts;

namespace SafeVaultApi.Services;

public sealed record UserRegistrationResult(
    UserRegistrationResponse? User,
    IReadOnlyCollection<IdentityError> Errors);
