namespace ReciclaFacil.Core.Models;

public sealed class HomeViewModel
{
    public IReadOnlyList<string> Estados { get; } =
    [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA",
        "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN",
        "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    ];
}

public sealed record CooperativaResumo(
    string Id,
    string Nome,
    string Cidade,
    string Estado,
    string Endereco,
    double? Latitude = null,
    double? Longitude = null);

public sealed record CooperativaDetalheViewModel(
    string Id,
    string RazaoSocial,
    string Cnpj,
    string Endereco,
    string Cidade,
    string Estado,
    string? Email,
    IReadOnlyList<MaterialComercializadoViewModel> Materiais);

public sealed record MaterialComercializadoViewModel(string Descricao, decimal? ValorRevenda);
