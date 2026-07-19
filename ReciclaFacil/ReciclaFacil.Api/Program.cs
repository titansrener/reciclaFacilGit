using Microsoft.AspNetCore.Http.HttpResults;
using ReciclaFacil.Application.Cooperatives;
using ReciclaFacil.Application.Materials;
using ReciclaFacil.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description =>
        description.RelativePath?.StartsWith("api/v1/", StringComparison.Ordinal) == true;
});
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
{
    options.AddPolicy("web", policy =>
        policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddReciclaFacilInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("web");

app.MapOpenApi("/openapi/{documentName}.json");
app.MapHealthChecks("/health");

var api = app.MapGroup("/api/v1")
    .WithGroupName("v1");

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
