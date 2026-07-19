using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReciclaFacil.Core.Data;
using ReciclaFacil.Core.Models;
using ReciclaFacil.Core.Services;

namespace ReciclaFacil.Core.Controllers;

public sealed class AccountController(
    ILegacyAuthenticationService authentication,
    ReciclaFacilDbContext database,
    IPasswordHasher<LegacyUser> passwordHasher) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginViewModel { ReturnUrl = returnUrl });

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await authentication.ValidateAsync(model.Email, model.Password, cancellationToken);
        if (!result.Succeeded || result.User is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Não foi possível entrar.");
            return View(model);
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, result.User.Id),
            new Claim(ClaimTypes.Name, result.User.UserName),
            new Claim(ClaimTypes.Email, result.User.Email ?? result.User.UserName),
            new Claim(ClaimTypes.Role, result.Role ?? "Usuario")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(model.RememberMe ? 72 : 8)
            });

        return Url.IsLocalUrl(model.ReturnUrl)
            ? LocalRedirect(model.ReturnUrl!)
            : RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AcessoNegado() => View();

    [AllowAnonymous]
    [HttpGet]
    public IActionResult RegisterCooperativa() => View(new RegisterCooperativaViewModel());

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterCooperativa(
        RegisterCooperativaViewModel model,
        CancellationToken cancellationToken)
    {
        await ValidateUniqueUserAsync(model.Email, cancellationToken);
        if (await database.Cooperativas.AsNoTracking()
            .AnyAsync(x => x.Cnpj == model.Cnpj, cancellationToken))
            ModelState.AddModelError(nameof(model.Cnpj), "Este CNPJ já está cadastrado.");
        if (!ModelState.IsValid)
            return View(model);

        var id = Guid.NewGuid().ToString();
        var user = CreateUser(id, model.Email);
        user.PasswordHash = passwordHasher.HashPassword(user, model.Password);
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        database.Users.Add(user);
        database.UserRoles.Add(new LegacyUserRole { UserId = id, RoleId = "2" });
        await database.SaveChangesAsync(cancellationToken);
        database.Cooperativas.Add(new Cooperativa
        {
            Id = id,
            Cnpj = model.Cnpj,
            RazaoSocial = model.RazaoSocial.Trim(),
            Endereco = model.Endereco.Trim(),
            Cidade = model.Cidade.Trim(),
            Estado = model.Estado.Trim().ToUpperInvariant()
        });
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        TempData["ContaCriada"] = "Conta criada. Você já pode entrar.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> RegisterCliente(CancellationToken cancellationToken) =>
        View(await BuildClientModelAsync(new RegisterClienteViewModel(), cancellationToken));

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterCliente(
        RegisterClienteViewModel model,
        CancellationToken cancellationToken)
    {
        await ValidateUniqueUserAsync(model.Email, cancellationToken);
        if (!string.IsNullOrWhiteSpace(model.Cpf) &&
            await database.Clientes.AsNoTracking().AnyAsync(x => x.Cpf == model.Cpf, cancellationToken))
            ModelState.AddModelError(nameof(model.Cpf), "Este CPF já está cadastrado.");
        if (!await database.Cooperativas.AsNoTracking()
            .AnyAsync(x => x.Id == model.CooperativaId, cancellationToken))
            ModelState.AddModelError(nameof(model.CooperativaId), "Selecione uma cooperativa válida.");
        if (!ModelState.IsValid)
            return View(await BuildClientModelAsync(model, cancellationToken));

        var id = Guid.NewGuid().ToString();
        var user = CreateUser(id, model.Email);
        user.PasswordHash = passwordHasher.HashPassword(user, model.Password);
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        database.Users.Add(user);
        database.UserRoles.Add(new LegacyUserRole { UserId = id, RoleId = "1" });
        await database.SaveChangesAsync(cancellationToken);
        database.Clientes.Add(new Cliente
        {
            Id = id,
            Cpf = string.IsNullOrWhiteSpace(model.Cpf) ? null : model.Cpf,
            Tipo = model.Tipo,
            Nome = model.Nome.Trim(),
            Endereco = model.Endereco.Trim(),
            Email = model.Email.Trim(),
            Sexo = model.Sexo,
            DataNascimento = model.DataNascimento,
            Telefone = model.Telefone,
            Celular = model.Celular,
            CooperativaId = model.CooperativaId
        });
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        TempData["ContaCriada"] = "Conta criada. Você já pode entrar.";
        return RedirectToAction(nameof(Login));
    }

    private async Task ValidateUniqueUserAsync(string email, CancellationToken cancellationToken)
    {
        var normalized = email.Trim();
        if (await database.Users.AsNoTracking()
            .AnyAsync(x => x.UserName == normalized || x.Email == normalized, cancellationToken))
            ModelState.AddModelError(nameof(RegisterBaseViewModel.Email),
                "Este e-mail já está cadastrado.");
    }

    private static LegacyUser CreateUser(string id, string email) => new()
    {
        Id = id,
        Email = email.Trim(),
        UserName = email.Trim(),
        EmailConfirmed = false,
        SecurityStamp = Guid.NewGuid().ToString(),
        DataCadastro = DateTime.UtcNow,
        Ativo = true,
        Discriminator = "ApplicationUser"
    };

    private async Task<RegisterClienteViewModel> BuildClientModelAsync(
        RegisterClienteViewModel model,
        CancellationToken cancellationToken)
    {
        model.Cooperativas = await database.Cooperativas.AsNoTracking()
            .OrderBy(x => x.RazaoSocial)
            .Select(x => new CooperativaOpcaoViewModel(
                x.Id, x.RazaoSocial, x.Cidade, x.Estado))
            .ToListAsync(cancellationToken);
        return model;
    }
}
