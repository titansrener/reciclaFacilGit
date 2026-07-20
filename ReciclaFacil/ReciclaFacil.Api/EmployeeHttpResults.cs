using ReciclaFacil.Application.Employees;

namespace ReciclaFacil.Api;

public static class EmployeeHttpResults
{
    public static IResult From(EmployeeOperationResult result)
    {
        if (result.Succeeded) return Results.Ok(new { totalValue = result.TotalValue });
        var (status, title) = result.Error switch
        {
            EmployeeOperationError.NotFound => (404, "Coleta ou cliente não encontrado."),
            EmployeeOperationError.CollectionNotStarted => (409, "A coleta precisa estar em andamento."),
            EmployeeOperationError.ClientAlreadyProcessed => (409, "O cliente já foi processado nesta coleta."),
            EmployeeOperationError.MissingPrice => (409, "A cooperativa precisa informar o preço de todos os materiais."),
            EmployeeOperationError.InvalidMaterials => (400, "Informe uma quantidade válida para cada material."),
            _ => (400, "Não foi possível registrar a coleta.")
        };
        return Results.Problem(statusCode: status, title: title);
    }
}
