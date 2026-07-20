namespace ReciclaFacil.Application.Authentication;

public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword);

public sealed record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string ConfirmPassword);

public sealed record PasswordResetRequestResult(string? DeliveryToken);

public enum CredentialError
{
    None,
    Validation,
    UserNotFound,
    InvalidCurrentPassword,
    InvalidOrExpiredToken
}

public sealed record CredentialResult(
    CredentialError Error = CredentialError.None,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null)
{
    public bool Succeeded => Error == CredentialError.None;
}

public interface ICredentialService
{
    Task<PasswordResetRequestResult> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<CredentialResult> ResetPasswordAsync(
        ResetPasswordCommand command,
        CancellationToken cancellationToken = default);

    Task<CredentialResult> ChangePasswordAsync(
        string userId,
        ChangePasswordCommand command,
        CancellationToken cancellationToken = default);
}

public interface IPasswordResetNotifier
{
    Task SendAsync(
        string email,
        string token,
        CancellationToken cancellationToken = default);
}
