namespace ReciclaFacil.Models.Entities_RF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Notificacoes
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int notificacaoId { get; set; }

        [Key]
        [Column(Order = 1)]
        public string clienteId { get; set; }

        public int coletaId { get; set; }

        [Required]
        [StringLength(128)]
        public string cooperativaId { get; set; }

        public DateTime? dataHorario { get; set; }

        public bool? ativa { get; set; }

        [Required]
        [StringLength(150)]
        public string descricao { get; set; }

        [Required]
        [StringLength(1)]
        public string tipo { get; set; }

        public virtual Clientes Clientes { get; set; }

        public virtual Coletas Coletas { get; set; }

        public virtual Cooperativas Cooperativas { get; set; }
    }
}
