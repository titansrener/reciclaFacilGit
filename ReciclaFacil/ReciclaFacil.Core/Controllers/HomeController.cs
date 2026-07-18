using Microsoft.AspNetCore.Mvc;
using ReciclaFacil.Core.Models;
using ReciclaFacil.Core.Services;

namespace ReciclaFacil.Core.Controllers;

public sealed class HomeController(ICooperativaService cooperativas) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new HomeViewModel());

    [HttpGet]
    public IActionResult MapaCooperativa(string? razaoSocial, string? cidade, string? estado)
    {
        var resultado = cooperativas.Pesquisar(razaoSocial, cidade, estado);
        return PartialView("_Cooperativas", resultado);
    }

    [HttpGet]
    public IActionResult Erro() => View();
}
