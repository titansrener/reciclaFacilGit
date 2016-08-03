namespace ReciclaFacil.Migrations
{
    using Models.Entities_RF;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ReciclaFacil.Models.Entities_RF.ReciclaFacil_Contexto>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(ReciclaFacil.Models.Entities_RF.ReciclaFacil_Contexto context)
        {
            context.Roles.AddOrUpdate(
                new Roles() { Id = "0", Name = "Admin" },
                new Roles() { Id = "1", Name = "Cliente" },
                new Roles() { Id = "2", Name = "Cooperativa" },
                new Roles() { Id = "3", Name = "Empresa" },
                new Roles() { Id = "4", Name = "Funcionario" }
            );
            //Criando Administrador
            context.Usuarios.AddOrUpdate(
                new Usuarios()
                {
                    usuarioId = "29bc1d57-736c-4b47-9cdb-2fca49e1eb82",
                    Email = "administrador@email.com",
                    EmailConfirmed = false,
                    PasswordHash = "ANumISLJUF1jNwMb5l5yZSPGOtH6CLxKRs894kvnONU5S89spplcaUDfxZ5B+eqhuA==",
                    SecurityStamp = "4b7e0783-a352-4017-8a50-fb96bfb2b6aa",
                    PhoneNumber = null,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEndDateUtc = null,
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
                    UserName = "administrador@email.com",
                    dataCadastro = DateTime.Now,
                    ativo = true,
                    Discriminator = "ApplicationUser"
                }
            );
            //Atribuindo a Role ao Administrador
            context.UsuarioRole.AddOrUpdate(
                new UsuarioRole() { RoleId = "0", UserId = "29bc1d57-736c-4b47-9cdb-2fca49e1eb82" }
            );
        }
    }
}
