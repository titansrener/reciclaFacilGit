using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReciclaFacil.Infrastructure.Data;
using ReciclaFacil.Core.Models;

namespace ReciclaFacil.Core.Controllers;

[Authorize(Roles = "Cliente")]
public sealed class ClientesController(ReciclaFacilDbContext database) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var id = CurrentUserId();
        var nome = await database.Clientes.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => x.Nome)
            .SingleOrDefaultAsync(cancellationToken);
        if (nome is null)
            return NotFound();

        var coletas = await database.ClientesColetas.AsNoTracking()
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.Coleta!.HoraAgendada)
            .Select(x => new ClienteColetaListItemViewModel(
                x.ColetaId,
                x.Coleta!.HoraAgendada,
                x.Coleta.Status,
                x.Materiais.Count))
            .ToListAsync(cancellationToken);

        return View(new ClienteDashboardViewModel(nome, coletas));
    }

    [HttpGet]
    public async Task<IActionResult> Carteira(CancellationToken cancellationToken)
    {
        var movimentos = await database.Carteiras.AsNoTracking()
            .Where(x => x.ClienteId == CurrentUserId())
            .OrderByDescending(x => x.DataUltimaMovimentacao)
            .Select(x => new CarteiraMovimentoViewModel(
                x.DataUltimaMovimentacao, x.Saldo))
            .ToListAsync(cancellationToken);
        return View(new CarteiraViewModel(
            movimentos.Sum(x => x.Valor), movimentos));
    }

    [HttpGet]
    public async Task<IActionResult> Notificacoes(CancellationToken cancellationToken)
    {
        var notificacoes = await database.Notificacoes.AsNoTracking()
            .Where(x => x.ClienteId == CurrentUserId())
            .OrderByDescending(x => x.DataHorario)
            .Select(x => new NotificacaoViewModel(
                x.Id, x.DataHorario, x.Descricao, x.Tipo, x.Ativa == true))
            .ToListAsync(cancellationToken);
        return View(notificacoes);
    }

    [HttpGet]
    public async Task<IActionResult> NotificacoesCount(CancellationToken cancellationToken)
    {
        var count = await database.Notificacoes.AsNoTracking()
            .CountAsync(x => x.ClienteId == CurrentUserId() && x.Ativa == true,
                cancellationToken);
        return Json(new { count });
    }

    [HttpGet]
    public async Task<IActionResult> AgendarColeta(CancellationToken cancellationToken) =>
        View(await BuildScheduleModelAsync(null, cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgendarColeta(
        AgendarColetaViewModel model,
        CancellationToken cancellationToken)
    {
        var clientId = CurrentUserId();
        var client = await database.Clientes.AsNoTracking()
            .Where(x => x.Id == clientId)
            .Select(x => new { x.Id, x.CooperativaId })
            .SingleOrDefaultAsync(cancellationToken);
        if (client is null)
            return NotFound();

        var slot = await database.Coletas.AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.Id == model.ColetaId &&
                x.CooperativaId == client.CooperativaId &&
                x.Status == "A" &&
                x.HoraAgendada > DateTime.Now,
                cancellationToken);
        if (slot is null)
            ModelState.AddModelError(nameof(model.ColetaId),
                "O horário selecionado não está mais disponível.");

        var alreadyScheduled = await database.ClientesColetas.AsNoTracking()
            .AnyAsync(x => x.ClienteId == clientId && x.ColetaId == model.ColetaId,
                cancellationToken);
        if (alreadyScheduled)
            ModelState.AddModelError(nameof(model.ColetaId),
                "Você já está associado a essa coleta.");

        var selectedIds = model.MateriaisSelecionados.Distinct().ToArray();
        if (selectedIds.Length == 0)
            ModelState.AddModelError(nameof(model.MateriaisSelecionados),
                "Selecione pelo menos um material.");

        var acceptedIds = await database.MateriaisComercializados.AsNoTracking()
            .Where(x => x.CooperativaId == client.CooperativaId &&
                        selectedIds.Contains(x.MaterialId))
            .Select(x => x.MaterialId)
            .ToArrayAsync(cancellationToken);
        if (acceptedIds.Length != selectedIds.Length)
            ModelState.AddModelError(nameof(model.MateriaisSelecionados),
                "Um dos materiais selecionados não é aceito pela cooperativa.");

        if (!ModelState.IsValid)
            return View(await BuildScheduleModelAsync(model, cancellationToken));

        await using var transaction = await database.Database
            .BeginTransactionAsync(cancellationToken);
        database.ClientesColetas.Add(new ClienteColeta
        {
            ClienteId = clientId,
            ColetaId = model.ColetaId,
            Status = "A"
        });
        foreach (var materialId in selectedIds)
        {
            database.MateriaisColetados.Add(new MaterialColetado
            {
                MaterialId = materialId,
                ColetaId = model.ColetaId,
                ClienteId = clientId,
                Status = "A"
            });
        }
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        TempData["Sucesso"] = "Coleta agendada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> DetalheColeta(
        int id,
        CancellationToken cancellationToken)
    {
        var model = await FindCollectionAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> CancelarColeta(
        int id,
        CancellationToken cancellationToken)
    {
        var model = await FindCollectionAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ActionName(nameof(CancelarColeta))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarCancelamento(
        int id,
        CancellationToken cancellationToken)
    {
        var clientId = CurrentUserId();
        var collection = await database.ClientesColetas
            .Include(x => x.Coleta)
            .Include(x => x.Materiais)
            .SingleOrDefaultAsync(x => x.ColetaId == id && x.ClienteId == clientId,
                cancellationToken);
        if (collection is null)
            return NotFound();
        if (collection.Status != "A" || collection.Coleta?.Status != "A")
            return Conflict("A coleta já foi iniciada ou finalizada e não pode ser cancelada.");

        await using var transaction = await database.Database
            .BeginTransactionAsync(cancellationToken);
        database.MateriaisColetados.RemoveRange(collection.Materiais);
        database.ClientesColetas.Remove(collection);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        TempData["Sucesso"] = "Agendamento cancelado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<AgendarColetaViewModel> BuildScheduleModelAsync(
        AgendarColetaViewModel? input,
        CancellationToken cancellationToken)
    {
        var clientId = CurrentUserId();
        var cooperativeId = await database.Clientes.AsNoTracking()
            .Where(x => x.Id == clientId)
            .Select(x => x.CooperativaId)
            .SingleOrDefaultAsync(cancellationToken);
        if (cooperativeId is null)
            return input ?? new();

        var scheduledIds = database.ClientesColetas.AsNoTracking()
            .Where(x => x.ClienteId == clientId)
            .Select(x => x.ColetaId);
        var slots = await database.Coletas.AsNoTracking()
            .Where(x => x.CooperativaId == cooperativeId &&
                        x.Status == "A" &&
                        x.HoraAgendada > DateTime.Now &&
                        !scheduledIds.Contains(x.Id))
            .OrderBy(x => x.HoraAgendada)
            .Select(x => new HorarioColetaViewModel(x.Id, x.HoraAgendada!.Value))
            .ToListAsync(cancellationToken);
        var materials = await database.MateriaisComercializados.AsNoTracking()
            .Where(x => x.CooperativaId == cooperativeId)
            .OrderBy(x => x.Material!.Descricao)
            .Select(x => new MaterialSelecaoViewModel(
                x.MaterialId, x.Material!.Descricao))
            .ToListAsync(cancellationToken);

        return new AgendarColetaViewModel
        {
            ColetaId = input?.ColetaId ?? 0,
            MateriaisSelecionados = input?.MateriaisSelecionados ?? [],
            Horarios = slots,
            Materiais = materials
        };
    }

    private async Task<ClienteColetaDetalheViewModel?> FindCollectionAsync(
        int id,
        CancellationToken cancellationToken) =>
        await database.ClientesColetas.AsNoTracking()
            .Where(x => x.ColetaId == id && x.ClienteId == CurrentUserId())
            .Select(x => new ClienteColetaDetalheViewModel(
                x.ColetaId,
                x.Coleta!.HoraAgendada,
                x.HoraDaColeta,
                x.Status ?? x.Coleta.Status,
                x.Materiais.OrderBy(m => m.Material!.Descricao)
                    .Select(m => new MaterialColetaViewModel(
                        m.Material!.Descricao, m.Quantidade, m.ValorCompra))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

    private string CurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Identificador do usuário ausente.");
}
