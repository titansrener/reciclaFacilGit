using Microsoft.EntityFrameworkCore;
using ReciclaFacil.Core.Data;
using ReciclaFacil.Core.Models;

namespace ReciclaFacil.Core.Services;

public sealed class CooperativaService(
    ReciclaFacilDbContext database,
    IConfiguration configuration,
    ILogger<CooperativaService> logger) : ICooperativaService
{
    private static readonly CooperativaResumo[] DadosDemonstracao =
    [
        new("demo-1", "Cooperativa Verde Fortaleza", "Fortaleza", "CE", "Centro"),
        new("demo-2", "Recicla Ceará", "Caucaia", "CE", "Parque Potira"),
        new("demo-3", "Coleta Sustentável", "Maracanaú", "CE", "Distrito Industrial")
    ];

    public async Task<IReadOnlyList<CooperativaResumo>> PesquisarAsync(
        string? razaoSocial,
        string? cidade,
        string? estado,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = database.Cooperativas.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(razaoSocial))
                query = query.Where(x => x.RazaoSocial.Contains(razaoSocial.Trim()));
            if (!string.IsNullOrWhiteSpace(cidade))
                query = query.Where(x => x.Cidade.Contains(cidade.Trim()));
            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(x => x.Estado == estado.Trim());

            var cooperativas = await query.OrderBy(x => x.RazaoSocial)
                .Take(100)
                .ToListAsync(cancellationToken);
            return cooperativas.Select(ToResumo).ToArray();
        }
        catch (Exception exception) when (configuration.GetValue<bool>("ReciclaFacil:ModoMigracao"))
        {
            logger.LogWarning(exception,
                "Banco legado indisponível; usando dados de demonstração no modo de migração.");
            return FiltrarDemonstracao(razaoSocial, cidade, estado);
        }
    }

    public async Task<CooperativaDetalheViewModel?> ObterAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (id.StartsWith("demo-", StringComparison.Ordinal))
            return null;

        var cooperativa = await database.Cooperativas.AsNoTracking()
            .Include(x => x.MateriaisComercializados)
            .ThenInclude(x => x.Material)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cooperativa is null)
            return null;

        var email = await database.Users.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => x.Email)
            .SingleOrDefaultAsync(cancellationToken);

        return new(
            cooperativa.Id,
            cooperativa.RazaoSocial,
            cooperativa.Cnpj,
            cooperativa.Endereco,
            cooperativa.Cidade,
            cooperativa.Estado,
            email,
            cooperativa.MateriaisComercializados
                .Where(x => x.Material is not null)
                .OrderBy(x => x.Material!.Descricao)
                .Select(x => new MaterialComercializadoViewModel(
                    x.Material!.Descricao, x.ValorRevenda))
                .ToArray());
    }

    private static CooperativaResumo ToResumo(Cooperativa value) =>
        new(value.Id, value.RazaoSocial, value.Cidade, value.Estado, value.Endereco,
            value.EnderecoCoordenada?.Y, value.EnderecoCoordenada?.X);

    private static IReadOnlyList<CooperativaResumo> FiltrarDemonstracao(
        string? razaoSocial, string? cidade, string? estado)
    {
        IEnumerable<CooperativaResumo> query = DadosDemonstracao;
        if (!string.IsNullOrWhiteSpace(razaoSocial))
            query = query.Where(x => x.Nome.Contains(razaoSocial.Trim(), StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(cidade))
            query = query.Where(x => x.Cidade.Contains(cidade.Trim(), StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(x => string.Equals(x.Estado, estado.Trim(), StringComparison.OrdinalIgnoreCase));
        return query.ToArray();
    }
}
