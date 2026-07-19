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

public sealed record CooperativeOverview(
    string Id,
    string Name,
    int ScheduledCollections,
    int InProgressCollections,
    int FinishedCollections,
    int RegisteredClients,
    IReadOnlyList<CooperativeCollectionSummary> Collections,
    IReadOnlyList<ManagedMaterial> Materials,
    IReadOnlyList<MaterialOption> MaterialOptions);

public sealed record CooperativeCollectionSummary(
    int Id,
    DateTime? ScheduledAt,
    string Status,
    int ClientCount,
    double? Quantity);

public sealed record CooperativeCollectionDetails(
    int Id,
    DateTime? ScheduledAt,
    string Status,
    double? Quantity,
    IReadOnlyList<CooperativeCollectionClient> Clients);

public sealed record CooperativeCollectionClient(
    string Id,
    string Name,
    string? Status,
    DateTime? CollectedAt,
    IReadOnlyList<string> Materials);

public sealed record ManagedMaterial(int Id, string Description, decimal? ResalePrice);
public sealed record MaterialOption(int Id, string Description);
public sealed record SaveCooperativeCollection(DateTime ScheduledAt);
public sealed record SaveManagedMaterial(int MaterialId, decimal? ResalePrice);

public enum CooperativeOperationError
{
    None,
    NotFound,
    InvalidDate,
    InvalidStatus,
    HasClients,
    AlreadyExists,
    InvalidPrice,
    MaterialInUse
}

public sealed record CooperativeOperationResult(
    CooperativeOperationError Error = CooperativeOperationError.None,
    int? CollectionId = null)
{
    public bool Succeeded => Error == CooperativeOperationError.None;
}

public interface ICooperativeOperations
{
    Task<CooperativeOverview?> GetOverviewAsync(string cooperativeId, CancellationToken cancellationToken = default);
    Task<CooperativeCollectionDetails?> GetCollectionAsync(string cooperativeId, int collectionId, CancellationToken cancellationToken = default);
    Task<CooperativeOperationResult> CreateCollectionAsync(string cooperativeId, SaveCooperativeCollection command, CancellationToken cancellationToken = default);
    Task<CooperativeOperationResult> UpdateCollectionAsync(string cooperativeId, int collectionId, SaveCooperativeCollection command, CancellationToken cancellationToken = default);
    Task<CooperativeOperationResult> DeleteCollectionAsync(string cooperativeId, int collectionId, CancellationToken cancellationToken = default);
    Task<CooperativeOperationResult> StartCollectionAsync(string cooperativeId, int collectionId, CancellationToken cancellationToken = default);
    Task<CooperativeOperationResult> FinishCollectionAsync(string cooperativeId, int collectionId, CancellationToken cancellationToken = default);
    Task<CooperativeOperationResult> AddMaterialAsync(string cooperativeId, SaveManagedMaterial command, CancellationToken cancellationToken = default);
    Task<CooperativeOperationResult> UpdateMaterialAsync(string cooperativeId, SaveManagedMaterial command, CancellationToken cancellationToken = default);
    Task<CooperativeOperationResult> RemoveMaterialAsync(string cooperativeId, int materialId, CancellationToken cancellationToken = default);
}
