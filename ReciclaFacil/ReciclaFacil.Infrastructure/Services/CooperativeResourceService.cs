using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReciclaFacil.Application.Cooperatives;
using ReciclaFacil.Infrastructure.Data;

namespace ReciclaFacil.Infrastructure.Services;

public sealed class CooperativeResourceService(
    ReciclaFacilDbContext database,
    IPasswordHasher<LegacyUser> passwordHasher) : ICooperativeResourceService
{
    public async Task<CooperativeResources?> ListAsync(
        string cooperativeId, CancellationToken cancellationToken = default)
    {
        if (!await database.Cooperativas.AsNoTracking().AnyAsync(x => x.Id == cooperativeId, cancellationToken))
            return null;
        var trucks = await database.Caminhoes.AsNoTracking()
            .Where(x => x.CooperativaId == cooperativeId).OrderBy(x => x.Placa)
            .Select(x => new TruckSummary(x.Id, x.Descricao, x.Placa))
            .ToArrayAsync(cancellationToken);
        var employees = await database.Funcionarios.AsNoTracking()
            .Where(x => x.CooperativaId == cooperativeId).OrderBy(x => x.Nome)
            .Select(x => new EmployeeSummary(x.Id, x.Nome, x.DataNascimento, x.Usuario!.Email ?? string.Empty))
            .ToArrayAsync(cancellationToken);
        return new(trucks, employees);
    }

    public async Task<CooperativeResourceResult> CreateTruckAsync(
        string cooperativeId, SaveTruck command, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeTruck(command);
        if (normalized is null) return new(CooperativeResourceError.InvalidInput);
        if (!await database.Cooperativas.AnyAsync(x => x.Id == cooperativeId, cancellationToken))
            return new(CooperativeResourceError.NotFound);
        if (await database.Caminhoes.AnyAsync(x => x.Placa == normalized.Value.Plate, cancellationToken))
            return new(CooperativeResourceError.PlateExists);
        var truck = new Caminhao
        {
            CooperativaId = cooperativeId,
            Descricao = normalized.Value.Description,
            Placa = normalized.Value.Plate
        };
        database.Caminhoes.Add(truck);
        await database.SaveChangesAsync(cancellationToken);
        return new(TruckId: truck.Id);
    }

    public async Task<CooperativeResourceResult> UpdateTruckAsync(
        string cooperativeId, int truckId, SaveTruck command, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeTruck(command);
        if (normalized is null) return new(CooperativeResourceError.InvalidInput);
        var truck = await database.Caminhoes.SingleOrDefaultAsync(
            x => x.Id == truckId && x.CooperativaId == cooperativeId, cancellationToken);
        if (truck is null) return new(CooperativeResourceError.NotFound);
        if (await database.Caminhoes.AnyAsync(
                x => x.Id != truckId && x.Placa == normalized.Value.Plate, cancellationToken))
            return new(CooperativeResourceError.PlateExists);
        truck.Descricao = normalized.Value.Description;
        truck.Placa = normalized.Value.Plate;
        await database.SaveChangesAsync(cancellationToken);
        return new();
    }

    public async Task<CooperativeResourceResult> DeleteTruckAsync(
        string cooperativeId, int truckId, CancellationToken cancellationToken = default)
    {
        var truck = await database.Caminhoes.Include(x => x.Coletas).SingleOrDefaultAsync(
            x => x.Id == truckId && x.CooperativaId == cooperativeId, cancellationToken);
        if (truck is null) return new(CooperativeResourceError.NotFound);
        database.CaminhoesColetas.RemoveRange(truck.Coletas);
        database.Caminhoes.Remove(truck);
        await database.SaveChangesAsync(cancellationToken);
        return new();
    }

    public async Task<CooperativeResourceResult> CreateEmployeeAsync(
        string cooperativeId, CreateEmployee command, CancellationToken cancellationToken = default)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        if (!ValidEmployee(command.Name, command.BirthDate) ||
            string.IsNullOrWhiteSpace(email) || !email.Contains('@') || command.Password.Length < 8)
            return new(CooperativeResourceError.InvalidInput);
        if (!await database.Cooperativas.AnyAsync(x => x.Id == cooperativeId, cancellationToken))
            return new(CooperativeResourceError.NotFound);
        if (await database.Users.AnyAsync(x => x.Email == email || x.UserName == email, cancellationToken))
            return new(CooperativeResourceError.EmailExists);

        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var id = Guid.NewGuid().ToString();
        var user = new LegacyUser
        {
            Id = id,
            Email = email,
            EmailConfirmed = false,
            UserName = email,
            Ativo = true,
            DataCadastro = DateTime.Now,
            Discriminator = "Funcionario",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        user.PasswordHash = passwordHasher.HashPassword(user, command.Password);
        database.Users.Add(user);
        var role = await database.Roles.SingleOrDefaultAsync(x => x.Name == "Funcionario", cancellationToken);
        if (role is null)
        {
            role = new LegacyRole { Id = "4", Name = "Funcionario" };
            database.Roles.Add(role);
        }
        database.UserRoles.Add(new() { UserId = id, RoleId = role.Id });
        database.Funcionarios.Add(new()
        {
            Id = id,
            CooperativaId = cooperativeId,
            Nome = command.Name.Trim(),
            DataNascimento = command.BirthDate.Date
        });
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(EmployeeId: id);
    }

    public async Task<CooperativeResourceResult> UpdateEmployeeAsync(
        string cooperativeId, string employeeId, UpdateEmployee command,
        CancellationToken cancellationToken = default)
    {
        if (!ValidEmployee(command.Name, command.BirthDate))
            return new(CooperativeResourceError.InvalidInput);
        var employee = await database.Funcionarios.SingleOrDefaultAsync(
            x => x.Id == employeeId && x.CooperativaId == cooperativeId, cancellationToken);
        if (employee is null) return new(CooperativeResourceError.NotFound);
        employee.Nome = command.Name.Trim();
        employee.DataNascimento = command.BirthDate.Date;
        await database.SaveChangesAsync(cancellationToken);
        return new();
    }

    public async Task<CooperativeResourceResult> DeleteEmployeeAsync(
        string cooperativeId, string employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await database.Funcionarios.Include(x => x.Coletas).SingleOrDefaultAsync(
            x => x.Id == employeeId && x.CooperativaId == cooperativeId, cancellationToken);
        if (employee is null) return new(CooperativeResourceError.NotFound);
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        database.FuncionariosColetas.RemoveRange(employee.Coletas);
        database.Funcionarios.Remove(employee);
        database.RefreshTokens.RemoveRange(database.RefreshTokens.Where(x => x.UserId == employeeId));
        database.UserRoles.RemoveRange(database.UserRoles.Where(x => x.UserId == employeeId));
        var user = await database.Users.SingleAsync(x => x.Id == employeeId, cancellationToken);
        database.Users.Remove(user);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new();
    }

    public async Task<CooperativeResourceResult> AssignTruckAsync(
        string cooperativeId, int collectionId, int truckId, bool assign,
        CancellationToken cancellationToken = default)
    {
        var validCollection = await database.Coletas.AnyAsync(
            x => x.Id == collectionId && x.CooperativaId == cooperativeId, cancellationToken);
        var validTruck = await database.Caminhoes.AnyAsync(
            x => x.Id == truckId && x.CooperativaId == cooperativeId, cancellationToken);
        if (!validCollection || !validTruck) return new(CooperativeResourceError.NotFound);
        var link = await database.CaminhoesColetas.FindAsync([truckId, collectionId], cancellationToken);
        if (assign && link is not null) return new(CooperativeResourceError.AlreadyAssigned);
        if (!assign && link is null) return new(CooperativeResourceError.NotAssigned);
        if (assign) database.CaminhoesColetas.Add(new() { CaminhaoId = truckId, ColetaId = collectionId });
        else database.CaminhoesColetas.Remove(link!);
        await database.SaveChangesAsync(cancellationToken);
        return new();
    }

    public async Task<CooperativeResourceResult> AssignEmployeeAsync(
        string cooperativeId, int collectionId, string employeeId, bool assign,
        CancellationToken cancellationToken = default)
    {
        var validCollection = await database.Coletas.AnyAsync(
            x => x.Id == collectionId && x.CooperativaId == cooperativeId, cancellationToken);
        var validEmployee = await database.Funcionarios.AnyAsync(
            x => x.Id == employeeId && x.CooperativaId == cooperativeId, cancellationToken);
        if (!validCollection || !validEmployee) return new(CooperativeResourceError.NotFound);
        var link = await database.FuncionariosColetas.FindAsync([collectionId, employeeId], cancellationToken);
        if (assign && link is not null) return new(CooperativeResourceError.AlreadyAssigned);
        if (!assign && link is null) return new(CooperativeResourceError.NotAssigned);
        if (assign) database.FuncionariosColetas.Add(new() { ColetaId = collectionId, FuncionarioId = employeeId });
        else database.FuncionariosColetas.Remove(link!);
        await database.SaveChangesAsync(cancellationToken);
        return new();
    }

    private static (string Description, string Plate)? NormalizeTruck(SaveTruck command)
    {
        var description = command.Description.Trim();
        var plate = command.Plate.Trim().Replace("-", string.Empty).ToUpperInvariant();
        return description.Length is < 2 or > 45 || plate.Length != 7
            ? null
            : (description, plate);
    }

    private static bool ValidEmployee(string name, DateTime birthDate) =>
        !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 45 &&
        birthDate.Date < DateTime.Today && birthDate.Date >= DateTime.Today.AddYears(-100);
}
