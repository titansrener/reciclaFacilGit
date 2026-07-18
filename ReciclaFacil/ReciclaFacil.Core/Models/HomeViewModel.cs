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
    string Nome,
    string Cidade,
    string Estado,
    string Endereco);
