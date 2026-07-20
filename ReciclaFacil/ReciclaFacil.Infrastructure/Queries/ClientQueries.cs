using Microsoft.EntityFrameworkCore;
using ReciclaFacil.Application.Clients;
using ReciclaFacil.Infrastructure.Data;

namespace ReciclaFacil.Infrastructure.Queries;

public sealed class ClientQueries(ReciclaFacilDbContext database) : IClientQueries
{
    public async Task<ClientOverview?> GetOverviewAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var client = await database.Clientes.AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new
            {
                x.Nome,
                CooperativeName = x.Cooperativa!.RazaoSocial
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (client is null)
            return null;

        var collections = await database.ClientesColetas.AsNoTracking()
            .Where(x => x.ClienteId == userId)
            .OrderByDescending(x => x.Coleta!.HoraAgendada)
            .Select(x => new ClientCollectionSummary(
                x.ColetaId,
                x.Coleta!.HoraAgendada,
                x.HoraDaColeta,
                x.Status ?? x.Coleta.Status,
                x.Materiais.Count))
            .ToListAsync(cancellationToken);

        var walletBalance = await database.Carteiras.AsNoTracking()
            .Where(x => x.ClienteId == userId)
            .SumAsync(x => (decimal?)x.Saldo, cancellationToken) ?? 0m;

        var activeNotificationCount = await database.Notificacoes.AsNoTracking()
            .CountAsync(x => x.ClienteId == userId && x.Ativa == true, cancellationToken);
        var notifications = await database.Notificacoes.AsNoTracking()
            .Where(x => x.ClienteId == userId)
            .OrderByDescending(x => x.DataHorario)
            .Take(5)
            .Select(x => new ClientNotificationSummary(
                x.Id,
                x.ColetaId,
                x.DataHorario,
                x.Descricao,
                x.Tipo,
                x.Ativa == true,
                x.Ativa == true && database.ClientesColetas.Any(c =>
                    c.ClienteId == userId && c.ColetaId == x.ColetaId && c.Status == "P"),
                database.MateriaisColetados
                    .Where(m => m.ClienteId == userId && m.ColetaId == x.ColetaId)
                    .Sum(m => (decimal?)m.ValorCompra)))
            .ToListAsync(cancellationToken);

        return new(
            client.Nome,
            client.CooperativeName,
            walletBalance,
            activeNotificationCount,
            collections,
            notifications);
    }
}
