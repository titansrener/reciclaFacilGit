using NetTopologySuite.Geometries;

namespace ReciclaFacil.Infrastructure.Data;

public sealed class LegacyUser
{
    public required string Id { get; set; }
    public string? Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PasswordHash { get; set; }
    public string? SecurityStamp { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LockoutEndDateUtc { get; set; }
    public bool LockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }
    public required string UserName { get; set; }
    public DateTime? DataCadastro { get; set; }
    public bool? Ativo { get; set; }
    public required string Discriminator { get; set; }
    public ICollection<LegacyUserRole> UserRoles { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}

public sealed class RefreshToken
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public required string TokenHash { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByHash { get; set; }
    public LegacyUser? User { get; set; }
}

public sealed class LegacyRole
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public ICollection<LegacyUserRole> UserRoles { get; set; } = [];
}

public sealed class LegacyUserRole
{
    public required string UserId { get; set; }
    public required string RoleId { get; set; }
    public string? IdentityUserId { get; set; }
    public LegacyUser? User { get; set; }
    public LegacyRole? Role { get; set; }
}

public sealed class Material
{
    public int Id { get; set; }
    public required string Descricao { get; set; }
    public int TempoMedioDecomposicao { get; set; }
    public bool Selecionado { get; set; }
}

public sealed class Cooperativa
{
    public required string Id { get; set; }
    public required string Cnpj { get; set; }
    public required string RazaoSocial { get; set; }
    public required string Endereco { get; set; }
    public required string Cidade { get; set; }
    public required string Estado { get; set; }
    public Point? EnderecoCoordenada { get; set; }
    public ICollection<MaterialComercializado> MateriaisComercializados { get; set; } = [];
    public ICollection<Caminhao> Caminhoes { get; set; } = [];
    public ICollection<Funcionario> Funcionarios { get; set; } = [];
}

public sealed class MaterialComercializado
{
    public int MaterialId { get; set; }
    public required string CooperativaId { get; set; }
    public decimal? ValorRevenda { get; set; }
    public Material? Material { get; set; }
    public Cooperativa? Cooperativa { get; set; }
}
