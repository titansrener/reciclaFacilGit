using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReciclaFacil.Application.Clients;
using ReciclaFacil.Infrastructure.Data;

namespace ReciclaFacil.Infrastructure.Services;

public sealed class ClientCollectionService(ReciclaFacilDbContext database)
    : IClientCollectionService
{
    public async Task<ClientCollectionOptions?> GetOptionsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var cooperativeId = await database.Clientes.AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => x.CooperativaId)
            .SingleOrDefaultAsync(cancellationToken);
        if (cooperativeId is null)
            return null;

        var scheduledIds = database.ClientesColetas.AsNoTracking()
            .Where(x => x.ClienteId == userId)
            .Select(x => x.ColetaId);
        var slots = await database.Coletas.AsNoTracking()
            .Where(x => x.CooperativaId == cooperativeId &&
                        x.Status == "A" &&
                        x.HoraAgendada > DateTime.Now &&
                        !scheduledIds.Contains(x.Id))
            .OrderBy(x => x.HoraAgendada)
            .Select(x => new ClientCollectionSlot(x.Id, x.HoraAgendada!.Value))
            .ToListAsync(cancellationToken);
        var materials = await database.MateriaisComercializados.AsNoTracking()
            .Where(x => x.CooperativaId == cooperativeId)
            .OrderBy(x => x.Material!.Descricao)
            .Select(x => new ClientAcceptedMaterial(
                x.MaterialId, x.Material!.Descricao))
            .ToListAsync(cancellationToken);
        return new(slots, materials);
    }

    public Task<ClientCollectionDetails?> GetDetailsAsync(
        string userId,
        int collectionId,
        CancellationToken cancellationToken = default) =>
        database.ClientesColetas.AsNoTracking()
            .Where(x => x.ColetaId == collectionId && x.ClienteId == userId)
            .Select(x => new ClientCollectionDetails(
                x.ColetaId,
                x.Coleta!.HoraAgendada,
                x.HoraDaColeta,
                x.Status ?? x.Coleta.Status,
                x.Materiais.OrderBy(m => m.Material!.Descricao)
                    .Select(m => new ClientCollectionMaterial(
                        m.MaterialId,
                        m.Material!.Descricao,
                        m.Quantidade,
                        m.ValorCompra))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ClientCollectionResult> ScheduleAsync(
        string userId,
        ScheduleClientCollection command,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await database.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var cooperativeId = await database.Clientes
            .Where(x => x.Id == userId)
            .Select(x => x.CooperativaId)
            .SingleOrDefaultAsync(cancellationToken);
        if (cooperativeId is null)
            return ClientCollectionResult.Failed(
                ClientCollectionError.ClientNotFound, "Cliente não encontrado.");

        var collection = await database.Coletas.SingleOrDefaultAsync(
            x => x.Id == command.CollectionId &&
                 x.CooperativaId == cooperativeId,
            cancellationToken);
        if (collection is null)
            return ClientCollectionResult.Failed(
                ClientCollectionError.CollectionNotFound, "Horário não encontrado.");
        if (collection.Status != "A" || collection.HoraAgendada <= DateTime.Now)
            return ClientCollectionResult.Failed(
                ClientCollectionError.CollectionUnavailable,
                "O horário selecionado não está mais disponível.");

        if (await database.ClientesColetas.AnyAsync(
                x => x.ClienteId == userId && x.ColetaId == command.CollectionId,
                cancellationToken))
            return ClientCollectionResult.Failed(
                ClientCollectionError.AlreadyScheduled,
                "Você já está associado a essa coleta.");

        var selectedIds = command.MaterialIds.Distinct().ToArray();
        if (selectedIds.Length == 0)
            return ClientCollectionResult.Failed(
                ClientCollectionError.NoMaterials,
                "Selecione pelo menos um material.");

        var acceptedCount = await database.MateriaisComercializados.CountAsync(
            x => x.CooperativaId == cooperativeId &&
                 selectedIds.Contains(x.MaterialId),
            cancellationToken);
        if (acceptedCount != selectedIds.Length)
            return ClientCollectionResult.Failed(
                ClientCollectionError.MaterialNotAccepted,
                "Um dos materiais selecionados não é aceito pela cooperativa.");

        database.ClientesColetas.Add(new ClienteColeta
        {
            ClienteId = userId,
            ColetaId = command.CollectionId,
            Status = "A"
        });
        foreach (var materialId in selectedIds)
        {
            database.MateriaisColetados.Add(new MaterialColetado
            {
                MaterialId = materialId,
                ColetaId = command.CollectionId,
                ClienteId = userId,
                Status = "A"
            });
        }

        try
        {
            await database.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return ClientCollectionResult.Completed();
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return ClientCollectionResult.Failed(
                ClientCollectionError.AlreadyScheduled,
                "A coleta já foi agendada.");
        }
    }

    public async Task<ClientCollectionResult> CancelAsync(
        string userId,
        int collectionId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await database.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var collection = await database.ClientesColetas
            .Include(x => x.Coleta)
            .Include(x => x.Materiais)
            .SingleOrDefaultAsync(
                x => x.ColetaId == collectionId && x.ClienteId == userId,
                cancellationToken);
        if (collection is null)
            return ClientCollectionResult.Failed(
                ClientCollectionError.CollectionNotFound, "Coleta não encontrada.");
        if (collection.Status != "A" || collection.Coleta?.Status != "A")
            return ClientCollectionResult.Failed(
                ClientCollectionError.CannotCancel,
                "A coleta já foi iniciada ou finalizada e não pode ser cancelada.");

        database.MateriaisColetados.RemoveRange(collection.Materiais);
        database.ClientesColetas.Remove(collection);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ClientCollectionResult.Completed();
    }
}
