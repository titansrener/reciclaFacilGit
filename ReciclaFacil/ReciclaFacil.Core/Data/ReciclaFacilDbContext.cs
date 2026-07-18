using Microsoft.EntityFrameworkCore;

namespace ReciclaFacil.Core.Data;

public sealed class ReciclaFacilDbContext(DbContextOptions<ReciclaFacilDbContext> options)
    : DbContext(options)
{
    public DbSet<LegacyUser> Users => Set<LegacyUser>();
    public DbSet<LegacyRole> Roles => Set<LegacyRole>();
    public DbSet<LegacyUserRole> UserRoles => Set<LegacyUserRole>();
    public DbSet<Material> Materiais => Set<Material>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LegacyUser>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("usuarioId").HasMaxLength(128);
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.UserName).HasMaxLength(256);
            entity.Property(x => x.DataCadastro).HasColumnName("dataCadastro");
            entity.Property(x => x.Ativo).HasColumnName("ativo");
            entity.Property(x => x.Discriminator).HasMaxLength(128);
        });

        modelBuilder.Entity<LegacyRole>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(128);
            entity.Property(x => x.Name).HasMaxLength(256);
        });

        modelBuilder.Entity<LegacyUserRole>(entity =>
        {
            entity.ToTable("UsuarioRole");
            entity.HasKey(x => new { x.UserId, x.RoleId });
            entity.Property(x => x.UserId).HasMaxLength(128);
            entity.Property(x => x.RoleId).HasMaxLength(128);
            entity.Property(x => x.IdentityUserId).HasColumnName("IdentityUser_Id").HasMaxLength(128);
            entity.HasOne(x => x.Role).WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.User).WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.ToTable("Materiais");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("materialId");
            entity.Property(x => x.Descricao).HasColumnName("descricao").HasMaxLength(50).IsUnicode(false);
            entity.Property(x => x.TempoMedioDecomposicao).HasColumnName("tempoMedioDecomposicao");
            entity.Property(x => x.Selecionado).HasColumnName("selecionado");
        });
    }
}
