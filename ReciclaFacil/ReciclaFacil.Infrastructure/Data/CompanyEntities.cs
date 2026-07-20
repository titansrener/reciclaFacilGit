using NetTopologySuite.Geometries;

namespace ReciclaFacil.Infrastructure.Data;

public sealed class Empresa
{
    public required string Id { get; set; }
    public required string Cnpj { get; set; }
    public required string RazaoSocial { get; set; }
    public required string Endereco { get; set; }
    public Point? EnderecoCoordenada { get; set; }
    public required string Telefone { get; set; }
    public string? Fax { get; set; }
    public required string Email { get; set; }
}
