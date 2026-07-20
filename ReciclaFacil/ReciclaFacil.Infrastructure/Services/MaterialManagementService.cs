using System.Data;
using Microsoft.EntityFrameworkCore;
using ReciclaFacil.Application.Materials;
using ReciclaFacil.Infrastructure.Data;

namespace ReciclaFacil.Infrastructure.Services;

public sealed class MaterialManagementService(ReciclaFacilDbContext database)
    : IMaterialManagementService
{
    public async Task<MaterialManagementResult> CreateAsync(
        SaveMaterial command, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(command);
        if (normalized is null) return new(MaterialManagementError.InvalidInput);
        await using var transaction = await database.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        if (await DescriptionExists(normalized.Value.Description, null, cancellationToken))
            return new(MaterialManagementError.DescriptionExists);
        var material = new Material
        {
            Descricao = normalized.Value.Description,
            TempoMedioDecomposicao = normalized.Value.Time,
            Selecionado = false
        };
        database.Materiais.Add(material);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(Id: material.Id);
    }

    public async Task<MaterialManagementResult> UpdateAsync(
        int id, SaveMaterial command, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(command);
        if (normalized is null) return new(MaterialManagementError.InvalidInput);
        await using var transaction = await database.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var material = await database.Materiais.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (material is null) return new(MaterialManagementError.NotFound);
        if (await DescriptionExists(normalized.Value.Description, id, cancellationToken))
            return new(MaterialManagementError.DescriptionExists);
        material.Descricao = normalized.Value.Description;
        material.TempoMedioDecomposicao = normalized.Value.Time;
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new();
    }

    public async Task<MaterialManagementResult> DeleteAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var material = await database.Materiais.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (material is null) return new(MaterialManagementError.NotFound);
        var inUse = await database.MateriaisComercializados.AnyAsync(
            x => x.MaterialId == id, cancellationToken)
            || await database.MateriaisColetados.AnyAsync(x => x.MaterialId == id, cancellationToken);
        if (inUse) return new(MaterialManagementError.InUse);
        database.Materiais.Remove(material);
        await database.SaveChangesAsync(cancellationToken);
        return new();
    }

    private Task<bool> DescriptionExists(
        string description, int? exceptId, CancellationToken cancellationToken) =>
        database.Materiais.AnyAsync(
            x => x.Descricao == description && (!exceptId.HasValue || x.Id != exceptId.Value),
            cancellationToken);

    private static (string Description, int Time)? Normalize(SaveMaterial command)
    {
        var description = command.Description.Trim();
        return description.Length is < 2 or > 50 ||
               command.AverageDecompositionTime is < 0 or > 1000000
            ? null
            : (description, command.AverageDecompositionTime);
    }
}
