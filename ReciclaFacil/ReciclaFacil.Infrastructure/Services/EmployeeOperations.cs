using Microsoft.EntityFrameworkCore;
using ReciclaFacil.Application.Employees;
using ReciclaFacil.Infrastructure.Data;

namespace ReciclaFacil.Infrastructure.Services;

public sealed class EmployeeOperations(ReciclaFacilDbContext database) : IEmployeeOperations
{
    public async Task<EmployeeOverview?> GetOverviewAsync(
        string employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await database.Funcionarios.AsNoTracking()
            .Include(x => x.Cooperativa)
            .SingleOrDefaultAsync(x => x.Id == employeeId, cancellationToken);
        if (employee is null) return null;
        var collections = await database.FuncionariosColetas.AsNoTracking()
            .Where(x => x.FuncionarioId == employeeId)
            .OrderByDescending(x => x.Coleta!.HoraAgendada)
            .Select(x => new EmployeeCollectionSummary(
                x.ColetaId, x.Coleta!.HoraAgendada, x.Coleta.Status,
                x.Coleta.Clientes.Count(c => c.Status == "A"),
                x.Coleta.Clientes.Count(c => c.Status == "S")))
            .ToArrayAsync(cancellationToken);
        return new(employee.Id, employee.Nome, employee.Cooperativa!.RazaoSocial, collections);
    }

    public async Task<EmployeeCollectionDetails?> GetCollectionAsync(
        string employeeId, int collectionId, CancellationToken cancellationToken = default)
    {
        var assigned = await database.FuncionariosColetas.AsNoTracking()
            .AnyAsync(x => x.FuncionarioId == employeeId && x.ColetaId == collectionId, cancellationToken);
        if (!assigned) return null;
        var collection = await database.Coletas.AsNoTracking()
            .Include(x => x.Caminhoes).ThenInclude(x => x.Caminhao)
            .Include(x => x.Clientes).ThenInclude(x => x.Cliente)
            .SingleAsync(x => x.Id == collectionId, cancellationToken);
        return new(
            collection.Id, collection.HoraAgendada, collection.Status,
            collection.Caminhoes.Where(x => x.Caminhao is not null)
                .Select(x => new EmployeeTruck(x.CaminhaoId, x.Caminhao!.Placa, x.Caminhao.Descricao))
                .OrderBy(x => x.Plate).ToArray(),
            collection.Clientes.Where(x => x.Cliente is not null)
                .Select(x => new EmployeeClientSummary(
                    x.ClienteId, x.Cliente!.Nome, x.Cliente.Tipo, x.Cliente.Endereco,
                    x.Cliente.Celular, x.Status,
                    x.Cliente.EnderecoCoordenada?.Y, x.Cliente.EnderecoCoordenada?.X))
                .OrderBy(x => x.Status).ThenBy(x => x.Name).ToArray());
    }

    public async Task<EmployeeClientDetails?> GetClientAsync(
        string employeeId, int collectionId, string clientId,
        CancellationToken cancellationToken = default)
    {
        var assigned = await database.FuncionariosColetas.AsNoTracking()
            .AnyAsync(x => x.FuncionarioId == employeeId && x.ColetaId == collectionId, cancellationToken);
        if (!assigned) return null;
        var link = await database.ClientesColetas.AsNoTracking()
            .Include(x => x.Cliente)
            .Include(x => x.Coleta)
            .Include(x => x.Materiais).ThenInclude(x => x.Material)
            .SingleOrDefaultAsync(x => x.ColetaId == collectionId && x.ClienteId == clientId, cancellationToken);
        return link is null ? null : new(
            link.ClienteId, link.Cliente!.Nome, link.Cliente.Tipo, link.Cliente.Endereco,
            link.Cliente.Telefone, link.Cliente.Celular, link.ColetaId, link.Coleta!.Status,
            link.Status, link.HoraDaColeta,
            link.Materiais.Where(x => x.Material is not null)
                .Select(x => new EmployeeMaterial(
                    x.MaterialId, x.Material!.Descricao, x.Quantidade, x.ValorCompra, x.Status))
                .OrderBy(x => x.Description).ToArray());
    }

    public async Task<EmployeeCollectionRoute?> GetRouteAsync(
        string employeeId, int collectionId, CancellationToken cancellationToken = default)
    {
        var employee = await database.Funcionarios.AsNoTracking()
            .Where(x => x.Id == employeeId)
            .Select(x => new
            {
                x.CooperativaId,
                Name = x.Cooperativa!.RazaoSocial,
                Address = x.Cooperativa.Endereco,
                Latitude = x.Cooperativa.EnderecoCoordenada == null
                    ? (double?)null : x.Cooperativa.EnderecoCoordenada.Y,
                Longitude = x.Cooperativa.EnderecoCoordenada == null
                    ? (double?)null : x.Cooperativa.EnderecoCoordenada.X
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (employee is null) return null;

        var assigned = await database.FuncionariosColetas.AsNoTracking()
            .AnyAsync(x => x.FuncionarioId == employeeId && x.ColetaId == collectionId,
                cancellationToken);
        if (!assigned) return null;

        var stops = await database.ClientesColetas.AsNoTracking()
            .Where(x => x.ColetaId == collectionId && x.Status == "A" && x.Cliente != null)
            .Select(x => new RouteCandidate(
                x.ClienteId,
                x.Cliente!.Nome,
                x.Cliente.Endereco,
                x.Cliente.Celular,
                x.Cliente.EnderecoCoordenada == null
                    ? (double?)null : x.Cliente.EnderecoCoordenada.Y,
                x.Cliente.EnderecoCoordenada == null
                    ? (double?)null : x.Cliente.EnderecoCoordenada.X))
            .ToArrayAsync(cancellationToken);

        var ordered = OrderStops(stops, employee.Latitude, employee.Longitude)
            .Select((x, index) => new EmployeeRouteStop(
                index + 1, x.ClientId, x.Name, x.Address, x.Mobile, x.Latitude, x.Longitude))
            .ToArray();
        return new(
            collectionId,
            new(employee.Name, employee.Address, employee.Latitude, employee.Longitude),
            ordered,
            ordered.Count(x => x.Latitude is null || x.Longitude is null));
    }

    public async Task<EmployeeOperationResult> RecordAsync(
        string employeeId, int collectionId, string clientId,
        RecordCollectedMaterials command, CancellationToken cancellationToken = default)
    {
        var employee = await database.Funcionarios.SingleOrDefaultAsync(
            x => x.Id == employeeId, cancellationToken);
        if (employee is null) return new(EmployeeOperationError.NotFound);
        var assigned = await database.FuncionariosColetas.AnyAsync(
            x => x.FuncionarioId == employeeId && x.ColetaId == collectionId, cancellationToken);
        if (!assigned) return new(EmployeeOperationError.NotFound);
        var link = await database.ClientesColetas
            .Include(x => x.Cliente)
            .Include(x => x.Coleta)
            .Include(x => x.Materiais)
            .SingleOrDefaultAsync(x => x.ColetaId == collectionId && x.ClienteId == clientId, cancellationToken);
        if (link is null) return new(EmployeeOperationError.NotFound);
        if (link.Coleta!.Status != "I") return new(EmployeeOperationError.CollectionNotStarted);
        if (link.Status != "A") return new(EmployeeOperationError.ClientAlreadyProcessed);

        var submitted = command.Materials ?? [];
        if (submitted.Count == 0 || submitted.Any(x => x.Quantity < 0 || !double.IsFinite(x.Quantity)) ||
            submitted.Select(x => x.MaterialId).Distinct().Count() != submitted.Count ||
            !submitted.Select(x => x.MaterialId).Order().SequenceEqual(
                link.Materiais.Select(x => x.MaterialId).Order()))
            return new(EmployeeOperationError.InvalidMaterials);

        Dictionary<int, decimal?> prices = [];
        if (link.Cliente!.Tipo == "V")
        {
            prices = await database.MateriaisComercializados.AsNoTracking()
                .Where(x => x.CooperativaId == employee.CooperativaId &&
                            submitted.Select(m => m.MaterialId).Contains(x.MaterialId))
                .ToDictionaryAsync(x => x.MaterialId, x => x.ValorRevenda, cancellationToken);
            if (submitted.Any(x => !prices.TryGetValue(x.MaterialId, out var value) || value is null))
                return new(EmployeeOperationError.MissingPrice);
        }

        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        decimal total = 0;
        foreach (var material in link.Materiais)
        {
            var quantity = submitted.Single(x => x.MaterialId == material.MaterialId).Quantity;
            material.Quantidade = quantity;
            material.Status = quantity > 0 ? "S" : "N";
            material.ValorCompra = link.Cliente.Tipo == "V"
                ? prices[material.MaterialId]!.Value * (decimal)quantity
                : 0;
            total += material.ValorCompra ?? 0;
        }
        link.Status = submitted.Any(x => x.Quantity > 0)
            ? link.Cliente.Tipo == "V" ? "P" : "S"
            : "N";
        link.HoraDaColeta = DateTime.Now;
        if (link.Cliente.Tipo == "V" && link.Status == "P")
            database.Notificacoes.Add(new()
            {
                ClienteId = link.ClienteId,
                ColetaId = collectionId,
                CooperativaId = employee.CooperativaId,
                DataHorario = DateTime.Now,
                Ativa = true,
                Descricao = $"Coleta realizada. Valor total a receber: R$ {total:N2}.",
                Tipo = "C"
            });
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(TotalValue: total);
    }

    private static IReadOnlyList<RouteCandidate> OrderStops(
        IReadOnlyCollection<RouteCandidate> candidates, double? originLatitude,
        double? originLongitude)
    {
        var located = candidates
            .Where(x => x.Latitude is not null && x.Longitude is not null)
            .ToList();
        List<RouteCandidate> result = [];
        var latitude = originLatitude;
        var longitude = originLongitude;
        while (located.Count > 0)
        {
            var next = latitude is null || longitude is null
                ? located.OrderBy(x => x.Name).First()
                : located.MinBy(x => DistanceSquared(
                    latitude.Value, longitude.Value, x.Latitude!.Value, x.Longitude!.Value))!;
            result.Add(next);
            located.Remove(next);
            latitude = next.Latitude;
            longitude = next.Longitude;
        }
        result.AddRange(candidates
            .Where(x => x.Latitude is null || x.Longitude is null)
            .OrderBy(x => x.Name));
        return result;
    }

    private static double DistanceSquared(
        double latitude1, double longitude1, double latitude2, double longitude2)
    {
        var latitudeDelta = latitude2 - latitude1;
        var longitudeDelta = (longitude2 - longitude1) *
            Math.Cos((latitude1 + latitude2) * Math.PI / 360);
        return latitudeDelta * latitudeDelta + longitudeDelta * longitudeDelta;
    }

    private sealed record RouteCandidate(
        string ClientId,
        string Name,
        string Address,
        string Mobile,
        double? Latitude,
        double? Longitude);
}
