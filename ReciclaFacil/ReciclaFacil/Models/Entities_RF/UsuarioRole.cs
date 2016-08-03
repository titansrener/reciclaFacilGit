namespace ReciclaFacil.Models.Entities_RF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UsuarioRole")]
    public partial class UsuarioRole
    {
        [Key]
        [Column(Order = 0)]
        public string UserId { get; set; }

        [Key]
        [Column(Order = 1)]
        public string RoleId { get; set; }

        [StringLength(128)]
        public string IdentityUser_Id { get; set; }

        public virtual Roles Roles { get; set; }

        public virtual Usuarios Usuarios { get; set; }
    }
}
