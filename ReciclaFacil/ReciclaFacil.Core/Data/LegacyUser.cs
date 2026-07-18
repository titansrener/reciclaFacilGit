namespace ReciclaFacil.Core.Data;

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
