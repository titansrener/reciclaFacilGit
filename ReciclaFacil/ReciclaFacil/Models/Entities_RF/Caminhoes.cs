namespace ReciclaFacil.Models.Entities_RF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Caminhoes
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Caminhoes()
        {
            Coletas = new HashSet<Coletas>();
        }

        [Key]
        public int caminhaoId { get; set; }

        [Required]
        [StringLength(45)]
        public string descricao { get; set; }

        [Required]
        [StringLength(8)]
        public string placa { get; set; }

        [Required]
        [StringLength(128)]
        public string cooperativaId { get; set; }

        public virtual Cooperativas Cooperativas { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Coletas> Coletas { get; set; }
    }
}
