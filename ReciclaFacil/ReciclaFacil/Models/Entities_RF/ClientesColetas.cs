namespace ReciclaFacil.Models.Entities_RF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ClientesColetas
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ClientesColetas()
        {
            MateriaisColetados = new HashSet<MateriaisColetados>();
        }

        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int coletaId { get; set; }

        [Key]
        [Column(Order = 1)]
        public string clienteId { get; set; }

        public DateTime? horaDaColeta { get; set; }

        [StringLength(1)]
        public string coletado { get; set; }

        public virtual Clientes Clientes { get; set; }

        public virtual Coletas Coletas { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MateriaisColetados> MateriaisColetados { get; set; }
    }
}
