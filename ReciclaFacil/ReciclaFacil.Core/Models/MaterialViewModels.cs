using System.ComponentModel.DataAnnotations;

namespace ReciclaFacil.Core.Models;

public sealed record MaterialListItemViewModel(
    int Id,
    string Descricao,
    int TempoMedioDecomposicao);

public sealed class MaterialEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe a descrição.")]
    [StringLength(50, ErrorMessage = "A descrição deve ter no máximo 50 caracteres.")]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = "";

    [Range(0, 1_000_000, ErrorMessage = "Informe um tempo válido em anos.")]
    [Display(Name = "Tempo médio de decomposição (anos)")]
    public int TempoMedioDecomposicao { get; set; }
}
