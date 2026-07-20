using Microsoft.EntityFrameworkCore;
using ReciclaFacil.Application.Cooperatives;
using ReciclaFacil.Infrastructure.Data;

namespace ReciclaFacil.Infrastructure.Services;

public sealed class CooperativeOperations(ReciclaFacilDbContext database) : ICooperativeOperations
{
    public async Task<CooperativeOverview?> GetOverviewAsync(
        string cooperativeId, CancellationToken cancellationToken = default)
    {
        var cooperative = await database.Cooperativas.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == cooperativeId, cancellationToken);
        if (cooperative is null) return null;

        var collections = await database.Coletas.AsNoTracking()
            .Where(x => x.CooperativaId == cooperativeId)
            .OrderByDescending(x => x.HoraAgendada)
            .Select(x => new CooperativeCollectionSummary(
                x.Id, x.HoraAgendada, x.Status, x.Clientes.Count, x.Quantidade))
            .ToArrayAsync(cancellationToken);
        var materials = await database.MateriaisComercializados.AsNoTracking()
            .Where(x => x.CooperativaId == cooperativeId)
            .OrderBy(x => x.Material!.Descricao)
            .Select(x => new ManagedMaterial(x.MaterialId, x.Material!.Descricao, x.ValorRevenda))
            .ToArrayAsync(cancellationToken);
        var materialIds = materials.Select(x => x.Id).ToArray();
        var options = await database.Materiais.AsNoTracking()
            .Where(x => !materialIds.Contains(x.Id))
            .OrderBy(x => x.Descricao)
            .Select(x => new MaterialOption(x.Id, x.Descricao))
            .ToArrayAsync(cancellationToken);
        var clientCount = await database.Clientes.AsNoTracking()
            .CountAsync(x => x.CooperativaId == cooperativeId, cancellationToken);

        return new(
            cooperative.Id, cooperative.RazaoSocial,
            collections.Count(x => x.Status == "A"),
            collections.Count(x => x.Status == "I"),
            collections.Count(x => x.Status == "F"),
            clientCount, collections, materials, options);
    }

    public async Task<CooperativeCollectionDetails?> GetCollectionAsync(
        string cooperativeId, int collectionId, CancellationToken cancellationToken = default)
    {
        var collection = await database.Coletas.AsNoTracking()
            .Include(x => x.Clientes).ThenInclude(x => x.Cliente)
            .Include(x => x.Clientes).ThenInclude(x => x.Materiais).ThenInclude(x => x.Material)
            .Include(x => x.Caminhoes).ThenInclude(x => x.Caminhao)
            .Include(x => x.Funcionarios).ThenInclude(x => x.Funcionario).ThenInclude(x => x!.Usuario)
            .SingleOrDefaultAsync(x => x.Id == collectionId && x.CooperativaId == cooperativeId, cancellationToken);
        return collection is null ? null : new(
            collection.Id, collection.HoraAgendada, collection.Status, collection.Quantidade,
            collection.Clientes.OrderBy(x => x.Cliente!.Nome)
                .Select(x => new CooperativeCollectionClient(
                    x.ClienteId, x.Cliente!.Nome, x.Status, x.HoraDaColeta,
                    x.Materiais.Where(m => m.Material is not null)
                        .OrderBy(m => m.Material!.Descricao)
                        .Select(m => m.Material!.Descricao).ToArray()))
                .ToArray(),
            collection.Caminhoes.Where(x => x.Caminhao is not null)
                .Select(x => new TruckSummary(x.CaminhaoId, x.Caminhao!.Descricao, x.Caminhao.Placa))
                .OrderBy(x => x.Plate).ToArray(),
            collection.Funcionarios.Where(x => x.Funcionario is not null)
                .Select(x => new EmployeeSummary(
                    x.FuncionarioId, x.Funcionario!.Nome, x.Funcionario.DataNascimento,
                    x.Funcionario.Usuario!.Email ?? string.Empty))
                .OrderBy(x => x.Name).ToArray());
    }

    public async Task<CooperativeOperationResult> CreateCollectionAsync(
        string cooperativeId, SaveCooperativeCollection command, CancellationToken cancellationToken = default)
    {
        if (!ValidDate(command.ScheduledAt)) return new(CooperativeOperationError.InvalidDate);
        if (!await database.Cooperativas.AnyAsync(x => x.Id == cooperativeId, cancellationToken))
            return new(CooperativeOperationError.NotFound);
        var collection = new Coleta
        {
            CooperativaId = cooperativeId,
            HoraAgendada = command.ScheduledAt,
            Status = "A"
        };
        database.Coletas.Add(collection);
        await database.SaveChangesAsync(cancellationToken);
        return new(CollectionId: collection.Id);
    }

    public async Task<CooperativeOperationResult> UpdateCollectionAsync(
        string cooperativeId, int collectionId, SaveCooperativeCollection command, CancellationToken cancellationToken = default)
    {
        if (!ValidDate(command.ScheduledAt)) return new(CooperativeOperationError.InvalidDate);
        var collection = await FindCollection(cooperativeId, collectionId, cancellationToken);
        if (collection is null) return new(CooperativeOperationError.NotFound);
        if (collection.Status != "A") return new(CooperativeOperationError.InvalidStatus);
        collection.HoraAgendada = command.ScheduledAt;
        await database.SaveChangesAsync(cancellationToken);
        return new(CollectionId: collection.Id);
    }

    public async Task<CooperativeOperationResult> DeleteCollectionAsync(
        string cooperativeId, int collectionId, CancellationToken cancellationToken = default)
    {
        var collection = await database.Coletas.Include(x => x.Clientes)
            .SingleOrDefaultAsync(x => x.Id == collectionId && x.CooperativaId == cooperativeId, cancellationToken);
        if (collection is null) return new(CooperativeOperationError.NotFound);
        if (collection.Status != "A") return new(CooperativeOperationError.InvalidStatus);
        if (collection.Clientes.Count != 0) return new(CooperativeOperationError.HasClients);
        database.Coletas.Remove(collection);
        await database.SaveChangesAsync(cancellationToken);
        return new();
    }

    public Task<CooperativeOperationResult> StartCollectionAsync(
        string cooperativeId, int collectionId, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(cooperativeId, collectionId, "A", "I", "A coleta foi iniciada.", cancellationToken);

    public async Task<CooperativeOperationResult> FinishCollectionAsync(
        string cooperativeId, int collectionId, CancellationToken cancellationToken = default)
    {
        var collection = await database.Coletas
            .Include(x => x.Clientes).ThenInclude(x => x.Materiais)
            .SingleOrDefaultAsync(x => x.Id == collectionId && x.CooperativaId == cooperativeId, cancellationToken);
        if (collection is null) return new(CooperativeOperationError.NotFound);
        if (collection.Status != "I") return new(CooperativeOperationError.InvalidStatus);

        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        collection.Status = "F";
        foreach (var client in collection.Clientes.Where(x => x.Status == "A"))
        {
            client.Status = "N";
            foreach (var material in client.Materiais) material.Status = "N";
        }
        AddNotifications(collection, "A coleta foi finalizada.");
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new();
    }

    public async Task<CooperativeOperationResult> AddMaterialAsync(
        string cooperativeId, SaveManagedMaterial command, CancellationToken cancellationToken = default)
    {
        if (command.ResalePrice < 0) return new(CooperativeOperationError.InvalidPrice);
        if (!await database.Cooperativas.AnyAsync(x => x.Id == cooperativeId, cancellationToken) ||
            !await database.Materiais.AnyAsync(x => x.Id == command.MaterialId, cancellationToken))
            return new(CooperativeOperationError.NotFound);
        if (await database.MateriaisComercializados.AnyAsync(
                x => x.CooperativaId == cooperativeId && x.MaterialId == command.MaterialId, cancellationToken))
            return new(CooperativeOperationError.AlreadyExists);
        database.MateriaisComercializados.Add(new()
        {
            CooperativaId = cooperativeId,
            MaterialId = command.MaterialId,
            ValorRevenda = command.ResalePrice
        });
        await database.SaveChangesAsync(cancellationToken);
        return new();
    }

    public async Task<CooperativeOperationResult> UpdateMaterialAsync(
        string cooperativeId, SaveManagedMaterial command, CancellationToken cancellationToken = default)
    {
        if (command.ResalePrice < 0) return new(CooperativeOperationError.InvalidPrice);
        var material = await database.MateriaisComercializados.SingleOrDefaultAsync(
            x => x.CooperativaId == cooperativeId && x.MaterialId == command.MaterialId, cancellationToken);
        if (material is null) return new(CooperativeOperationError.NotFound);
        material.ValorRevenda = command.ResalePrice;
        await database.SaveChangesAsync(cancellationToken);
        return new();
    }

    public async Task<CooperativeOperationResult> RemoveMaterialAsync(
        string cooperativeId, int materialId, CancellationToken cancellationToken = default)
    {
        var material = await database.MateriaisComercializados.SingleOrDefaultAsync(
            x => x.CooperativaId == cooperativeId && x.MaterialId == materialId, cancellationToken);
        if (material is null) return new(CooperativeOperationError.NotFound);
        var inUse = await database.MateriaisColetados.AnyAsync(
            x => x.MaterialId == materialId && x.ClienteColeta!.Coleta!.CooperativaId == cooperativeId,
            cancellationToken);
        if (inUse) return new(CooperativeOperationError.MaterialInUse);
        database.MateriaisComercializados.Remove(material);
        await database.SaveChangesAsync(cancellationToken);
        return new();
    }

    private Task<Coleta?> FindCollection(string cooperativeId, int collectionId, CancellationToken cancellationToken) =>
        database.Coletas.SingleOrDefaultAsync(
            x => x.Id == collectionId && x.CooperativaId == cooperativeId, cancellationToken);

    private static bool ValidDate(DateTime value) =>
        value > DateTime.Now && value.Minute == 0 && value.Second == 0 &&
        value.Hour is >= 8 and <= 18;

    private async Task<CooperativeOperationResult> ChangeStatusAsync(
        string cooperativeId, int collectionId, string expected, string next,
        string notification, CancellationToken cancellationToken)
    {
        var collection = await database.Coletas.Include(x => x.Clientes)
            .SingleOrDefaultAsync(x => x.Id == collectionId && x.CooperativaId == cooperativeId, cancellationToken);
        if (collection is null) return new(CooperativeOperationError.NotFound);
        if (collection.Status != expected) return new(CooperativeOperationError.InvalidStatus);
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        collection.Status = next;
        AddNotifications(collection, notification);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new();
    }

    private void AddNotifications(Coleta collection, string description)
    {
        foreach (var client in collection.Clientes)
            database.Notificacoes.Add(new()
            {
                ClienteId = client.ClienteId,
                ColetaId = collection.Id,
                CooperativaId = collection.CooperativaId,
                DataHorario = DateTime.Now,
                Ativa = true,
                Descricao = description,
                Tipo = "C"
            });
    }
}
