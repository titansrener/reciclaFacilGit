using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using ReciclaFacil.Api.Security;
using ReciclaFacil.Api;
using ReciclaFacil.Application.Authentication;
using ReciclaFacil.Application.Clients;
using ReciclaFacil.Application.Cooperatives;
using ReciclaFacil.Application.Materials;
using ReciclaFacil.Application.Employees;
using ReciclaFacil.Application.Registrations;
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

var registrations = auth.MapGroup("/register")
    .WithTags("Public Registration");

registrations.MapGet("/cooperatives", async (
    IPublicRegistrationService registration,
    CancellationToken cancellationToken) =>
    TypedResults.Ok(await registration.ListCooperativesAsync(cancellationToken)))
.WithName("RegistrationCooperatives")
.WithSummary("Lista cooperativas disponíveis para o cadastro de clientes.");

registrations.MapPost("/clients", async (
    RegisterClient request,
    IPublicRegistrationService registration,
    CancellationToken cancellationToken) =>
{
    var result = await registration.RegisterClientAsync(request, cancellationToken);
    return RegistrationHttpResults.From(
        result, result.Account is null ? "" : $"/api/v1/clients/{result.Account.Id}");
})
.WithName("RegisterClient")
.WithSummary("Cria uma conta pública de cliente.")
.Produces<RegisteredAccount>(StatusCodes.Status201Created)
.ProducesValidationProblem()
.ProducesProblem(StatusCodes.Status404NotFound)
.ProducesProblem(StatusCodes.Status409Conflict);

registrations.MapPost("/cooperatives", async (
    RegisterCooperative request,
    IPublicRegistrationService registration,
    CancellationToken cancellationToken) =>
{
    var result = await registration.RegisterCooperativeAsync(request, cancellationToken);
    return RegistrationHttpResults.From(
        result, result.Account is null ? "" : $"/api/v1/cooperatives/{result.Account.Id}");
})
.WithName("RegisterCooperative")
.WithSummary("Cria uma conta pública de cooperativa.")
.Produces<RegisteredAccount>(StatusCodes.Status201Created)
.ProducesValidationProblem()
.ProducesProblem(StatusCodes.Status409Conflict);

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

api.MapGet("/clients/me/overview", async Task<Results<
    Ok<ClientOverview>, NotFound>> (
    ClaimsPrincipal user,
    IClientQueries queries,
    CancellationToken cancellationToken) =>
{
    var overview = await queries.GetOverviewAsync(
        user.FindFirstValue("sub")!, cancellationToken);
    return overview is null
        ? TypedResults.NotFound()
        : TypedResults.Ok(overview);
})
.RequireAuthorization(policy => policy.RequireRole("Cliente"))
.WithName("ClientOverview")
.WithSummary("Retorna o resumo autenticado do cliente.");

var clientCollections = api.MapGroup("/clients/me")
    .RequireAuthorization(policy => policy.RequireRole("Cliente"))
    .WithTags("Client Collections");

clientCollections.MapGet("/collection-options", async Task<Results<
    Ok<ClientCollectionOptions>, NotFound>> (
    ClaimsPrincipal user,
    IClientCollectionService collections,
    CancellationToken cancellationToken) =>
{
    var options = await collections.GetOptionsAsync(
        user.FindFirstValue("sub")!, cancellationToken);
    return options is null
        ? TypedResults.NotFound()
        : TypedResults.Ok(options);
})
.WithName("ClientCollectionOptions")
.WithSummary("Lista horários e materiais disponíveis para o cliente.");

clientCollections.MapGet("/collections/{id:int}", async Task<Results<
    Ok<ClientCollectionDetails>, NotFound>> (
    int id,
    ClaimsPrincipal user,
    IClientCollectionService collections,
    CancellationToken cancellationToken) =>
{
    var details = await collections.GetDetailsAsync(
        user.FindFirstValue("sub")!, id, cancellationToken);
    return details is null
        ? TypedResults.NotFound()
        : TypedResults.Ok(details);
})
.WithName("ClientCollectionDetails")
.WithSummary("Retorna uma coleta pertencente ao cliente autenticado.");

clientCollections.MapPost("/collections", async (
    ScheduleClientCollectionRequest request,
    ClaimsPrincipal user,
    IClientCollectionService collections,
    CancellationToken cancellationToken) =>
{
    var result = await collections.ScheduleAsync(
        user.FindFirstValue("sub")!,
        new(request.CollectionId, request.MaterialIds ?? []),
        cancellationToken);
    return ClientCollectionHttpResults.From(
        result, $"/api/v1/clients/me/collections/{request.CollectionId}");
})
.WithName("ScheduleClientCollection")
.WithSummary("Agenda uma coleta com os materiais selecionados.")
.Produces(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status404NotFound)
.ProducesProblem(StatusCodes.Status409Conflict);

clientCollections.MapDelete("/collections/{id:int}", async (
    int id,
    ClaimsPrincipal user,
    IClientCollectionService collections,
    CancellationToken cancellationToken) =>
{
    var result = await collections.CancelAsync(
        user.FindFirstValue("sub")!, id, cancellationToken);
    return ClientCollectionHttpResults.From(result);
})
.WithName("CancelClientCollection")
.WithSummary("Cancela uma coleta ainda não iniciada.")
.Produces(StatusCodes.Status204NoContent)
.ProducesProblem(StatusCodes.Status404NotFound)
.ProducesProblem(StatusCodes.Status409Conflict);

clientCollections.MapPut("/collections/{id:int}/offer/accept", async (
    int id,
    ClaimsPrincipal user,
    IClientFinancialService financial,
    CancellationToken cancellationToken) =>
    ClientFinancialHttpResults.From(await financial.AcceptOfferAsync(
        user.FindFirstValue("sub")!, id, cancellationToken)))
.WithName("AcceptCollectionOffer")
.WithSummary("Aceita o valor calculado e credita a carteira.");

clientCollections.MapPut("/collections/{id:int}/offer/reject", async (
    int id,
    ClaimsPrincipal user,
    IClientFinancialService financial,
    CancellationToken cancellationToken) =>
    ClientFinancialHttpResults.From(await financial.RejectOfferAsync(
        user.FindFirstValue("sub")!, id, cancellationToken)))
.WithName("RejectCollectionOffer")
.WithSummary("Recusa o valor e devolve o atendimento ao funcionário.");

clientCollections.MapPut("/notifications/{id:int}/read", async (
    int id,
    ClaimsPrincipal user,
    IClientFinancialService financial,
    CancellationToken cancellationToken) =>
    ClientFinancialHttpResults.From(await financial.ReadNotificationAsync(
        user.FindFirstValue("sub")!, id, cancellationToken)))
.WithName("ReadClientNotification");

var cooperative = api.MapGroup("/cooperatives/me")
    .RequireAuthorization(policy => policy.RequireRole("Cooperativa"))
    .WithTags("Cooperative Management");

cooperative.MapGet("/overview", async Task<Results<Ok<CooperativeOverview>, NotFound>> (
    ClaimsPrincipal user,
    ICooperativeOperations operations,
    CancellationToken cancellationToken) =>
{
    var overview = await operations.GetOverviewAsync(user.FindFirstValue("sub")!, cancellationToken);
    return overview is null ? TypedResults.NotFound() : TypedResults.Ok(overview);
})
.WithName("CooperativeOverview");

cooperative.MapGet("/collections/{id:int}", async Task<Results<Ok<CooperativeCollectionDetails>, NotFound>> (
    int id,
    ClaimsPrincipal user,
    ICooperativeOperations operations,
    CancellationToken cancellationToken) =>
{
    var details = await operations.GetCollectionAsync(user.FindFirstValue("sub")!, id, cancellationToken);
    return details is null ? TypedResults.NotFound() : TypedResults.Ok(details);
})
.WithName("CooperativeCollectionDetails");

cooperative.MapPost("/collections", async (
    SaveCooperativeCollection request,
    ClaimsPrincipal user,
    ICooperativeOperations operations,
    CancellationToken cancellationToken) =>
{
    var result = await operations.CreateCollectionAsync(
        user.FindFirstValue("sub")!, request, cancellationToken);
    return CooperativeHttpResults.From(
        result, result.CollectionId is null ? null : $"/api/v1/cooperatives/me/collections/{result.CollectionId}");
})
.WithName("CreateCooperativeCollection");

cooperative.MapPut("/collections/{id:int}", async (
    int id,
    SaveCooperativeCollection request,
    ClaimsPrincipal user,
    ICooperativeOperations operations,
    CancellationToken cancellationToken) =>
    CooperativeHttpResults.From(await operations.UpdateCollectionAsync(
        user.FindFirstValue("sub")!, id, request, cancellationToken)))
.WithName("UpdateCooperativeCollection");

cooperative.MapDelete("/collections/{id:int}", async (
    int id,
    ClaimsPrincipal user,
    ICooperativeOperations operations,
    CancellationToken cancellationToken) =>
    CooperativeHttpResults.From(await operations.DeleteCollectionAsync(
        user.FindFirstValue("sub")!, id, cancellationToken)))
.WithName("DeleteCooperativeCollection");

cooperative.MapPost("/collections/{id:int}/start", async (
    int id,
    ClaimsPrincipal user,
    ICooperativeOperations operations,
    CancellationToken cancellationToken) =>
    CooperativeHttpResults.From(await operations.StartCollectionAsync(
        user.FindFirstValue("sub")!, id, cancellationToken)))
.WithName("StartCooperativeCollection");

cooperative.MapPost("/collections/{id:int}/finish", async (
    int id,
    ClaimsPrincipal user,
    ICooperativeOperations operations,
    CancellationToken cancellationToken) =>
    CooperativeHttpResults.From(await operations.FinishCollectionAsync(
        user.FindFirstValue("sub")!, id, cancellationToken)))
.WithName("FinishCooperativeCollection");

cooperative.MapPost("/materials", async (
    SaveManagedMaterial request,
    ClaimsPrincipal user,
    ICooperativeOperations operations,
    CancellationToken cancellationToken) =>
    CooperativeHttpResults.From(await operations.AddMaterialAsync(
        user.FindFirstValue("sub")!, request, cancellationToken)))
.WithName("AddCooperativeMaterial");

cooperative.MapPut("/materials/{id:int}", async (
    int id,
    UpdateManagedMaterialRequest request,
    ClaimsPrincipal user,
    ICooperativeOperations operations,
    CancellationToken cancellationToken) =>
    CooperativeHttpResults.From(await operations.UpdateMaterialAsync(
        user.FindFirstValue("sub")!, new(id, request.ResalePrice), cancellationToken)))
.WithName("UpdateCooperativeMaterial");

cooperative.MapDelete("/materials/{id:int}", async (
    int id,
    ClaimsPrincipal user,
    ICooperativeOperations operations,
    CancellationToken cancellationToken) =>
    CooperativeHttpResults.From(await operations.RemoveMaterialAsync(
        user.FindFirstValue("sub")!, id, cancellationToken)))
.WithName("RemoveCooperativeMaterial");

cooperative.MapGet("/resources", async Task<Results<Ok<CooperativeResources>, NotFound>> (
    ClaimsPrincipal user,
    ICooperativeResourceService resources,
    CancellationToken cancellationToken) =>
{
    var result = await resources.ListAsync(user.FindFirstValue("sub")!, cancellationToken);
    return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
})
.WithName("CooperativeResources");

cooperative.MapPost("/trucks", async (
    SaveTruck request,
    ClaimsPrincipal user,
    ICooperativeResourceService resources,
    CancellationToken cancellationToken) =>
{
    var result = await resources.CreateTruckAsync(user.FindFirstValue("sub")!, request, cancellationToken);
    return CooperativeResourceHttpResults.From(
        result, result.TruckId is null ? null : $"/api/v1/cooperatives/me/trucks/{result.TruckId}");
})
.WithName("CreateTruck");

cooperative.MapPut("/trucks/{id:int}", async (
    int id, SaveTruck request, ClaimsPrincipal user,
    ICooperativeResourceService resources, CancellationToken cancellationToken) =>
    CooperativeResourceHttpResults.From(await resources.UpdateTruckAsync(
        user.FindFirstValue("sub")!, id, request, cancellationToken)))
.WithName("UpdateTruck");

cooperative.MapDelete("/trucks/{id:int}", async (
    int id, ClaimsPrincipal user,
    ICooperativeResourceService resources, CancellationToken cancellationToken) =>
    CooperativeResourceHttpResults.From(await resources.DeleteTruckAsync(
        user.FindFirstValue("sub")!, id, cancellationToken)))
.WithName("DeleteTruck");

cooperative.MapPost("/employees", async (
    CreateEmployee request,
    ClaimsPrincipal user,
    ICooperativeResourceService resources,
    CancellationToken cancellationToken) =>
{
    var result = await resources.CreateEmployeeAsync(user.FindFirstValue("sub")!, request, cancellationToken);
    return CooperativeResourceHttpResults.From(
        result, result.EmployeeId is null ? null : $"/api/v1/cooperatives/me/employees/{result.EmployeeId}");
})
.WithName("CreateEmployee");

cooperative.MapPut("/employees/{id}", async (
    string id, UpdateEmployee request, ClaimsPrincipal user,
    ICooperativeResourceService resources, CancellationToken cancellationToken) =>
    CooperativeResourceHttpResults.From(await resources.UpdateEmployeeAsync(
        user.FindFirstValue("sub")!, id, request, cancellationToken)))
.WithName("UpdateEmployee");

cooperative.MapDelete("/employees/{id}", async (
    string id, ClaimsPrincipal user,
    ICooperativeResourceService resources, CancellationToken cancellationToken) =>
    CooperativeResourceHttpResults.From(await resources.DeleteEmployeeAsync(
        user.FindFirstValue("sub")!, id, cancellationToken)))
.WithName("DeleteEmployee");

cooperative.MapPut("/collections/{collectionId:int}/trucks/{truckId:int}", async (
    int collectionId, int truckId, ClaimsPrincipal user,
    ICooperativeResourceService resources, CancellationToken cancellationToken) =>
    CooperativeResourceHttpResults.From(await resources.AssignTruckAsync(
        user.FindFirstValue("sub")!, collectionId, truckId, true, cancellationToken)))
.WithName("AssignTruck");

cooperative.MapDelete("/collections/{collectionId:int}/trucks/{truckId:int}", async (
    int collectionId, int truckId, ClaimsPrincipal user,
    ICooperativeResourceService resources, CancellationToken cancellationToken) =>
    CooperativeResourceHttpResults.From(await resources.AssignTruckAsync(
        user.FindFirstValue("sub")!, collectionId, truckId, false, cancellationToken)))
.WithName("UnassignTruck");

cooperative.MapPut("/collections/{collectionId:int}/employees/{employeeId}", async (
    int collectionId, string employeeId, ClaimsPrincipal user,
    ICooperativeResourceService resources, CancellationToken cancellationToken) =>
    CooperativeResourceHttpResults.From(await resources.AssignEmployeeAsync(
        user.FindFirstValue("sub")!, collectionId, employeeId, true, cancellationToken)))
.WithName("AssignEmployee");

cooperative.MapDelete("/collections/{collectionId:int}/employees/{employeeId}", async (
    int collectionId, string employeeId, ClaimsPrincipal user,
    ICooperativeResourceService resources, CancellationToken cancellationToken) =>
    CooperativeResourceHttpResults.From(await resources.AssignEmployeeAsync(
        user.FindFirstValue("sub")!, collectionId, employeeId, false, cancellationToken)))
.WithName("UnassignEmployee");

var employee = api.MapGroup("/employees/me")
    .RequireAuthorization(policy => policy.RequireRole("Funcionario"))
    .WithTags("Employee Operations");

employee.MapGet("/overview", async Task<Results<Ok<EmployeeOverview>, NotFound>> (
    ClaimsPrincipal user,
    IEmployeeOperations operations,
    CancellationToken cancellationToken) =>
{
    var result = await operations.GetOverviewAsync(user.FindFirstValue("sub")!, cancellationToken);
    return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
})
.WithName("EmployeeOverview");

employee.MapGet("/collections/{id:int}", async Task<Results<Ok<EmployeeCollectionDetails>, NotFound>> (
    int id,
    ClaimsPrincipal user,
    IEmployeeOperations operations,
    CancellationToken cancellationToken) =>
{
    var result = await operations.GetCollectionAsync(user.FindFirstValue("sub")!, id, cancellationToken);
    return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
})
.WithName("EmployeeCollectionDetails");

employee.MapGet("/collections/{collectionId:int}/clients/{clientId}", async Task<Results<
    Ok<EmployeeClientDetails>, NotFound>> (
    int collectionId,
    string clientId,
    ClaimsPrincipal user,
    IEmployeeOperations operations,
    CancellationToken cancellationToken) =>
{
    var result = await operations.GetClientAsync(
        user.FindFirstValue("sub")!, collectionId, clientId, cancellationToken);
    return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
})
.WithName("EmployeeClientDetails");

employee.MapPut("/collections/{collectionId:int}/clients/{clientId}/materials", async (
    int collectionId,
    string clientId,
    RecordCollectedMaterials request,
    ClaimsPrincipal user,
    IEmployeeOperations operations,
    CancellationToken cancellationToken) =>
    EmployeeHttpResults.From(await operations.RecordAsync(
        user.FindFirstValue("sub")!, collectionId, clientId, request, cancellationToken)))
.WithName("RecordCollectedMaterials");

var administration = api.MapGroup("/admin")
    .RequireAuthorization(policy => policy.RequireRole("Admin"))
    .WithTags("Administration");

administration.MapGet("/materials", async (
    IMaterialQueries queries,
    CancellationToken cancellationToken) =>
    TypedResults.Ok(await queries.ListAsync(cancellationToken)))
.WithName("AdminListMaterials");

administration.MapPost("/materials", async (
    SaveMaterial request,
    IMaterialManagementService management,
    CancellationToken cancellationToken) =>
{
    var result = await management.CreateAsync(request, cancellationToken);
    return MaterialManagementHttpResults.From(
        result, result.Id is null ? null : $"/api/v1/admin/materials/{result.Id}");
})
.WithName("AdminCreateMaterial");

administration.MapPut("/materials/{id:int}", async (
    int id,
    SaveMaterial request,
    IMaterialManagementService management,
    CancellationToken cancellationToken) =>
    MaterialManagementHttpResults.From(
        await management.UpdateAsync(id, request, cancellationToken)))
.WithName("AdminUpdateMaterial");

administration.MapDelete("/materials/{id:int}", async (
    int id,
    IMaterialManagementService management,
    CancellationToken cancellationToken) =>
    MaterialManagementHttpResults.From(
        await management.DeleteAsync(id, cancellationToken)))
.WithName("AdminDeleteMaterial");

app.Run();

public partial class Program;

public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record ScheduleClientCollectionRequest(
    int CollectionId,
    int[]? MaterialIds);
public sealed record UpdateManagedMaterialRequest(decimal? ResalePrice);
public sealed record WebSessionResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    AuthenticatedUser User)
{
    public static WebSessionResponse From(TokenPair pair) =>
        new(pair.AccessToken, pair.AccessTokenExpiresAt, pair.User);
}
