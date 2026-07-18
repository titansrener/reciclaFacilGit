using ReciclaFacil.Core.Models;

namespace ReciclaFacil.Core.Services;

// Adaptador temporário. Na próxima etapa será substituído pelo repositório EF Core,
// sem alterar controllers ou views.
public sealed class CooperativaService : ICooperativaService
{
    private static readonly CooperativaResumo[] DadosDemonstracao =
    [
        new("Cooperativa Verde Fortaleza", "Fortaleza", "CE", "Centro"),
        new("Recicla Ceará", "Caucaia", "CE", "Parque Potira"),
        new("Coleta Sustentável", "Maracanaú", "CE", "Distrito Industrial")
    ];

    public IReadOnlyList<CooperativaResumo> Pesquisar(
        string? razaoSocial,
        string? cidade,
        string? estado)
    {
        IEnumerable<CooperativaResumo> consulta = DadosDemonstracao;

        if (!string.IsNullOrWhiteSpace(razaoSocial))
            consulta = consulta.Where(x => x.Nome.Contains(razaoSocial.Trim(), StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(cidade))
            consulta = consulta.Where(x => x.Cidade.Contains(cidade.Trim(), StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(estado))
            consulta = consulta.Where(x => string.Equals(x.Estado, estado.Trim(), StringComparison.OrdinalIgnoreCase));

        return consulta.ToArray();
    }
}
