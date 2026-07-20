using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReciclaFacil.Application.Registrations;
using ReciclaFacil.Infrastructure.Data;

namespace ReciclaFacil.Infrastructure.Services;

public sealed class PublicRegistrationService(
    ReciclaFacilDbContext database,
    IPasswordHasher<LegacyUser> passwordHasher) : IPublicRegistrationService
{
    public async Task<IReadOnlyList<RegistrationCooperative>> ListCooperativesAsync(
        CancellationToken cancellationToken = default) =>
        await database.Cooperativas.AsNoTracking()
            .OrderBy(x => x.RazaoSocial)
            .Select(x => new RegistrationCooperative(x.Id, x.RazaoSocial, x.Cidade, x.Estado))
            .ToListAsync(cancellationToken);

    public async Task<RegistrationResult> RegisterClientAsync(
        RegisterClient command,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateClient(command);
        if (errors.Count > 0)
            return Invalid(errors);

        var email = Clean(command.Email).ToLowerInvariant();
        var cpf = NullIfWhiteSpace(command.Cpf);
        if (await EmailExistsAsync(email, cancellationToken))
            return new(RegistrationError.EmailAlreadyExists);
        if (cpf is not null && await database.Clientes.AsNoTracking()
            .AnyAsync(x => x.Cpf == cpf, cancellationToken))
            return new(RegistrationError.DocumentAlreadyExists);
        if (!await database.Cooperativas.AsNoTracking()
            .AnyAsync(x => x.Id == Clean(command.CooperativeId), cancellationToken))
            return new(RegistrationError.CooperativeNotFound);

        var roleId = await GetRoleIdAsync("Cliente", cancellationToken);
        var id = Guid.NewGuid().ToString();
        var user = CreateUser(id, email);
        user.PasswordHash = passwordHasher.HashPassword(user, command.Password ?? "");

        await using var transaction = await database.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        if (await EmailExistsAsync(email, cancellationToken))
            return new(RegistrationError.EmailAlreadyExists);
        if (cpf is not null && await database.Clientes
            .AnyAsync(x => x.Cpf == cpf, cancellationToken))
            return new(RegistrationError.DocumentAlreadyExists);

        database.Users.Add(user);
        database.UserRoles.Add(new LegacyUserRole { UserId = id, RoleId = roleId });
        await database.SaveChangesAsync(cancellationToken);
        database.Clientes.Add(new Cliente
        {
            Id = id,
            Cpf = cpf,
            Tipo = Clean(command.Type).ToUpperInvariant(),
            Nome = Clean(command.Name),
            Endereco = Clean(command.Address),
            Email = email,
            Sexo = Clean(command.Gender).ToUpperInvariant(),
            DataNascimento = command.BirthDate.Date,
            Telefone = NullIfWhiteSpace(command.Phone),
            Celular = Clean(command.Mobile),
            CooperativaId = Clean(command.CooperativeId)
        });
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(RegistrationError.None, new(id, email, "Cliente"));
    }

    public async Task<RegistrationResult> RegisterCooperativeAsync(
        RegisterCooperative command,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateCooperative(command);
        if (errors.Count > 0)
            return Invalid(errors);

        var email = Clean(command.Email).ToLowerInvariant();
        var cnpj = Clean(command.Cnpj);
        if (await EmailExistsAsync(email, cancellationToken))
            return new(RegistrationError.EmailAlreadyExists);
        if (await CnpjExistsAsync(cnpj, cancellationToken))
            return new(RegistrationError.DocumentAlreadyExists);

        var roleId = await GetRoleIdAsync("Cooperativa", cancellationToken);
        var id = Guid.NewGuid().ToString();
        var user = CreateUser(id, email);
        user.PasswordHash = passwordHasher.HashPassword(user, command.Password ?? "");

        await using var transaction = await database.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        if (await EmailExistsAsync(email, cancellationToken))
            return new(RegistrationError.EmailAlreadyExists);
        if (await CnpjExistsAsync(cnpj, cancellationToken))
            return new(RegistrationError.DocumentAlreadyExists);

        database.Users.Add(user);
        database.UserRoles.Add(new LegacyUserRole { UserId = id, RoleId = roleId });
        await database.SaveChangesAsync(cancellationToken);
        database.Cooperativas.Add(new Cooperativa
        {
            Id = id,
            Cnpj = cnpj,
            RazaoSocial = Clean(command.CorporateName),
            Endereco = Clean(command.Address),
            Cidade = Clean(command.City),
            Estado = Clean(command.State).ToUpperInvariant()
        });
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(RegistrationError.None, new(id, email, "Cooperativa"));
    }

    public async Task<RegistrationResult> RegisterCompanyAsync(
        RegisterCompany command,
        CancellationToken cancellationToken = default)
    {
        var address = BuildCompanyAddress(command);
        var errors = ValidateCompany(command, address);
        if (errors.Count > 0)
            return Invalid(errors);

        var email = Clean(command.Email).ToLowerInvariant();
        var cnpj = Clean(command.Cnpj);
        if (await EmailExistsAsync(email, cancellationToken))
            return new(RegistrationError.EmailAlreadyExists);
        if (await CnpjExistsAsync(cnpj, cancellationToken))
            return new(RegistrationError.DocumentAlreadyExists);

        var roleId = await GetRoleIdAsync("Empresa", cancellationToken);
        var id = Guid.NewGuid().ToString();
        var user = CreateUser(id, email);
        user.PasswordHash = passwordHasher.HashPassword(user, command.Password ?? "");

        await using var transaction = await database.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        if (await EmailExistsAsync(email, cancellationToken))
            return new(RegistrationError.EmailAlreadyExists);
        if (await CnpjExistsAsync(cnpj, cancellationToken))
            return new(RegistrationError.DocumentAlreadyExists);

        database.Users.Add(user);
        database.UserRoles.Add(new LegacyUserRole { UserId = id, RoleId = roleId });
        await database.SaveChangesAsync(cancellationToken);
        database.Empresas.Add(new Empresa
        {
            Id = id,
            Cnpj = cnpj,
            RazaoSocial = Clean(command.CorporateName),
            Endereco = address,
            Telefone = Clean(command.Phone),
            Fax = NullIfWhiteSpace(command.Fax),
            Email = email
        });
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(RegistrationError.None, new(id, email, "Empresa"));
    }

    private Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
        database.Users.AnyAsync(
            x => x.UserName == email || x.Email == email, cancellationToken);

    private async Task<bool> CnpjExistsAsync(string cnpj, CancellationToken cancellationToken) =>
        await database.Cooperativas.AnyAsync(x => x.Cnpj == cnpj, cancellationToken) ||
        await database.Empresas.AnyAsync(x => x.Cnpj == cnpj, cancellationToken);

    private async Task<string> GetRoleIdAsync(string name, CancellationToken cancellationToken) =>
        await database.Roles.Where(x => x.Name == name)
            .Select(x => x.Id)
            .SingleAsync(cancellationToken);

    private static LegacyUser CreateUser(string id, string email) => new()
    {
        Id = id,
        Email = email,
        UserName = email,
        EmailConfirmed = false,
        SecurityStamp = Guid.NewGuid().ToString(),
        DataCadastro = DateTime.UtcNow,
        Ativo = true,
        Discriminator = "ApplicationUser"
    };

    private static Dictionary<string, string[]> ValidateClient(RegisterClient command)
    {
        var errors = ValidateAccount(command.Email, command.Password, command.ConfirmPassword, 45);
        AddRequired(errors, "name", command.Name, 75, "Informe o nome.");
        AddRequired(errors, "address", command.Address, 100, "Informe o endereço.");
        AddDigits(errors, "cpf", command.Cpf, 11, false, "O CPF deve conter 11 números.");
        AddDigits(errors, "phone", command.Phone, null, false, "O telefone deve conter 10 ou 11 números.", 10, 11);
        AddDigits(errors, "mobile", command.Mobile, null, true, "O celular deve conter 10 ou 11 números.", 10, 11);
        if (command.Type is not ("V" or "F"))
            errors["type"] = ["Selecione um tipo de cliente válido."];
        if (command.Gender is not ("F" or "M" or "O"))
            errors["gender"] = ["Selecione uma opção de gênero válida."];
        if (command.BirthDate.Date >= DateTime.UtcNow.Date || command.BirthDate.Year < 1900)
            errors["birthDate"] = ["Informe uma data de nascimento válida."];
        if (string.IsNullOrWhiteSpace(command.CooperativeId))
            errors["cooperativeId"] = ["Selecione uma cooperativa."];
        return errors;
    }

    private static Dictionary<string, string[]> ValidateCooperative(RegisterCooperative command)
    {
        var errors = ValidateAccount(command.Email, command.Password, command.ConfirmPassword, 256);
        AddDigits(errors, "cnpj", command.Cnpj, 14, true, "O CNPJ deve conter 14 números.");
        AddRequired(errors, "corporateName", command.CorporateName, 100, "Informe a razão social.");
        AddRequired(errors, "address", command.Address, 150, "Informe o endereço.");
        AddRequired(errors, "city", command.City, 80, "Informe a cidade.");
        var state = Clean(command.State);
        if (state.Length != 2 || !state.All(char.IsLetter))
            errors["state"] = ["Informe a sigla do estado com duas letras."];
        return errors;
    }

    private static Dictionary<string, string[]> ValidateCompany(
        RegisterCompany command,
        string address)
    {
        var errors = ValidateAccount(
            command.Email, command.Password, command.ConfirmPassword, 45, 8);
        AddDigits(errors, "cnpj", command.Cnpj, 14, true, "O CNPJ deve conter 14 números.");
        AddRequired(errors, "corporateName", command.CorporateName, 150, "Informe a razão social.");
        AddRequired(errors, "street", command.Street, 100, "Informe o endereço.");
        AddRequired(errors, "city", command.City, 80, "Informe a cidade.");
        if (command.Number <= 0)
            errors["number"] = ["Informe um número válido."];
        var state = Clean(command.State);
        if (state.Length != 2 || !state.All(char.IsLetter))
            errors["state"] = ["Informe a sigla do estado com duas letras."];
        AddDigits(errors, "phone", command.Phone, null, true,
            "O telefone deve conter 10 ou 11 números.", 10, 11);
        AddDigits(errors, "fax", command.Fax, null, false,
            "O fax deve conter até 25 números.", 1, 25);
        if (address.Length > 150)
            errors["street"] = ["O endereço completo deve conter no máximo 150 caracteres."];
        return errors;
    }

    private static Dictionary<string, string[]> ValidateAccount(
        string email,
        string password,
        string confirmation,
        int emailMaxLength,
        int passwordMinLength = 6)
    {
        var errors = new Dictionary<string, string[]>();
        var normalizedEmail = Clean(email);
        if (normalizedEmail.Length == 0 || normalizedEmail.Length > emailMaxLength ||
            !new EmailAddressAttribute().IsValid(normalizedEmail))
            errors["email"] = ["Informe um e-mail válido."];
        if ((password?.Length ?? 0) < passwordMinLength || (password?.Length ?? 0) > 100)
            errors["password"] = [$"A senha deve ter entre {passwordMinLength} e 100 caracteres."];
        if (!string.Equals(password, confirmation, StringComparison.Ordinal))
            errors["confirmPassword"] = ["As senhas não conferem."];
        return errors;
    }

    private static void AddRequired(
        IDictionary<string, string[]> errors, string field, string value, int maxLength, string message)
    {
        var trimmed = Clean(value);
        if (trimmed.Length == 0 || trimmed.Length > maxLength)
            errors[field] = [message];
    }

    private static void AddDigits(
        IDictionary<string, string[]> errors,
        string field,
        string? value,
        int? exactLength,
        bool required,
        string message,
        int? minLength = null,
        int? maxLength = null)
    {
        var trimmed = value?.Trim() ?? "";
        if (!required && trimmed.Length == 0)
            return;
        if (trimmed.Length == 0 || !trimmed.All(char.IsDigit) ||
            (exactLength.HasValue && trimmed.Length != exactLength) ||
            (minLength.HasValue && trimmed.Length < minLength) ||
            (maxLength.HasValue && trimmed.Length > maxLength))
            errors[field] = [message];
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Clean(string? value) => value?.Trim() ?? "";

    private static string BuildCompanyAddress(RegisterCompany command) =>
        $"{Clean(command.Street)}, {command.Number} - {Clean(command.City)} - " +
        Clean(command.State).ToUpperInvariant();

    private static RegistrationResult Invalid(IReadOnlyDictionary<string, string[]> errors) =>
        new(RegistrationError.Validation, ValidationErrors: errors);
}
