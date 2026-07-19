using NetTopologySuite.Geometries;

namespace ReciclaFacil.Core.Data;

public sealed class Cliente
{
    public required string Id { get; set; }
    public string? Cpf { get; set; }
    public required string Tipo { get; set; }
    public required string Nome { get; set; }
    public required string Endereco { get; set; }
    public Point? EnderecoCoordenada { get; set; }
    public required string Email { get; set; }
    public required string Sexo { get; set; }
    public DateTime DataNascimento { get; set; }
    public string? Telefone { get; set; }
    public required string Celular { get; set; }
    public required string CooperativaId { get; set; }
    public Cooperativa? Cooperativa { get; set; }
    public ICollection<ClienteColeta> Coletas { get; set; } = [];
    public ICollection<CarteiraMovimento> Carteira { get; set; } = [];
    public ICollection<Notificacao> Notificacoes { get; set; } = [];
}

public sealed class Coleta
{
    public int Id { get; set; }
    public DateTime? HoraAgendada { get; set; }
    public double? Quantidade { get; set; }
    public required string Status { get; set; }
    public required string CooperativaId { get; set; }
    public Cooperativa? Cooperativa { get; set; }
    public ICollection<ClienteColeta> Clientes { get; set; } = [];
}

public sealed class ClienteColeta
{
    public int ColetaId { get; set; }
    public required string ClienteId { get; set; }
    public DateTime? HoraDaColeta { get; set; }
    public string? Status { get; set; }
    public Cliente? Cliente { get; set; }
    public Coleta? Coleta { get; set; }
    public ICollection<MaterialColetado> Materiais { get; set; } = [];
}

public sealed class CarteiraMovimento
{
    public int Id { get; set; }
    public decimal Saldo { get; set; }
    public DateTime? DataUltimaMovimentacao { get; set; }
    public required string ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
}

public sealed class MaterialColetado
{
    public int MaterialId { get; set; }
    public int ColetaId { get; set; }
    public required string ClienteId { get; set; }
    public double? Quantidade { get; set; }
    public decimal? ValorCompra { get; set; }
    public string? Status { get; set; }
    public Material? Material { get; set; }
    public ClienteColeta? ClienteColeta { get; set; }
}

public sealed class Notificacao
{
    public int Id { get; set; }
    public required string ClienteId { get; set; }
    public int ColetaId { get; set; }
    public required string CooperativaId { get; set; }
    public DateTime? DataHorario { get; set; }
    public bool? Ativa { get; set; }
    public required string Descricao { get; set; }
    public required string Tipo { get; set; }
    public Cliente? Cliente { get; set; }
}
