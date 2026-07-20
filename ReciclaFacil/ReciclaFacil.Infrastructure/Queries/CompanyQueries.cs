using Microsoft.EntityFrameworkCore;
using ReciclaFacil.Application.Companies;
using ReciclaFacil.Infrastructure.Data;

namespace ReciclaFacil.Infrastructure.Queries;

public sealed class CompanyQueries(ReciclaFacilDbContext database) : ICompanyQueries
{
    public Task<CompanyOverview?> GetOverviewAsync(
        string companyId,
        CancellationToken cancellationToken = default) =>
        database.Empresas.AsNoTracking()
            .Where(x => x.Id == companyId)
            .Select(x => new CompanyOverview(
                x.Id,
                x.Cnpj,
                x.RazaoSocial,
                x.Endereco,
                x.Telefone,
                x.Fax,
                x.Email))
            .SingleOrDefaultAsync(cancellationToken);
}
