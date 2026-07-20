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
    IReadOnlyList<CooperativeCollectionClient> Clients,
    IReadOnlyList<TruckSummary> Trucks,
    IReadOnlyList<EmployeeSummary> Employees);

public sealed record CooperativeCollectionClient(
    string Id,
    string Name,
    string Type,
    string Address,
    string Email,
    string? Phone,
    string Mobile,
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

public sealed record CooperativeResources(
    IReadOnlyList<TruckSummary> Trucks,
    IReadOnlyList<EmployeeSummary> Employees);
public sealed record TruckSummary(int Id, string Description, string Plate);
public sealed record EmployeeSummary(string Id, string Name, DateTime BirthDate, string Email);
public sealed record SaveTruck(string Description, string Plate);
public sealed record CreateEmployee(string Name, DateTime BirthDate, string Email, string Password);
public sealed record UpdateEmployee(string Name, DateTime BirthDate);

public enum CooperativeResourceError
{
    None,
    NotFound,
    InvalidInput,
    PlateExists,
    EmailExists,
    AlreadyAssigned,
    NotAssigned
}

public sealed record CooperativeResourceResult(
    CooperativeResourceError Error = CooperativeResourceError.None,
    int? TruckId = null,
    string? EmployeeId = null)
{
    public bool Succeeded => Error == CooperativeResourceError.None;
}

public interface ICooperativeResourceService
{
    Task<CooperativeResources?> ListAsync(string cooperativeId, CancellationToken cancellationToken = default);
    Task<CooperativeResourceResult> CreateTruckAsync(string cooperativeId, SaveTruck command, CancellationToken cancellationToken = default);
    Task<CooperativeResourceResult> UpdateTruckAsync(string cooperativeId, int truckId, SaveTruck command, CancellationToken cancellationToken = default);
    Task<CooperativeResourceResult> DeleteTruckAsync(string cooperativeId, int truckId, CancellationToken cancellationToken = default);
    Task<CooperativeResourceResult> CreateEmployeeAsync(string cooperativeId, CreateEmployee command, CancellationToken cancellationToken = default);
    Task<CooperativeResourceResult> UpdateEmployeeAsync(string cooperativeId, string employeeId, UpdateEmployee command, CancellationToken cancellationToken = default);
    Task<CooperativeResourceResult> DeleteEmployeeAsync(string cooperativeId, string employeeId, CancellationToken cancellationToken = default);
    Task<CooperativeResourceResult> AssignTruckAsync(string cooperativeId, int collectionId, int truckId, bool assign, CancellationToken cancellationToken = default);
    Task<CooperativeResourceResult> AssignEmployeeAsync(string cooperativeId, int collectionId, string employeeId, bool assign, CancellationToken cancellationToken = default);
}
