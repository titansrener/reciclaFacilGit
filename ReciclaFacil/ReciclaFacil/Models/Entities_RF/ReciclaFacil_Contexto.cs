namespace ReciclaFacil.Models.Entities_RF
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ReciclaFacil_Contexto : DbContext
    {
        public ReciclaFacil_Contexto()
            : base("name=ReciclaFacil_Contexto")
        {
        }

        public virtual DbSet<C__MigrationHistory> C__MigrationHistory { get; set; }
        public virtual DbSet<Caminhoes> Caminhoes { get; set; }
        public virtual DbSet<Carteiras> Carteiras { get; set; }
        public virtual DbSet<Claims> Claims { get; set; }
        public virtual DbSet<Clientes> Clientes { get; set; }
        public virtual DbSet<ClientesColetas> ClientesColetas { get; set; }
        public virtual DbSet<Coletas> Coletas { get; set; }
        public virtual DbSet<Cooperativas> Cooperativas { get; set; }
        public virtual DbSet<Dicas> Dicas { get; set; }
        public virtual DbSet<Empresas> Empresas { get; set; }
        public virtual DbSet<Funcionarios> Funcionarios { get; set; }
        public virtual DbSet<ItensPedidos> ItensPedidos { get; set; }
        public virtual DbSet<Logins> Logins { get; set; }
        public virtual DbSet<Materiais> Materiais { get; set; }
        public virtual DbSet<MateriaisColetados> MateriaisColetados { get; set; }
        public virtual DbSet<MateriaisComercializados> MateriaisComercializados { get; set; }
        public virtual DbSet<Notificacoes> Notificacoes { get; set; }
        public virtual DbSet<Pedidos> Pedidos { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<Rotas> Rotas { get; set; }
        public virtual DbSet<UsuarioRole> UsuarioRole { get; set; }
        public virtual DbSet<Usuarios> Usuarios { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Caminhoes>()
                .Property(e => e.descricao)
                .IsUnicode(false);

            modelBuilder.Entity<Caminhoes>()
                .Property(e => e.placa)
                .IsUnicode(false);

            modelBuilder.Entity<Caminhoes>()
                .HasMany(e => e.Coletas)
                .WithMany(e => e.Caminhoes)
                .Map(m => m.ToTable("CaminhoesColetas").MapLeftKey("caminhaoId").MapRightKey("coletaId"));

            modelBuilder.Entity<Carteiras>()
                .Property(e => e.saldo)
                .HasPrecision(8, 2);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.cpf)
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.tipo)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.nome)
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.endereco)
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.email)
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.sexo)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.telefone)
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.celular)
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .HasMany(e => e.Carteiras)
                .WithRequired(e => e.Clientes)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Clientes>()
                .HasMany(e => e.ClientesColetas)
                .WithRequired(e => e.Clientes)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Clientes>()
                .HasMany(e => e.Notificacoes)
                .WithRequired(e => e.Clientes)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Clientes>()
                .HasMany(e => e.Dicas)
                .WithMany(e => e.Clientes)
                .Map(m => m.ToTable("DicasClientes").MapLeftKey("clienteId").MapRightKey("dicaId"));

            modelBuilder.Entity<ClientesColetas>()
                .Property(e => e.coletado)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<ClientesColetas>()
                .HasMany(e => e.MateriaisColetados)
                .WithRequired(e => e.ClientesColetas)
                .HasForeignKey(e => new { e.coletaId, e.clienteId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Coletas>()
                .Property(e => e.coletado)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Coletas>()
                .HasMany(e => e.ClientesColetas)
                .WithRequired(e => e.Coletas)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Coletas>()
                .HasMany(e => e.Notificacoes)
                .WithRequired(e => e.Coletas)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Coletas>()
                .HasMany(e => e.Rotas)
                .WithRequired(e => e.Coletas)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Coletas>()
                .HasMany(e => e.Funcionarios)
                .WithMany(e => e.Coletas)
                .Map(m => m.ToTable("FuncionariosColetas").MapLeftKey("coletaId").MapRightKey("funcionarioId"));

            modelBuilder.Entity<Cooperativas>()
                .Property(e => e.cnpj)
                .IsUnicode(false);

            modelBuilder.Entity<Cooperativas>()
                .Property(e => e.razaoSocial)
                .IsUnicode(false);

            modelBuilder.Entity<Cooperativas>()
                .Property(e => e.endereco)
                .IsUnicode(false);

            modelBuilder.Entity<Cooperativas>()
                .Property(e => e.cidade)
                .IsUnicode(false);

            modelBuilder.Entity<Cooperativas>()
                .Property(e => e.estado)
                .IsUnicode(false);

            modelBuilder.Entity<Cooperativas>()
                .HasMany(e => e.Caminhoes)
                .WithRequired(e => e.Cooperativas)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Cooperativas>()
                .HasMany(e => e.Clientes)
                .WithRequired(e => e.Cooperativas)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Cooperativas>()
                .HasMany(e => e.Coletas)
                .WithRequired(e => e.Cooperativas)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Cooperativas>()
                .HasMany(e => e.Funcionarios)
                .WithRequired(e => e.Cooperativas)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Cooperativas>()
                .HasMany(e => e.MateriaisComercializados)
                .WithRequired(e => e.Cooperativas)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Cooperativas>()
                .HasMany(e => e.Notificacoes)
                .WithRequired(e => e.Cooperativas)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Dicas>()
                .Property(e => e.dicaEmTexto)
                .IsUnicode(false);

            modelBuilder.Entity<Empresas>()
                .Property(e => e.cnpj)
                .IsUnicode(false);

            modelBuilder.Entity<Empresas>()
                .Property(e => e.razaoSocial)
                .IsUnicode(false);

            modelBuilder.Entity<Empresas>()
                .Property(e => e.endereco)
                .IsUnicode(false);

            modelBuilder.Entity<Empresas>()
                .Property(e => e.telefone)
                .IsUnicode(false);

            modelBuilder.Entity<Empresas>()
                .Property(e => e.fax)
                .IsUnicode(false);

            modelBuilder.Entity<Empresas>()
                .Property(e => e.email)
                .IsUnicode(false);

            modelBuilder.Entity<Empresas>()
                .HasMany(e => e.Pedidos)
                .WithRequired(e => e.Empresas)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Funcionarios>()
                .Property(e => e.nome)
                .IsUnicode(false);

            modelBuilder.Entity<ItensPedidos>()
                .Property(e => e.valor)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Materiais>()
                .Property(e => e.descricao)
                .IsUnicode(false);

            modelBuilder.Entity<Materiais>()
                .HasMany(e => e.ItensPedidos)
                .WithRequired(e => e.Materiais)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Materiais>()
                .HasMany(e => e.MateriaisColetados)
                .WithRequired(e => e.Materiais)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Materiais>()
                .HasMany(e => e.MateriaisComercializados)
                .WithRequired(e => e.Materiais)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MateriaisColetados>()
                .Property(e => e.valorCompra)
                .HasPrecision(18, 0);

            modelBuilder.Entity<MateriaisColetados>()
                .Property(e => e.coletado)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<MateriaisComercializados>()
                .Property(e => e.valorRevenda)
                .HasPrecision(8, 2);

            modelBuilder.Entity<Notificacoes>()
                .Property(e => e.descricao)
                .IsUnicode(false);

            modelBuilder.Entity<Notificacoes>()
                .Property(e => e.tipo)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Pedidos>()
                .Property(e => e.total)
                .HasPrecision(8, 2);

            modelBuilder.Entity<Pedidos>()
                .HasMany(e => e.ItensPedidos)
                .WithRequired(e => e.Pedidos)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Roles>()
                .HasMany(e => e.UsuarioRole)
                .WithRequired(e => e.Roles)
                .HasForeignKey(e => e.RoleId);

            modelBuilder.Entity<Rotas>()
                .Property(e => e.pontos)
                .HasPrecision(8, 2);

            modelBuilder.Entity<Usuarios>()
                .HasMany(e => e.Claims)
                .WithOptional(e => e.Usuarios)
                .HasForeignKey(e => e.IdentityUser_Id);

            modelBuilder.Entity<Usuarios>()
                .HasMany(e => e.Logins)
                .WithOptional(e => e.Usuarios)
                .HasForeignKey(e => e.IdentityUser_Id);

            modelBuilder.Entity<Usuarios>()
                .HasMany(e => e.UsuarioRole)
                .WithOptional(e => e.Usuarios)
                .HasForeignKey(e => e.IdentityUser_Id);
        }
    }
}
