using ReciclaFacil.Models.Entities_RF;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReciclaFacil.Models
{
    public class AgendamentoViewModel
    {
        [Required]
        [Display(Name = "Data e Hora da coleta:")]
        public int coletaId { get; set; }

        [Display(Name = "Data e Hora da coleta:")]
        public int novoColetaId { get; set; }

        [Display(Name = "Materiais:")]
        public List<Materiais> materiais { get; set; }
    }
}