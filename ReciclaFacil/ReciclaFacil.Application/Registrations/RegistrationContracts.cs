namespace ReciclaFacil.Application.Registrations;

public sealed record RegisterClient(
    string Email,
    string Password,
    string ConfirmPassword,
    string? Cpf,
    string Type,
    string Name,
    string Address,
    string Gender,
    DateTime BirthDate,
    string? Phone,
    string Mobile,
    string CooperativeId);

public sealed record RegisterCooperative(
    string Email,
    string Password,
    string ConfirmPassword,
    string Cnpj,
    string CorporateName,
    string Address,
    string City,
    string State);

public sealed record RegistrationCooperative(
    string Id,
    string Name,
    string City,
    string State);

public sealed record RegisteredAccount(
    string Id,
    string Email,
    string Role);

public enum RegistrationError
{
    None,
    Validation,
    EmailAlreadyExists,
    DocumentAlreadyExists,
    CooperativeNotFound
}

public sealed record RegistrationResult(
    RegistrationError Error,
    RegisteredAccount? Account = null,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null)
{
    public bool Succeeded => Error == RegistrationError.None;
}

public interface IPublicRegistrationService
{
    Task<IReadOnlyList<RegistrationCooperative>> ListCooperativesAsync(
        CancellationToken cancellationToken = default);

    Task<RegistrationResult> RegisterClientAsync(
        RegisterClient command,
        CancellationToken cancellationToken = default);

    Task<RegistrationResult> RegisterCooperativeAsync(
        RegisterCooperative command,
        CancellationToken cancellationToken = default);
}
