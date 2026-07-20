using Microsoft.AspNetCore.Mvc;
using ReciclaFacil.Application.Authentication;

namespace ReciclaFacil.Api;

internal static class CredentialHttpResults
{
    internal static IResult From(CredentialResult result)
    {
        if (result.Succeeded)
            return Results.NoContent();
        if (result.Error == CredentialError.Validation)
            return Results.ValidationProblem(
                result.ValidationErrors ?? new Dictionary<string, string[]>());

        var status = result.Error switch
        {
            CredentialError.UserNotFound => StatusCodes.Status404NotFound,
            CredentialError.InvalidCurrentPassword => StatusCodes.Status401Unauthorized,
            CredentialError.InvalidOrExpiredToken => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };
        var title = result.Error switch
        {
            CredentialError.InvalidCurrentPassword => "A senha atual está incorreta.",
            CredentialError.InvalidOrExpiredToken => "O token é inválido, já foi usado ou expirou.",
            CredentialError.UserNotFound => "Usuário não encontrado.",
            _ => "Não foi possível alterar a senha."
        };
        return Results.Problem(new ProblemDetails
        {
            Status = status,
            Title = title,
            Extensions = { ["code"] = result.Error.ToString() }
        });
    }
}
