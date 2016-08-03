namespace ReciclaFacil.Models.Entities_RF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Dicas
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Dicas()
        {
            Clientes = new HashSet<Clientes>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int dicaId { get; set; }

        public string dicaEmTexto { get; set; }

        public byte[] dicaEmImagem { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Clientes> Clientes { get; set; }
    }
}
