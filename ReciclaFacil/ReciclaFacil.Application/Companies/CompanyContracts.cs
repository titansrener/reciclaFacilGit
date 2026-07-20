namespace ReciclaFacil.Application.Companies;

public sealed record CompanyOverview(
    string Id,
    string Cnpj,
    string CorporateName,
    string Address,
    string Phone,
    string? Fax,
    string Email);

public interface ICompanyQueries
{
    Task<CompanyOverview?> GetOverviewAsync(
        string companyId,
        CancellationToken cancellationToken = default);
}
