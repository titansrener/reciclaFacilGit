namespace ReciclaFacil.Models.Entities_RF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Empresas
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Empresas()
        {
            Pedidos = new HashSet<Pedidos>();
        }

        [Key]
        public string empresaId { get; set; }

        [Required]
        [StringLength(14)]
        public string cnpj { get; set; }

        [Required]
        [StringLength(150)]
        public string razaoSocial { get; set; }

        [Required]
        [StringLength(150)]
        public string endereco { get; set; }

        public DbGeometry enderecoCoordenada { get; set; }

        [Required]
        [StringLength(11)]
        public string telefone { get; set; }

        [StringLength(25)]
        public string fax { get; set; }

        [StringLength(45)]
        public string email { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Pedidos> Pedidos { get; set; }
    }
}
