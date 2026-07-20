using Microsoft.EntityFrameworkCore;

namespace ReciclaFacil.Infrastructure.Data;

public sealed class ReciclaFacilDbContext(DbContextOptions<ReciclaFacilDbContext> options)
    : DbContext(options)
{
    public DbSet<LegacyUser> Users => Set<LegacyUser>();
    public DbSet<LegacyRole> Roles => Set<LegacyRole>();
    public DbSet<LegacyUserRole> UserRoles => Set<LegacyUserRole>();
    public DbSet<Material> Materiais => Set<Material>();
    public DbSet<Cooperativa> Cooperativas => Set<Cooperativa>();
    public DbSet<MaterialComercializado> MateriaisComercializados => Set<MaterialComercializado>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Coleta> Coletas => Set<Coleta>();
    public DbSet<ClienteColeta> ClientesColetas => Set<ClienteColeta>();
    public DbSet<CarteiraMovimento> Carteiras => Set<CarteiraMovimento>();
    public DbSet<MaterialColetado> MateriaisColetados => Set<MaterialColetado>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Caminhao> Caminhoes => Set<Caminhao>();
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<CaminhaoColeta> CaminhoesColetas => Set<CaminhaoColeta>();
    public DbSet<FuncionarioColeta> FuncionariosColetas => Set<FuncionarioColeta>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

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

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserId).HasMaxLength(128);
            entity.Property(x => x.TokenHash).HasMaxLength(64).IsUnicode(false);
            entity.Property(x => x.ReplacedByHash).HasMaxLength(64).IsUnicode(false);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasOne(x => x.User).WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("PasswordResetTokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserId).HasMaxLength(128);
            entity.Property(x => x.TokenHash).HasMaxLength(64).IsUnicode(false);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasOne(x => x.User).WithMany(x => x.PasswordResetTokens)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
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

        modelBuilder.Entity<Cooperativa>(entity =>
        {
            entity.ToTable("Cooperativas");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("cooperativaId").HasMaxLength(128);
            entity.Property(x => x.Cnpj).HasColumnName("cnpj").HasMaxLength(14).IsUnicode(false);
            entity.Property(x => x.RazaoSocial).HasColumnName("razaoSocial").HasMaxLength(100).IsUnicode(false);
            entity.Property(x => x.Endereco).HasColumnName("endereco").HasMaxLength(150).IsUnicode(false);
            entity.Property(x => x.Cidade).HasColumnName("cidade").HasMaxLength(80).IsUnicode(false);
            entity.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(2).IsUnicode(false);
            entity.Property(x => x.EnderecoCoordenada).HasColumnName("enderecoCoordenada").HasColumnType("geometry");
        });

        modelBuilder.Entity<MaterialComercializado>(entity =>
        {
            entity.ToTable("MateriaisComercializados");
            entity.HasKey(x => new { x.MaterialId, x.CooperativaId });
            entity.Property(x => x.MaterialId).HasColumnName("materialId");
            entity.Property(x => x.CooperativaId).HasColumnName("cooperativaId").HasMaxLength(128);
            entity.Property(x => x.ValorRevenda).HasColumnName("valorRevenda").HasPrecision(8, 2);
            entity.HasOne(x => x.Material).WithMany()
                .HasForeignKey(x => x.MaterialId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.Cooperativa).WithMany(x => x.MateriaisComercializados)
                .HasForeignKey(x => x.CooperativaId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("clienteId").HasMaxLength(128);
            entity.Property(x => x.Cpf).HasColumnName("cpf").HasMaxLength(11).IsUnicode(false);
            entity.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(1).IsFixedLength().IsUnicode(false);
            entity.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(75).IsUnicode(false);
            entity.Property(x => x.Endereco).HasColumnName("endereco").HasMaxLength(100).IsUnicode(false);
            entity.Property(x => x.EnderecoCoordenada).HasColumnName("enderecoCoordenada").HasColumnType("geometry");
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(45).IsUnicode(false);
            entity.Property(x => x.Sexo).HasColumnName("sexo").HasMaxLength(1).IsFixedLength().IsUnicode(false);
            entity.Property(x => x.DataNascimento).HasColumnName("dataNascimento");
            entity.Property(x => x.Telefone).HasColumnName("telefone").HasMaxLength(11).IsUnicode(false);
            entity.Property(x => x.Celular).HasColumnName("celular").HasMaxLength(11).IsUnicode(false);
            entity.Property(x => x.CooperativaId).HasColumnName("cooperativaId").HasMaxLength(128);
            entity.HasOne(x => x.Cooperativa).WithMany()
                .HasForeignKey(x => x.CooperativaId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Coleta>(entity =>
        {
            entity.ToTable("Coletas");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("coletaId");
            entity.Property(x => x.HoraAgendada).HasColumnName("horaAgendada");
            entity.Property(x => x.Quantidade).HasColumnName("quantidade");
            entity.Property(x => x.Status).HasColumnName("coletado").HasMaxLength(1).IsFixedLength().IsUnicode(false);
            entity.Property(x => x.CooperativaId).HasColumnName("cooperativaId").HasMaxLength(128);
            entity.HasOne(x => x.Cooperativa).WithMany()
                .HasForeignKey(x => x.CooperativaId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ClienteColeta>(entity =>
        {
            entity.ToTable("ClientesColetas");
            entity.HasKey(x => new { x.ColetaId, x.ClienteId });
            entity.Property(x => x.ColetaId).HasColumnName("coletaId");
            entity.Property(x => x.ClienteId).HasColumnName("clienteId").HasMaxLength(128);
            entity.Property(x => x.HoraDaColeta).HasColumnName("horaDaColeta");
            entity.Property(x => x.Status).HasColumnName("coletado").HasMaxLength(1).IsFixedLength().IsUnicode(false);
            entity.HasOne(x => x.Cliente).WithMany(x => x.Coletas)
                .HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.Coleta).WithMany(x => x.Clientes)
                .HasForeignKey(x => x.ColetaId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<CarteiraMovimento>(entity =>
        {
            entity.ToTable("Carteiras");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("carteiraId");
            entity.Property(x => x.Saldo).HasColumnName("saldo").HasPrecision(8, 2);
            entity.Property(x => x.DataUltimaMovimentacao).HasColumnName("dataUltimaMovimentacao");
            entity.Property(x => x.ClienteId).HasColumnName("clienteId").HasMaxLength(128);
            entity.HasOne(x => x.Cliente).WithMany(x => x.Carteira)
                .HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<MaterialColetado>(entity =>
        {
            entity.ToTable("MateriaisColetados");
            entity.HasKey(x => new { x.MaterialId, x.ColetaId, x.ClienteId });
            entity.Property(x => x.MaterialId).HasColumnName("materialId");
            entity.Property(x => x.ColetaId).HasColumnName("coletaId");
            entity.Property(x => x.ClienteId).HasColumnName("clienteId").HasMaxLength(128);
            entity.Property(x => x.Quantidade).HasColumnName("quantidade");
            entity.Property(x => x.ValorCompra).HasColumnName("valorCompra").HasPrecision(18, 2);
            entity.Property(x => x.Status).HasColumnName("coletado").HasMaxLength(1).IsFixedLength().IsUnicode(false);
            entity.HasOne(x => x.Material).WithMany()
                .HasForeignKey(x => x.MaterialId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.ClienteColeta).WithMany(x => x.Materiais)
                .HasForeignKey(x => new { x.ColetaId, x.ClienteId }).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Notificacao>(entity =>
        {
            entity.ToTable("Notificacoes");
            entity.HasKey(x => new { x.Id, x.ClienteId });
            entity.Property(x => x.Id).HasColumnName("notificacaoId").ValueGeneratedOnAdd();
            entity.Property(x => x.ClienteId).HasColumnName("clienteId").HasMaxLength(128);
            entity.Property(x => x.ColetaId).HasColumnName("coletaId");
            entity.Property(x => x.CooperativaId).HasColumnName("cooperativaId").HasMaxLength(128);
            entity.Property(x => x.DataHorario).HasColumnName("dataHorario");
            entity.Property(x => x.Ativa).HasColumnName("ativa");
            entity.Property(x => x.Descricao).HasColumnName("descricao").HasMaxLength(150).IsUnicode(false);
            entity.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(1).IsFixedLength().IsUnicode(false);
            entity.HasOne(x => x.Cliente).WithMany(x => x.Notificacoes)
                .HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Caminhao>(entity =>
        {
            entity.ToTable("Caminhoes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("caminhaoId");
            entity.Property(x => x.Descricao).HasColumnName("descricao").HasMaxLength(45).IsUnicode(false);
            entity.Property(x => x.Placa).HasColumnName("placa").HasMaxLength(8).IsUnicode(false);
            entity.Property(x => x.CooperativaId).HasColumnName("cooperativaId").HasMaxLength(128);
            entity.HasIndex(x => x.Placa).IsUnique();
            entity.HasOne(x => x.Cooperativa).WithMany(x => x.Caminhoes)
                .HasForeignKey(x => x.CooperativaId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Funcionario>(entity =>
        {
            entity.ToTable("Funcionarios");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("funcionarioId").HasMaxLength(128);
            entity.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(45).IsUnicode(false);
            entity.Property(x => x.DataNascimento).HasColumnName("dataNascimento").HasColumnType("date");
            entity.Property(x => x.CooperativaId).HasColumnName("cooperativaId").HasMaxLength(128);
            entity.HasOne(x => x.Cooperativa).WithMany(x => x.Funcionarios)
                .HasForeignKey(x => x.CooperativaId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.Usuario).WithOne()
                .HasForeignKey<Funcionario>(x => x.Id).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<CaminhaoColeta>(entity =>
        {
            entity.ToTable("CaminhoesColetas");
            entity.HasKey(x => new { x.CaminhaoId, x.ColetaId });
            entity.Property(x => x.CaminhaoId).HasColumnName("caminhaoId");
            entity.Property(x => x.ColetaId).HasColumnName("coletaId");
            entity.HasOne(x => x.Caminhao).WithMany(x => x.Coletas)
                .HasForeignKey(x => x.CaminhaoId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.Coleta).WithMany(x => x.Caminhoes)
                .HasForeignKey(x => x.ColetaId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<FuncionarioColeta>(entity =>
        {
            entity.ToTable("FuncionariosColetas");
            entity.HasKey(x => new { x.ColetaId, x.FuncionarioId });
            entity.Property(x => x.ColetaId).HasColumnName("coletaId");
            entity.Property(x => x.FuncionarioId).HasColumnName("funcionarioId").HasMaxLength(128);
            entity.HasOne(x => x.Coleta).WithMany(x => x.Funcionarios)
                .HasForeignKey(x => x.ColetaId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.Funcionario).WithMany(x => x.Coletas)
                .HasForeignKey(x => x.FuncionarioId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.ToTable("Empresas");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("empresaId").HasMaxLength(128);
            entity.Property(x => x.Cnpj).HasColumnName("cnpj").HasMaxLength(14).IsUnicode(false);
            entity.Property(x => x.RazaoSocial).HasColumnName("razaoSocial").HasMaxLength(150).IsUnicode(false);
            entity.Property(x => x.Endereco).HasColumnName("endereco").HasMaxLength(150).IsUnicode(false);
            entity.Property(x => x.EnderecoCoordenada).HasColumnName("enderecoCoordenada").HasColumnType("geometry");
            entity.Property(x => x.Telefone).HasColumnName("telefone").HasMaxLength(11).IsUnicode(false);
            entity.Property(x => x.Fax).HasColumnName("fax").HasMaxLength(25).IsUnicode(false);
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(45).IsUnicode(false);
        });
    }
}
