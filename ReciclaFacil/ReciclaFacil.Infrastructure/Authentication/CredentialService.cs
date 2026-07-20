using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReciclaFacil.Application.Authentication;
using ReciclaFacil.Infrastructure.Data;

namespace ReciclaFacil.Infrastructure.Authentication;

public sealed class CredentialService(
    ReciclaFacilDbContext database,
    IPasswordHasher<LegacyUser> passwordHasher,
    IPasswordResetNotifier notifier,
    IOptions<PasswordResetOptions> options,
    ILogger<CredentialService> logger) : ICredentialService
{
    private readonly PasswordResetOptions _options = options.Value;

    public async Task<PasswordResetRequestResult> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalized = email?.Trim().ToLowerInvariant() ?? "";
        var user = await database.Users.SingleOrDefaultAsync(
            x => x.Ativo != false &&
                 (x.Email == normalized || x.UserName == normalized),
            cancellationToken);
        if (user?.Email is null)
            return new(null);

        var now = DateTime.UtcNow;
        var activeTokens = await database.PasswordResetTokens
            .Where(x => x.UserId == user.Id &&
                        x.UsedAtUtc == null &&
                        x.ExpiresAtUtc > now)
            .ToListAsync(cancellationToken);
        foreach (var active in activeTokens)
            active.UsedAtUtc = now;

        var rawToken = Base64Url(RandomNumberGenerator.GetBytes(48));
        database.PasswordResetTokens.Add(new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = Hash(rawToken),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddMinutes(
                Math.Clamp(_options.TokenLifetimeMinutes, 5, 1440))
        });
        await database.SaveChangesAsync(cancellationToken);

        try
        {
            await notifier.SendAsync(user.Email, rawToken, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception,
                "Falha ao enviar redefinição de senha para o usuário {UserId}.", user.Id);
        }
        return new(rawToken);
    }

    public async Task<CredentialResult> ResetPasswordAsync(
        ResetPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateNewPassword(command.NewPassword, command.ConfirmPassword);
        if (string.IsNullOrWhiteSpace(command.Token))
            errors["token"] = ["Token de redefinição é obrigatório."];
        if (errors.Count > 0)
            return Invalid(errors);

        await using var transaction = await database.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var now = DateTime.UtcNow;
        var tokenHash = Hash(command.Token);
        var resetToken = await database.PasswordResetTokens
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
        if (resetToken?.User is null ||
            resetToken.UsedAtUtc is not null ||
            resetToken.ExpiresAtUtc <= now ||
            resetToken.User.Ativo == false)
            return new(CredentialError.InvalidOrExpiredToken);

        resetToken.User.PasswordHash = passwordHasher.HashPassword(
            resetToken.User, command.NewPassword);
        resetToken.User.SecurityStamp = Guid.NewGuid().ToString();
        resetToken.UsedAtUtc = now;
        await RevokeSessionsAsync(resetToken.UserId, now, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new();
    }

    public async Task<CredentialResult> ChangePasswordAsync(
        string userId,
        ChangePasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateNewPassword(command.NewPassword, command.ConfirmPassword);
        if (string.IsNullOrWhiteSpace(command.CurrentPassword))
            errors["currentPassword"] = ["Informe a senha atual."];
        if (errors.Count > 0)
            return Invalid(errors);

        await using var transaction = await database.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var user = await database.Users.SingleOrDefaultAsync(
            x => x.Id == userId && x.Ativo != false, cancellationToken);
        if (user is null)
            return new(CredentialError.UserNotFound);
        if (string.IsNullOrWhiteSpace(user.PasswordHash) ||
            passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, command.CurrentPassword) ==
            PasswordVerificationResult.Failed)
            return new(CredentialError.InvalidCurrentPassword);

        user.PasswordHash = passwordHasher.HashPassword(user, command.NewPassword);
        user.SecurityStamp = Guid.NewGuid().ToString();
        await RevokeSessionsAsync(userId, DateTime.UtcNow, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new();
    }

    private async Task RevokeSessionsAsync(
        string userId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var sessions = await database.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);
        foreach (var session in sessions)
            session.RevokedAtUtc = now;
    }

    private static Dictionary<string, string[]> ValidateNewPassword(
        string password,
        string confirmation)
    {
        var errors = new Dictionary<string, string[]>();
        if ((password?.Length ?? 0) is < 8 or > 100)
            errors["newPassword"] = ["A nova senha deve ter entre 8 e 100 caracteres."];
        if (!string.Equals(password, confirmation, StringComparison.Ordinal))
            errors["confirmPassword"] = ["As senhas não conferem."];
        return errors;
    }

    private static CredentialResult Invalid(IReadOnlyDictionary<string, string[]> errors) =>
        new(CredentialError.Validation, errors);

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static string Base64Url(byte[] value) =>
        Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
