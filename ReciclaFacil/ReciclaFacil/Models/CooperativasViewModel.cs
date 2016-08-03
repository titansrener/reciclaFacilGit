using ReciclaFacil.Models.Entities_RF;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReciclaFacil.Models
{
    public class ColetaViewModel
    {
        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Data:")]
        [DataType(DataType.Date)]
        public DateTime data { get; set; }

        [Required]
        [Display(Name = "Horário:")]
        public int hora { get; set; }

        public string idCoop { get; set; }

        public int id { get; set; }
    }

    public class CaminhaoViewModel
    {
        [Required]
        [Display(Name = "Descrição:")]
        [StringLength(45, ErrorMessage = "Máximo de {0} caracteres.")]
        public string descricao { get; set; }

        [Required]
        [Display(Name = "Placa:")]
        [StringLength(8, ErrorMessage = "Máximo de {0} caracteres.")]
        public string placa { get; set; }

        public string cooperativaId { get; set; }

        [Key]
        public int id { get; set; }
    }

    public class FuncionarioColetaViewModel
    {
        public string nome { get; set; }
        public string funcionarioId { get; set; }
        public bool selecionado { get; set; }
    }

    public class CaminhaoColetaViewModel
    {
        public int caminhaoId { get; set; }

        public string placa { get; set; }

        public string descricao { get; set; }

        public bool selecionado { get; set; }
    }

    public class AssociarFuncionarioViewModel
    {
        public int coletaId { get; set; }

        public List<FuncionarioColetaViewModel> funcionarios { get; set; }
    }

    public class AssociarCaminhaoViewModel
    {
        public int coletaId { get; set; }

        public List<CaminhaoColetaViewModel> caminhoes { get; set; }
    }
}