using Microsoft.EntityFrameworkCore;
using ReciclaFacil.Application.Cooperatives;
using ReciclaFacil.Infrastructure.Data;

namespace ReciclaFacil.Infrastructure.Queries;

public sealed class CooperativeQueries(ReciclaFacilDbContext database) : ICooperativeQueries
{
    public async Task<PagedResult<CooperativeListItem>> SearchAsync(
        CooperativeSearch search,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, search.Page);
        var pageSize = Math.Clamp(search.PageSize, 1, 100);
        var query = database.Cooperativas.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search.Name))
            query = query.Where(x => x.RazaoSocial.Contains(search.Name.Trim()));
        if (!string.IsNullOrWhiteSpace(search.City))
            query = query.Where(x => x.Cidade.Contains(search.City.Trim()));
        if (!string.IsNullOrWhiteSpace(search.State))
            query = query.Where(x => x.Estado == search.State.Trim());

        var total = await query.CountAsync(cancellationToken);
        var entities = await query.OrderBy(x => x.RazaoSocial)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var items = entities.Select(x => new CooperativeListItem(
            x.Id, x.RazaoSocial, x.Endereco, x.Cidade, x.Estado,
            x.EnderecoCoordenada?.Y, x.EnderecoCoordenada?.X)).ToArray();
        return new(items, page, pageSize, total);
    }

    public async Task<CooperativeDetails?> GetAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var cooperative = await database.Cooperativas.AsNoTracking()
            .Include(x => x.MateriaisComercializados)
            .ThenInclude(x => x.Material)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cooperative is null)
            return null;
        var email = await database.Users.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => x.Email)
            .SingleOrDefaultAsync(cancellationToken);
        return new(
            cooperative.Id, cooperative.RazaoSocial, cooperative.Cnpj,
            cooperative.Endereco, cooperative.Cidade, cooperative.Estado, email,
            cooperative.MateriaisComercializados
                .Where(x => x.Material is not null)
                .Select(x => new CommercializedMaterial(
                    x.MaterialId, x.Material!.Descricao, x.ValorRevenda))
                .OrderBy(x => x.Description)
                .ToArray());
    }
}
