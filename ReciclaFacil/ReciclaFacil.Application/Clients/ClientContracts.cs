namespace ReciclaFacil.Application.Clients;

public sealed record ClientOverview(
    string Name,
    string CooperativeName,
    decimal WalletBalance,
    int ActiveNotificationCount,
    IReadOnlyList<ClientCollectionSummary> Collections,
    IReadOnlyList<ClientNotificationSummary> RecentNotifications);

public sealed record ClientCollectionSummary(
    int Id,
    DateTime? ScheduledAt,
    DateTime? CollectedAt,
    string Status,
    int MaterialCount);

public sealed record ClientNotificationSummary(
    int Id,
    int CollectionId,
    DateTime? CreatedAt,
    string Description,
    string Type,
    bool Active,
    bool RequiresValueDecision,
    decimal? OfferedValue);

public interface IClientQueries
{
    Task<ClientOverview?> GetOverviewAsync(
        string userId,
        CancellationToken cancellationToken = default);
}

public sealed record ClientCollectionOptions(
    IReadOnlyList<ClientCollectionSlot> Slots,
    IReadOnlyList<ClientAcceptedMaterial> Materials);

public sealed record ClientCollectionSlot(int Id, DateTime ScheduledAt);
public sealed record ClientAcceptedMaterial(int Id, string Description);

public sealed record ClientCollectionDetails(
    int Id,
    DateTime? ScheduledAt,
    DateTime? CollectedAt,
    string Status,
    IReadOnlyList<ClientCollectionMaterial> Materials);

public sealed record ClientCollectionMaterial(
    int Id,
    string Description,
    double? Quantity,
    decimal? Value);

public sealed record ScheduleClientCollection(
    int CollectionId,
    IReadOnlyList<int> MaterialIds);

public enum ClientCollectionError
{
    None,
    ClientNotFound,
    CollectionNotFound,
    CollectionUnavailable,
    AlreadyScheduled,
    NoMaterials,
    MaterialNotAccepted,
    CannotCancel,
    CannotUpdate
}

public sealed record ClientCollectionResult(
    bool Success,
    ClientCollectionError Error = ClientCollectionError.None,
    string? Message = null)
{
    public static ClientCollectionResult Completed() => new(true);
    public static ClientCollectionResult Failed(
        ClientCollectionError error,
        string message) => new(false, error, message);
}

public interface IClientCollectionService
{
    Task<ClientCollectionOptions?> GetOptionsAsync(
        string userId,
        int? currentCollectionId = null,
        CancellationToken cancellationToken = default);

    Task<ClientCollectionDetails?> GetDetailsAsync(
        string userId,
        int collectionId,
        CancellationToken cancellationToken = default);

    Task<ClientCollectionResult> ScheduleAsync(
        string userId,
        ScheduleClientCollection command,
        CancellationToken cancellationToken = default);

    Task<ClientCollectionResult> CancelAsync(
        string userId,
        int collectionId,
        CancellationToken cancellationToken = default);

    Task<ClientCollectionResult> UpdateAsync(
        string userId,
        int collectionId,
        ScheduleClientCollection command,
        CancellationToken cancellationToken = default);
}

public enum ClientFinancialError
{
    None,
    NotFound,
    NoPendingOffer
}

public sealed record ClientFinancialResult(
    ClientFinancialError Error = ClientFinancialError.None,
    decimal Value = 0)
{
    public bool Succeeded => Error == ClientFinancialError.None;
}

public interface IClientFinancialService
{
    Task<ClientFinancialResult> AcceptOfferAsync(
        string clientId, int collectionId, CancellationToken cancellationToken = default);
    Task<ClientFinancialResult> RejectOfferAsync(
        string clientId, int collectionId, CancellationToken cancellationToken = default);
    Task<ClientFinancialResult> ReadNotificationAsync(
        string clientId, int notificationId, CancellationToken cancellationToken = default);
}
