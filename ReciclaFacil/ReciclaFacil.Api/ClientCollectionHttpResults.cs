using Microsoft.AspNetCore.Mvc;
using ReciclaFacil.Application.Clients;

namespace ReciclaFacil.Api;

internal static class ClientCollectionHttpResults
{
    internal static IResult From(
        ClientCollectionResult result,
        string? createdLocation = null)
    {
        if (result.Success)
            return createdLocation is null
                ? Results.NoContent()
                : Results.Created(createdLocation, value: null);

        var status = result.Error switch
        {
            ClientCollectionError.ClientNotFound or
            ClientCollectionError.CollectionNotFound => StatusCodes.Status404NotFound,
            ClientCollectionError.NoMaterials or
            ClientCollectionError.MaterialNotAccepted => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status409Conflict
        };
        return Results.Problem(new ProblemDetails
        {
            Status = status,
            Title = result.Message,
            Type = $"https://httpstatuses.com/{status}",
            Extensions = { ["code"] = result.Error.ToString() }
        });
    }
}
