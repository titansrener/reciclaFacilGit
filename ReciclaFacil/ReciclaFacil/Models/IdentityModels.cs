using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using ReciclaFacil.Models.Entities_RF;

namespace ReciclaFacil.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public DateTime dataCadastro { get; set; }

        public bool ativo { get; set; }
        
    
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("ReciclaFacil_Contexto", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        // Tabelas para Login Customizadas
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IdentityUser>()
                .ToTable("Usuarios")
                .Property(p => p.Id)
                .HasColumnName("usuarioId");

            modelBuilder.Entity<ApplicationUser>()
                .ToTable("Usuarios")
                .Property(p => p.Id)
                .HasColumnName("usuarioId");

            modelBuilder.Entity<IdentityUserRole>()
                .ToTable("UsuarioRole");

            modelBuilder.Entity<IdentityUserLogin>()
                .ToTable("Logins");

            modelBuilder.Entity<IdentityUserClaim>()
                .ToTable("Claims");

            modelBuilder.Entity<IdentityRole>()
                .ToTable("Roles");

        }
        
    }
}