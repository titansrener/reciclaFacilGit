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

public sealed record SaveMaterial(
    string Description,
    int AverageDecompositionTime);

public enum MaterialManagementError
{
    None,
    NotFound,
    InvalidInput,
    DescriptionExists,
    InUse
}

public sealed record MaterialManagementResult(
    MaterialManagementError Error = MaterialManagementError.None,
    int? Id = null)
{
    public bool Succeeded => Error == MaterialManagementError.None;
}

public interface IMaterialManagementService
{
    Task<MaterialManagementResult> CreateAsync(
        SaveMaterial command, CancellationToken cancellationToken = default);
    Task<MaterialManagementResult> UpdateAsync(
        int id, SaveMaterial command, CancellationToken cancellationToken = default);
    Task<MaterialManagementResult> DeleteAsync(
        int id, CancellationToken cancellationToken = default);
}
