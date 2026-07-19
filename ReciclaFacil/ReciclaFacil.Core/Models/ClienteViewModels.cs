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
