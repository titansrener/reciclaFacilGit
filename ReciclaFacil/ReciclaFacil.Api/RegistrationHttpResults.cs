using Microsoft.AspNetCore.Mvc;
using ReciclaFacil.Application.Registrations;

namespace ReciclaFacil.Api;

public static class RegistrationHttpResults
{
    public static IResult From(RegistrationResult result, string location)
    {
        if (result.Succeeded)
            return Results.Created(location, result.Account);

        return result.Error switch
        {
            RegistrationError.Validation => Results.ValidationProblem(
                result.ValidationErrors ?? new Dictionary<string, string[]>()),
            RegistrationError.EmailAlreadyExists => Results.Conflict(new ProblemDetails
            {
                Title = "E-mail já cadastrado",
                Detail = "Já existe uma conta vinculada a este e-mail.",
                Status = StatusCodes.Status409Conflict
            }),
            RegistrationError.DocumentAlreadyExists => Results.Conflict(new ProblemDetails
            {
                Title = "Documento já cadastrado",
                Detail = "Já existe uma conta vinculada a este CPF ou CNPJ.",
                Status = StatusCodes.Status409Conflict
            }),
            RegistrationError.CooperativeNotFound => Results.Problem(
                title: "Cooperativa inválida",
                detail: "A cooperativa selecionada não existe.",
                statusCode: StatusCodes.Status404NotFound),
            _ => Results.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
