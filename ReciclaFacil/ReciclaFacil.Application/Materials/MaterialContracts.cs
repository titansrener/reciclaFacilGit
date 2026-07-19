namespace ReciclaFacil.Application.Materials;

public sealed record MaterialItem(
    int Id,
    string Description,
    int AverageDecompositionTime);

public interface IMaterialQueries
{
    Task<IReadOnlyList<MaterialItem>> ListAsync(
        CancellationToken cancellationToken = default);
}
