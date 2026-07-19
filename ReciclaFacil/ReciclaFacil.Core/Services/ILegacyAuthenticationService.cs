using ReciclaFacil.Infrastructure.Data;

namespace ReciclaFacil.Core.Services;

public interface ILegacyAuthenticationService
{
    Task<LegacyAuthenticationResult> ValidateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}

public sealed record LegacyAuthenticationResult(
    bool Succeeded,
    LegacyUser? User = null,
    string? Role = null,
    string? Error = null);
