namespace ReciclaFacil.Models.Entities_RF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Materiais
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Materiais()
        {
            ItensPedidos = new HashSet<ItensPedidos>();
            MateriaisColetados = new HashSet<MateriaisColetados>();
            MateriaisComercializados = new HashSet<MateriaisComercializados>();
        }

        [Key]
        public int materialId { get; set; }

        [Required]
        [StringLength(50)]
        public string descricao { get; set; }

        public int tempoMedioDecomposicao { get; set; }

        public bool selecionado { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ItensPedidos> ItensPedidos { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MateriaisColetados> MateriaisColetados { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MateriaisComercializados> MateriaisComercializados { get; set; }
    }
}
