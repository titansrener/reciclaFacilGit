using System.Data;
using Microsoft.EntityFrameworkCore;
using ReciclaFacil.Application.Clients;
using ReciclaFacil.Infrastructure.Data;

namespace ReciclaFacil.Infrastructure.Services;

public sealed class ClientFinancialService(ReciclaFacilDbContext database) : IClientFinancialService
{
    public async Task<ClientFinancialResult> AcceptOfferAsync(
        string clientId, int collectionId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await database.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var link = await database.ClientesColetas.Include(x => x.Cliente)
            .Include(x => x.Materiais)
            .SingleOrDefaultAsync(
                x => x.ClienteId == clientId && x.ColetaId == collectionId, cancellationToken);
        if (link is null || link.Cliente?.Tipo != "V")
            return new(ClientFinancialError.NotFound);
        if (link.Status != "P")
            return new(ClientFinancialError.NoPendingOffer);
        var total = link.Materiais.Sum(x => x.ValorCompra ?? 0);
        link.Status = "S";
        database.Carteiras.Add(new()
        {
            ClienteId = clientId,
            DataUltimaMovimentacao = DateTime.Now,
            Saldo = total
        });
        await DeactivateNotifications(clientId, collectionId, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(Value: total);
    }

    public async Task<ClientFinancialResult> RejectOfferAsync(
        string clientId, int collectionId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await database.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var link = await database.ClientesColetas.Include(x => x.Cliente)
            .Include(x => x.Materiais)
            .SingleOrDefaultAsync(
                x => x.ClienteId == clientId && x.ColetaId == collectionId, cancellationToken);
        if (link is null || link.Cliente?.Tipo != "V")
            return new(ClientFinancialError.NotFound);
        if (link.Status != "P")
            return new(ClientFinancialError.NoPendingOffer);
        link.Status = "A";
        link.HoraDaColeta = null;
        foreach (var material in link.Materiais)
        {
            material.Status = "A";
            material.Quantidade = null;
            material.ValorCompra = null;
        }
        await DeactivateNotifications(clientId, collectionId, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new();
    }

    public async Task<ClientFinancialResult> ReadNotificationAsync(
        string clientId, int notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await database.Notificacoes.SingleOrDefaultAsync(
            x => x.ClienteId == clientId && x.Id == notificationId, cancellationToken);
        if (notification is null) return new(ClientFinancialError.NotFound);
        notification.Ativa = false;
        await database.SaveChangesAsync(cancellationToken);
        return new();
    }

    private async Task DeactivateNotifications(
        string clientId, int collectionId, CancellationToken cancellationToken)
    {
        var notifications = await database.Notificacoes
            .Where(x => x.ClienteId == clientId && x.ColetaId == collectionId && x.Ativa == true)
            .ToListAsync(cancellationToken);
        foreach (var notification in notifications) notification.Ativa = false;
    }
}
