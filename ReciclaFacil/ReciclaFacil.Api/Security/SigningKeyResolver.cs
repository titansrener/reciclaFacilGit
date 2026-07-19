using System.Security.Cryptography;

namespace ReciclaFacil.Api.Security;

public static class SigningKeyResolver
{
    public static string Resolve(
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var configured = configuration["Jwt:SigningKey"];
        if (!string.IsNullOrWhiteSpace(configured) && configured.Length >= 32)
            return configured;
        if (!environment.IsDevelopment())
            throw new InvalidOperationException(
                "Configure Jwt__SigningKey com pelo menos 32 caracteres.");

        var directory = Path.Combine(environment.ContentRootPath, ".keys");
        var file = Path.Combine(directory, "jwt-signing-key.txt");
        if (File.Exists(file))
            return File.ReadAllText(file).Trim();

        Directory.CreateDirectory(directory);
        var generated = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        File.WriteAllText(file, generated);
        return generated;
    }
}
