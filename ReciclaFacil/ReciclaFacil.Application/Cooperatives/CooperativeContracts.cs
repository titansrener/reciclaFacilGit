namespace ReciclaFacil.Application.Cooperatives;

public sealed record CooperativeListItem(
    string Id,
    string Name,
    string Address,
    string City,
    string State,
    double? Latitude,
    double? Longitude);

public sealed record CooperativeDetails(
    string Id,
    string Name,
    string TaxId,
    string Address,
    string City,
    string State,
    string? Email,
    IReadOnlyList<CommercializedMaterial> Materials);

public sealed record CommercializedMaterial(
    int Id,
    string Description,
    decimal? ResalePrice);

public sealed record CooperativeSearch(
    string? Name = null,
    string? City = null,
    string? State = null,
    int Page = 1,
    int PageSize = 20);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int Total);

public interface ICooperativeQueries
{
    Task<PagedResult<CooperativeListItem>> SearchAsync(
        CooperativeSearch search,
        CancellationToken cancellationToken = default);

    Task<CooperativeDetails?> GetAsync(
        string id,
        CancellationToken cancellationToken = default);
}
