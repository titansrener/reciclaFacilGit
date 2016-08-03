namespace ReciclaFacil.Models.Entities_RF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MateriaisColetados
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int materialId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int coletaId { get; set; }

        [Key]
        [Column(Order = 2)]
        public string clienteId { get; set; }

        public double? quantidade { get; set; }

        public decimal? valorCompra { get; set; }

        [StringLength(1)]
        public string coletado { get; set; }

        public virtual ClientesColetas ClientesColetas { get; set; }

        public virtual Materiais Materiais { get; set; }
    }
}
