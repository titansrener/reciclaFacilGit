using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ReciclaFacil.Core.Data;
using ReciclaFacil.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<ICooperativaService, CooperativaService>();
builder.Services.AddScoped<ILegacyAuthenticationService, LegacyAuthenticationService>();
builder.Services.AddDbContext<ReciclaFacilDbContext>(options =>
{
    var template = builder.Configuration.GetConnectionString("ReciclaFacil")
        ?? throw new InvalidOperationException("A conexão ReciclaFacil não foi configurada.");
    var databaseFile = Path.GetFullPath(Path.Combine(
        builder.Environment.ContentRootPath, "..", "ReciclaFacil", "App_Data", "ReciclaFacil_DB.mdf"));
    options.UseSqlServer(
        template.Replace("{DatabaseFile}", databaseFile),
        sql => sql.UseNetTopologySuite());
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AcessoNegado";
        options.Cookie.Name = "ReciclaFacil.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddDataProtection()
    .SetApplicationName("ReciclaFacil.Core")
    .PersistKeysToFileSystem(new DirectoryInfo(
        Path.Combine(builder.Environment.ContentRootPath, ".keys")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Erro");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program;
