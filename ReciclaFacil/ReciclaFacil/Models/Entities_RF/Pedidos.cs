namespace ReciclaFacil.Models.Entities_RF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Pedidos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Pedidos()
        {
            ItensPedidos = new HashSet<ItensPedidos>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int pedidoId { get; set; }

        public decimal? total { get; set; }

        public DateTime dataPedido { get; set; }

        public DateTime? dataEnvio { get; set; }

        public DateTime? dataEntrega { get; set; }

        [Required]
        [StringLength(128)]
        public string empresaId { get; set; }

        public virtual Empresas Empresas { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ItensPedidos> ItensPedidos { get; set; }
    }
}
