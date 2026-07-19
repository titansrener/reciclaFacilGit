namespace ReciclaFacil.Core.Models;

public sealed record ClienteColetaListItemViewModel(
    int Id,
    DateTime? HorarioAgendado,
    string Status,
    int QuantidadeMateriais);

public sealed record ClienteDashboardViewModel(
    string Nome,
    IReadOnlyList<ClienteColetaListItemViewModel> Coletas);

public sealed record CarteiraMovimentoViewModel(
    DateTime? Data,
    decimal Valor);

public sealed record CarteiraViewModel(
    decimal Saldo,
    IReadOnlyList<CarteiraMovimentoViewModel> Movimentos);

public sealed record NotificacaoViewModel(
    int Id,
    DateTime? Data,
    string Descricao,
    string Tipo,
    bool Ativa);

public sealed class AgendarColetaViewModel
{
    [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue,
        ErrorMessage = "Selecione um horário.")]
    [System.ComponentModel.DataAnnotations.Display(Name = "Horário disponível")]
    public int ColetaId { get; set; }

    public IReadOnlyList<HorarioColetaViewModel> Horarios { get; set; } = [];
    public IReadOnlyList<MaterialSelecaoViewModel> Materiais { get; set; } = [];
    public int[] MateriaisSelecionados { get; set; } = [];
}

public sealed record HorarioColetaViewModel(int Id, DateTime Horario);
public sealed record MaterialSelecaoViewModel(int Id, string Descricao);

public sealed record ClienteColetaDetalheViewModel(
    int Id,
    DateTime? HorarioAgendado,
    DateTime? HorarioRealizado,
    string Status,
    IReadOnlyList<MaterialColetaViewModel> Materiais);

public sealed record MaterialColetaViewModel(
    string Descricao,
    double? Quantidade,
    decimal? Valor);
