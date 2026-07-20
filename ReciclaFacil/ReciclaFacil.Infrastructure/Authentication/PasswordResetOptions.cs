namespace ReciclaFacil.Infrastructure.Authentication;

public sealed class PasswordResetOptions
{
    public const string SectionName = "PasswordReset";
    public int TokenLifetimeMinutes { get; set; } = 60;
    public bool ExposeTokenInDevelopment { get; set; }
    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
}

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromAddress { get; set; } = "";
    public string FromName { get; set; } = "Recicla Fácil";
}
