using ReciclaFacil.Application.Cooperatives;

namespace ReciclaFacil.Api;

public static class CooperativeResourceHttpResults
{
    public static IResult From(CooperativeResourceResult result, string? location = null)
    {
        if (result.Succeeded)
            return location is null
                ? Results.NoContent()
                : Results.Created(location, new { id = result.EmployeeId ?? result.TruckId?.ToString() });
        var (status, title) = result.Error switch
        {
            CooperativeResourceError.NotFound => (404, "Registro não encontrado."),
            CooperativeResourceError.InvalidInput => (400, "Revise os dados informados."),
            CooperativeResourceError.PlateExists => (409, "A placa já está cadastrada."),
            CooperativeResourceError.EmailExists => (409, "O e-mail já está cadastrado."),
            CooperativeResourceError.AlreadyAssigned => (409, "O recurso já está associado à coleta."),
            CooperativeResourceError.NotAssigned => (409, "O recurso não está associado à coleta."),
            _ => (400, "Não foi possível concluir a operação.")
        };
        return Results.Problem(statusCode: status, title: title);
    }
}
