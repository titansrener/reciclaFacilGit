using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ReciclaFacil.Core.Data;

namespace ReciclaFacil.Core.Services;

public sealed class LegacyAuthenticationService(ReciclaFacilDbContext database)
    : ILegacyAuthenticationService
{
    private static readonly IReadOnlyDictionary<string, string> LegacyRoles =
        new Dictionary<string, string>
        {
            ["0"] = "Admin", ["1"] = "Cliente", ["2"] = "Cooperativa",
            ["3"] = "Empresa", ["4"] = "Funcionario"
        };

    private readonly PasswordHasher<LegacyUser> _passwordHasher =
        new(Options.Create(new PasswordHasherOptions
        {
            CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2
        }));

    public async Task<LegacyAuthenticationResult> ValidateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim();
        var user = await database.Users.AsNoTracking()
            .Include(x => x.UserRoles)
            .SingleOrDefaultAsync(
                x => x.UserName == normalizedEmail || x.Email == normalizedEmail,
                cancellationToken);

        if (user is null || user.Ativo == false || string.IsNullOrWhiteSpace(user.PasswordHash))
            return new(false, Error: "E-mail ou senha inválidos.");

        if (user.LockoutEnabled && user.LockoutEndDateUtc > DateTime.UtcNow)
            return new(false, Error: "A conta está temporariamente bloqueada.");

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
            return new(false, Error: "E-mail ou senha inválidos.");

        var roleId = user.UserRoles.Select(x => x.RoleId).FirstOrDefault();
        var role = roleId is not null && LegacyRoles.TryGetValue(roleId, out var mappedRole)
            ? mappedRole
            : "Usuario";
        return new(true, user, role);
    }
}
