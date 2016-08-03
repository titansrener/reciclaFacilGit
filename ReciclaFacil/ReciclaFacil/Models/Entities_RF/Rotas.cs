namespace ReciclaFacil.Models.Entities_RF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Rotas
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int rotaId { get; set; }

        public decimal pontos { get; set; }

        public DbGeometry pontosCoordenadas { get; set; }

        public int coletaId { get; set; }

        public virtual Coletas Coletas { get; set; }
    }
}
