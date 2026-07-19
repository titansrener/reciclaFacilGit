using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReciclaFacil.Core.Data;
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

    private string CurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Identificador do usuário ausente.");
}
