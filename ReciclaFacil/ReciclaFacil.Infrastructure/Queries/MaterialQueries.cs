using Microsoft.EntityFrameworkCore;
using ReciclaFacil.Application.Materials;
using ReciclaFacil.Infrastructure.Data;

namespace ReciclaFacil.Infrastructure.Queries;

public sealed class MaterialQueries(ReciclaFacilDbContext database) : IMaterialQueries
{
    public async Task<IReadOnlyList<MaterialItem>> ListAsync(
        CancellationToken cancellationToken = default) =>
        await database.Materiais.AsNoTracking()
            .OrderBy(x => x.Descricao)
            .Select(x => new MaterialItem(
                x.Id, x.Descricao, x.TempoMedioDecomposicao))
            .ToListAsync(cancellationToken);
}
