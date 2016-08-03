namespace ReciclaFacil.Models.Entities_RF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MateriaisComercializados
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int materialId { get; set; }

        [Key]
        [Column(Order = 1)]
        public string cooperativaId { get; set; }

        public decimal? valorRevenda { get; set; }

        public virtual Cooperativas Cooperativas { get; set; }

        public virtual Materiais Materiais { get; set; }
    }
}
