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
    DateTime? CreatedAt,
    string Description,
    string Type,
    bool Active);

public interface IClientQueries
{
    Task<ClientOverview?> GetOverviewAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
