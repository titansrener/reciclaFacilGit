using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReciclaFacil.Core.Data;
using ReciclaFacil.Core.Models;

namespace ReciclaFacil.Core.Controllers;

[Authorize(Roles = "Admin")]
public sealed class AdministradorController(ReciclaFacilDbContext database) : Controller
{
    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(Materiais));

    [HttpGet]
    public async Task<IActionResult> Materiais(CancellationToken cancellationToken)
    {
        var materiais = await database.Materiais.AsNoTracking()
            .OrderBy(x => x.Descricao)
            .Select(x => new MaterialListItemViewModel(
                x.Id, x.Descricao, x.TempoMedioDecomposicao))
            .ToListAsync(cancellationToken);
        return View(materiais);
    }

    [HttpGet]
    public IActionResult CriarMaterial() => View(new MaterialEditViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarMaterial(
        MaterialEditViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        database.Materiais.Add(new Material
        {
            Descricao = model.Descricao.Trim(),
            TempoMedioDecomposicao = model.TempoMedioDecomposicao,
            Selecionado = false
        });
        await database.SaveChangesAsync(cancellationToken);
        TempData["Sucesso"] = "Material cadastrado com sucesso.";
        return RedirectToAction(nameof(Materiais));
    }

    [HttpGet]
    public async Task<IActionResult> DetalheMaterial(int id, CancellationToken cancellationToken)
    {
        var material = await FindAsync(id, cancellationToken);
        return material is null ? NotFound() : View(material);
    }

    [HttpGet]
    public async Task<IActionResult> EditarMaterial(int id, CancellationToken cancellationToken)
    {
        var material = await FindAsync(id, cancellationToken);
        return material is null ? NotFound() : View(material);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarMaterial(
        MaterialEditViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var material = await database.Materiais.FindAsync([model.Id], cancellationToken);
        if (material is null)
            return NotFound();

        material.Descricao = model.Descricao.Trim();
        material.TempoMedioDecomposicao = model.TempoMedioDecomposicao;
        await database.SaveChangesAsync(cancellationToken);
        TempData["Sucesso"] = "Material atualizado com sucesso.";
        return RedirectToAction(nameof(Materiais));
    }

    [HttpGet]
    public async Task<IActionResult> ExcluirMaterial(int id, CancellationToken cancellationToken)
    {
        var material = await FindAsync(id, cancellationToken);
        return material is null ? NotFound() : View(material);
    }

    [HttpPost]
    [ActionName(nameof(ExcluirMaterial))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarExclusao(
        int id,
        CancellationToken cancellationToken)
    {
        var material = await database.Materiais.FindAsync([id], cancellationToken);
        if (material is null)
            return NotFound();

        database.Materiais.Remove(material);
        try
        {
            await database.SaveChangesAsync(cancellationToken);
            TempData["Sucesso"] = "Material excluído com sucesso.";
            return RedirectToAction(nameof(Materiais));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty,
                "O material está em uso e não pode ser excluído.");
            return View(nameof(ExcluirMaterial), new MaterialEditViewModel
            {
                Id = material.Id,
                Descricao = material.Descricao,
                TempoMedioDecomposicao = material.TempoMedioDecomposicao
            });
        }
    }

    private async Task<MaterialEditViewModel?> FindAsync(
        int id,
        CancellationToken cancellationToken) =>
        await database.Materiais.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new MaterialEditViewModel
            {
                Id = x.Id,
                Descricao = x.Descricao,
                TempoMedioDecomposicao = x.TempoMedioDecomposicao
            })
            .SingleOrDefaultAsync(cancellationToken);
}
