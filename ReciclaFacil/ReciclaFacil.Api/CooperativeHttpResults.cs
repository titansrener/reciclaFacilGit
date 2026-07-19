using ReciclaFacil.Application.Cooperatives;

namespace ReciclaFacil.Api;

public static class CooperativeHttpResults
{
    public static IResult From(CooperativeOperationResult result, string? location = null)
    {
        if (result.Succeeded)
            return location is null
                ? Results.NoContent()
                : Results.Created(location, new { id = result.CollectionId });

        var (status, title) = result.Error switch
        {
            CooperativeOperationError.NotFound => (404, "Registro não encontrado."),
            CooperativeOperationError.InvalidDate => (400, "Informe uma data futura, em hora cheia, entre 8h e 18h."),
            CooperativeOperationError.InvalidPrice => (400, "O preço de revenda não pode ser negativo."),
            CooperativeOperationError.HasClients => (409, "A coleta possui clientes agendados e não pode ser excluída."),
            CooperativeOperationError.AlreadyExists => (409, "O material já está associado à cooperativa."),
            CooperativeOperationError.MaterialInUse => (409, "O material já está vinculado a uma coleta e não pode ser removido."),
            CooperativeOperationError.InvalidStatus => (409, "A operação não é permitida no estado atual da coleta."),
            _ => (400, "Não foi possível concluir a operação.")
        };
        return Results.Problem(statusCode: status, title: title);
    }
}
