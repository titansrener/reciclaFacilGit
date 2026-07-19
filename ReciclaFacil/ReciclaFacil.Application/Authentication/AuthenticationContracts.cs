namespace ReciclaFacil.Application.Authentication;

public sealed record LoginCommand(string Email, string Password);
public sealed record RefreshCommand(string RefreshToken);

public sealed record TokenPair(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    AuthenticatedUser User);

public sealed record AuthenticatedUser(
    string Id,
    string Email,
    string Role);

public interface IApiAuthenticationService
{
    Task<TokenPair?> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default);

    Task<TokenPair?> RefreshAsync(
        RefreshCommand command,
        CancellationToken cancellationToken = default);

    Task<bool> RevokeAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}
