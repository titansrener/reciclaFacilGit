using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using ReciclaFacil.Api.Security;
using ReciclaFacil.Application.Authentication;
using ReciclaFacil.Application.Cooperatives;
using ReciclaFacil.Application.Materials;
using ReciclaFacil.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var signingKey = SigningKeyResolver.Resolve(builder.Configuration, builder.Environment);
builder.Configuration["Jwt:SigningKey"] = signingKey;

builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description =>
        description.RelativePath?.StartsWith("api/v1/", StringComparison.Ordinal) == true;
});
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("web", policy =>
        policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});
builder.Services.AddReciclaFacilInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("web");
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi("/openapi/{documentName}.json");
app.MapHealthChecks("/health");

var api = app.MapGroup("/api/v1")
    .WithGroupName("v1");

var auth = api.MapGroup("/auth").WithTags("Authentication");

auth.MapPost("/login", async Task<Results<Ok<TokenPair>, UnauthorizedHttpResult, ValidationProblem>> (
    LoginRequest request,
    IApiAuthenticationService authentication,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        return TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            ["credentials"] = ["E-mail e senha são obrigatórios."]
        });
    var result = await authentication.LoginAsync(
        new(request.Email, request.Password), cancellationToken);
    return result is null ? TypedResults.Unauthorized() : TypedResults.Ok(result);
})
.WithName("Login")
.WithSummary("Autentica e retorna access token e refresh token.");

auth.MapPost("/refresh", async Task<Results<Ok<TokenPair>, UnauthorizedHttpResult, ValidationProblem>> (
    RefreshRequest request,
    IApiAuthenticationService authentication,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.RefreshToken))
        return TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            ["refreshToken"] = ["Refresh token é obrigatório."]
        });
    var result = await authentication.RefreshAsync(
        new(request.RefreshToken), cancellationToken);
    return result is null ? TypedResults.Unauthorized() : TypedResults.Ok(result);
})
.WithName("RefreshToken")
.WithSummary("Rotaciona o refresh token e emite um novo par.");

auth.MapPost("/revoke", async (
    RefreshRequest request,
    IApiAuthenticationService authentication,
    CancellationToken cancellationToken) =>
{
    if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        await authentication.RevokeAsync(request.RefreshToken, cancellationToken);
    return TypedResults.NoContent();
})
.WithName("RevokeToken")
.WithSummary("Revoga um refresh token.");

auth.MapGet("/me", (ClaimsPrincipal user) => TypedResults.Ok(new
{
    id = user.FindFirstValue("sub"),
    email = user.FindFirstValue("email"),
    name = user.Identity?.Name,
    role = user.FindFirstValue(ClaimTypes.Role)
}))
.RequireAuthorization()
.WithName("CurrentUser")
.WithSummary("Retorna os dados do usuário autenticado.");

var webAuth = auth.MapGroup("/web").WithTags("Web Authentication");

webAuth.MapPost("/login", async Task<Results<
    Ok<WebSessionResponse>, UnauthorizedHttpResult, BadRequest>> (
    LoginRequest request,
    HttpContext context,
    IWebHostEnvironment environment,
    IApiAuthenticationService authentication,
    CancellationToken cancellationToken) =>
{
    if (!WebSessionCookie.IsWebClient(context.Request))
        return TypedResults.BadRequest();
    var result = await authentication.LoginAsync(
        new(request.Email, request.Password), cancellationToken);
    if (result is null)
        return TypedResults.Unauthorized();
    WebSessionCookie.Write(context.Response, result, !environment.IsDevelopment());
    context.Response.Headers.CacheControl = "no-store";
    return TypedResults.Ok(WebSessionResponse.From(result));
})
.WithName("WebLogin")
.WithSummary("Autentica o frontend web e grava o refresh token em cookie HttpOnly.");

webAuth.MapPost("/refresh", async Task<Results<
    Ok<WebSessionResponse>, UnauthorizedHttpResult, BadRequest>> (
    HttpContext context,
    IWebHostEnvironment environment,
    IApiAuthenticationService authentication,
    CancellationToken cancellationToken) =>
{
    if (!WebSessionCookie.IsWebClient(context.Request))
        return TypedResults.BadRequest();
    var refreshToken = WebSessionCookie.Read(context.Request);
    if (string.IsNullOrWhiteSpace(refreshToken))
        return TypedResults.Unauthorized();
    var result = await authentication.RefreshAsync(
        new(refreshToken), cancellationToken);
    if (result is null)
    {
        WebSessionCookie.Delete(context.Response, !environment.IsDevelopment());
        return TypedResults.Unauthorized();
    }
    WebSessionCookie.Write(context.Response, result, !environment.IsDevelopment());
    context.Response.Headers.CacheControl = "no-store";
    return TypedResults.Ok(WebSessionResponse.From(result));
})
.WithName("WebRefresh")
.WithSummary("Rotaciona o refresh cookie e restaura a sessão web.");

webAuth.MapPost("/logout", async Task<Results<NoContent, BadRequest>> (
    HttpContext context,
    IWebHostEnvironment environment,
    IApiAuthenticationService authentication,
    CancellationToken cancellationToken) =>
{
    if (!WebSessionCookie.IsWebClient(context.Request))
        return TypedResults.BadRequest();
    var refreshToken = WebSessionCookie.Read(context.Request);
    if (!string.IsNullOrWhiteSpace(refreshToken))
        await authentication.RevokeAsync(refreshToken, cancellationToken);
    WebSessionCookie.Delete(context.Response, !environment.IsDevelopment());
    return TypedResults.NoContent();
})
.WithName("WebLogout")
.WithSummary("Revoga o refresh token e encerra a sessão web.");

api.MapGet("/cooperatives", async (
    string? name,
    string? city,
    string? state,
    int page,
    int pageSize,
    ICooperativeQueries queries,
    CancellationToken cancellationToken) =>
{
    var result = await queries.SearchAsync(
        new(name, city, state, page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize),
        cancellationToken);
    return TypedResults.Ok(result);
})
.WithName("SearchCooperatives")
.WithSummary("Pesquisa cooperativas com paginação.");

api.MapGet("/cooperatives/{id}", async Task<Results<
    Ok<CooperativeDetails>, NotFound>> (
    string id,
    ICooperativeQueries queries,
    CancellationToken cancellationToken) =>
{
    var cooperative = await queries.GetAsync(id, cancellationToken);
    return cooperative is null
        ? TypedResults.NotFound()
        : TypedResults.Ok(cooperative);
})
.WithName("GetCooperative")
.WithSummary("Obtém uma cooperativa e seus materiais comercializados.");

api.MapGet("/materials", async (
    IMaterialQueries queries,
    CancellationToken cancellationToken) =>
    TypedResults.Ok(await queries.ListAsync(cancellationToken)))
.WithName("ListMaterials")
.WithSummary("Lista materiais recicláveis.");

app.Run();

public partial class Program;

public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record WebSessionResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    AuthenticatedUser User)
{
    public static WebSessionResponse From(TokenPair pair) =>
        new(pair.AccessToken, pair.AccessTokenExpiresAt, pair.User);
}
