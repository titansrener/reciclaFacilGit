namespace ReciclaFacil.Application.Employees;

public sealed record EmployeeOverview(
    string Id,
    string Name,
    string CooperativeName,
    IReadOnlyList<EmployeeCollectionSummary> Collections);

public sealed record EmployeeCollectionSummary(
    int Id,
    DateTime? ScheduledAt,
    string Status,
    int PendingClients,
    int CompletedClients);

public sealed record EmployeeCollectionDetails(
    int Id,
    DateTime? ScheduledAt,
    string Status,
    IReadOnlyList<EmployeeTruck> Trucks,
    IReadOnlyList<EmployeeClientSummary> Clients);

public sealed record EmployeeTruck(int Id, string Plate, string Description);
public sealed record EmployeeClientSummary(
    string Id,
    string Name,
    string Type,
    string Address,
    string Mobile,
    string? Status,
    double? Latitude,
    double? Longitude);

public sealed record EmployeeClientDetails(
    string Id,
    string Name,
    string Type,
    string Address,
    string? Phone,
    string Mobile,
    int CollectionId,
    string CollectionStatus,
    string? Status,
    DateTime? CollectedAt,
    IReadOnlyList<EmployeeMaterial> Materials);

public sealed record EmployeeMaterial(
    int Id,
    string Description,
    double? Quantity,
    decimal? PurchaseValue,
    string? Status);

public sealed record RecordCollectedMaterials(
    IReadOnlyList<CollectedMaterialQuantity> Materials);
public sealed record CollectedMaterialQuantity(int MaterialId, double Quantity);

public enum EmployeeOperationError
{
    None,
    NotFound,
    CollectionNotStarted,
    ClientAlreadyProcessed,
    InvalidMaterials,
    MissingPrice
}

public sealed record EmployeeOperationResult(
    EmployeeOperationError Error = EmployeeOperationError.None,
    decimal TotalValue = 0)
{
    public bool Succeeded => Error == EmployeeOperationError.None;
}

public interface IEmployeeOperations
{
    Task<EmployeeOverview?> GetOverviewAsync(string employeeId, CancellationToken cancellationToken = default);
    Task<EmployeeCollectionDetails?> GetCollectionAsync(string employeeId, int collectionId, CancellationToken cancellationToken = default);
    Task<EmployeeClientDetails?> GetClientAsync(string employeeId, int collectionId, string clientId, CancellationToken cancellationToken = default);
    Task<EmployeeOperationResult> RecordAsync(string employeeId, int collectionId, string clientId, RecordCollectedMaterials command, CancellationToken cancellationToken = default);
}
