using ReciclaFacil.Application.Materials;

namespace ReciclaFacil.Api;

public static class MaterialManagementHttpResults
{
    public static IResult From(MaterialManagementResult result, string? location = null)
    {
        if (result.Succeeded)
            return location is null
                ? Results.NoContent()
                : Results.Created(location, new { id = result.Id });
        var (status, title) = result.Error switch
        {
            MaterialManagementError.NotFound => (404, "Material não encontrado."),
            MaterialManagementError.InvalidInput => (400, "Revise a descrição e o tempo de decomposição."),
            MaterialManagementError.DescriptionExists => (409, "Já existe um material com essa descrição."),
            MaterialManagementError.InUse => (409, "O material está em uso e não pode ser excluído."),
            _ => (400, "Não foi possível concluir a operação.")
        };
        return Results.Problem(statusCode: status, title: title);
    }
}
