using Microsoft.AspNetCore.Mvc;
using ReciclaFacil.Core.Models;
using ReciclaFacil.Core.Services;

namespace ReciclaFacil.Core.Controllers;

public sealed class HomeController(ICooperativaService cooperativas) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new HomeViewModel());

    [HttpGet]
    public async Task<IActionResult> MapaCooperativa(
        string? razaoSocial,
        string? cidade,
        string? estado,
        CancellationToken cancellationToken)
    {
        var resultado = await cooperativas.PesquisarAsync(
            razaoSocial, cidade, estado, cancellationToken);
        return PartialView("_Cooperativas", resultado);
    }

    [HttpGet]
    public async Task<IActionResult> DetalheCooperativa(
        string id,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest();
        var cooperativa = await cooperativas.ObterAsync(id, cancellationToken);
        return cooperativa is null ? NotFound() : View(cooperativa);
    }

    [HttpGet]
    public IActionResult Erro() => View();
}
