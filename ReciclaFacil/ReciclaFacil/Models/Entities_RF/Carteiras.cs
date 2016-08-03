namespace ReciclaFacil.Models.Entities_RF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Carteiras
    {
        [Key]
        public int carteiraId { get; set; }

        public decimal saldo { get; set; }

        public DateTime? dataUltimaMovimentacao { get; set; }

        [Required]
        [StringLength(128)]
        public string clienteId { get; set; }

        public virtual Clientes Clientes { get; set; }
    }
}
