using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReciclaFacil.Application.Authentication;

namespace ReciclaFacil.Infrastructure.Authentication;

public sealed class PasswordResetNotifier(
    IOptions<SmtpOptions> smtpOptions,
    IOptions<PasswordResetOptions> resetOptions,
    ILogger<PasswordResetNotifier> logger) : IPasswordResetNotifier
{
    private readonly SmtpOptions _smtp = smtpOptions.Value;
    private readonly PasswordResetOptions _reset = resetOptions.Value;

    public async Task SendAsync(
        string email,
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_smtp.Host) ||
            string.IsNullOrWhiteSpace(_smtp.FromAddress))
        {
            logger.LogWarning(
                "SMTP não configurado; a solicitação de redefinição para {Email} não foi enviada.",
                email);
            return;
        }

        var baseUrl = _reset.FrontendBaseUrl.TrimEnd('/');
        var link = $"{baseUrl}/?resetToken={Uri.EscapeDataString(token)}";
        using var message = new MailMessage
        {
            From = new MailAddress(_smtp.FromAddress, _smtp.FromName),
            Subject = "Redefinição de senha — Recicla Fácil",
            Body = "Recebemos uma solicitação para redefinir sua senha. " +
                   $"Acesse o link abaixo em até {_reset.TokenLifetimeMinutes} minutos:\n\n{link}\n\n" +
                   "Se você não solicitou esta alteração, ignore esta mensagem.",
            IsBodyHtml = false
        };
        message.To.Add(email);
        using var client = new SmtpClient(_smtp.Host, _smtp.Port)
        {
            EnableSsl = _smtp.EnableSsl
        };
        if (!string.IsNullOrWhiteSpace(_smtp.UserName))
            client.Credentials = new NetworkCredential(_smtp.UserName, _smtp.Password);
        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
    }
}
