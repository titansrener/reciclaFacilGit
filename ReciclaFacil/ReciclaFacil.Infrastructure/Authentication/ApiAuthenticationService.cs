using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ReciclaFacil.Application.Authentication;
using ReciclaFacil.Infrastructure.Data;

namespace ReciclaFacil.Infrastructure.Authentication;

public sealed class ApiAuthenticationService(
    ReciclaFacilDbContext database,
    IPasswordHasher<LegacyUser> passwordHasher,
    IOptions<JwtOptions> jwtOptions) : IApiAuthenticationService
{
    private static readonly IReadOnlyDictionary<string, string> Roles =
        new Dictionary<string, string>
        {
            ["0"] = "Admin", ["1"] = "Cliente", ["2"] = "Cooperativa",
            ["3"] = "Empresa", ["4"] = "Funcionario"
        };
    private readonly JwtOptions _options = jwtOptions.Value;

    public async Task<TokenPair?> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = command.Email.Trim();
        var user = await database.Users.Include(x => x.UserRoles)
            .SingleOrDefaultAsync(
                x => x.UserName == email || x.Email == email,
                cancellationToken);
        if (user is null || user.Ativo == false || string.IsNullOrWhiteSpace(user.PasswordHash))
            return null;
        if (user.LockoutEnabled && user.LockoutEndDateUtc > DateTime.UtcNow)
            return null;
        if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, command.Password)
            == PasswordVerificationResult.Failed)
            return null;
        return await IssueAsync(user, cancellationToken);
    }

    public async Task<TokenPair?> RefreshAsync(
        RefreshCommand command,
        CancellationToken cancellationToken = default)
    {
        var hash = Hash(command.RefreshToken);
        var current = await database.RefreshTokens
            .Include(x => x.User).ThenInclude(x => x!.UserRoles)
            .SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        if (current?.User is null || current.RevokedAtUtc is not null ||
            current.ExpiresAtUtc <= DateTime.UtcNow || current.User.Ativo == false)
            return null;

        current.RevokedAtUtc = DateTime.UtcNow;
        var pair = await IssueAsync(current.User, cancellationToken, saveChanges: false);
        current.ReplacedByHash = Hash(pair.RefreshToken);
        await database.SaveChangesAsync(cancellationToken);
        return pair;
    }

    public async Task<bool> RevokeAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var hash = Hash(refreshToken);
        var token = await database.RefreshTokens
            .SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        if (token is null || token.RevokedAtUtc is not null)
            return false;
        token.RevokedAtUtc = DateTime.UtcNow;
        await database.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<TokenPair> IssueAsync(
        LegacyUser user,
        CancellationToken cancellationToken,
        bool saveChanges = true)
    {
        var now = DateTimeOffset.UtcNow;
        var accessExpires = now.AddMinutes(_options.AccessTokenMinutes);
        var refreshExpires = now.AddDays(_options.RefreshTokenDays);
        var roleId = user.UserRoles.Select(x => x.RoleId).FirstOrDefault();
        var role = roleId is not null && Roles.TryGetValue(roleId, out var name)
            ? name : "Usuario";
        var authenticatedUser = new AuthenticatedUser(
            user.Id, user.Email ?? user.UserName, role);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, authenticatedUser.Email),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            _options.Issuer, _options.Audience, claims,
            notBefore: now.UtcDateTime,
            expires: accessExpires.UtcDateTime,
            signingCredentials: credentials);
        var rawRefreshToken = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        database.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = Hash(rawRefreshToken),
            CreatedAtUtc = now.UtcDateTime,
            ExpiresAtUtc = refreshExpires.UtcDateTime
        });
        if (saveChanges)
            await database.SaveChangesAsync(cancellationToken);
        return new(
            new JwtSecurityTokenHandler().WriteToken(jwt),
            accessExpires,
            rawRefreshToken,
            refreshExpires,
            authenticatedUser);
    }

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
