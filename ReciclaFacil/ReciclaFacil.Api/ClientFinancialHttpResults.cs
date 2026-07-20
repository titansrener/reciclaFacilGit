using ReciclaFacil.Application.Clients;

namespace ReciclaFacil.Api;

public static class ClientFinancialHttpResults
{
    public static IResult From(ClientFinancialResult result)
    {
        if (result.Succeeded) return Results.Ok(new { value = result.Value });
        return result.Error switch
        {
            ClientFinancialError.NotFound =>
                Results.Problem(statusCode: 404, title: "Oferta ou notificação não encontrada."),
            ClientFinancialError.NoPendingOffer =>
                Results.Problem(statusCode: 409, title: "Não existe uma oferta pendente para esta coleta."),
            _ => Results.Problem(statusCode: 400, title: "Não foi possível concluir a operação.")
        };
    }
}
