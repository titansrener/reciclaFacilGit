namespace ReciclaFacil.Infrastructure.Data;

public sealed class Caminhao
{
    public int Id { get; set; }
    public required string Descricao { get; set; }
    public required string Placa { get; set; }
    public required string CooperativaId { get; set; }
    public Cooperativa? Cooperativa { get; set; }
    public ICollection<CaminhaoColeta> Coletas { get; set; } = [];
}

public sealed class Funcionario
{
    public required string Id { get; set; }
    public required string Nome { get; set; }
    public DateTime DataNascimento { get; set; }
    public required string CooperativaId { get; set; }
    public Cooperativa? Cooperativa { get; set; }
    public LegacyUser? Usuario { get; set; }
    public ICollection<FuncionarioColeta> Coletas { get; set; } = [];
}

public sealed class CaminhaoColeta
{
    public int CaminhaoId { get; set; }
    public int ColetaId { get; set; }
    public Caminhao? Caminhao { get; set; }
    public Coleta? Coleta { get; set; }
}

public sealed class FuncionarioColeta
{
    public int ColetaId { get; set; }
    public required string FuncionarioId { get; set; }
    public Coleta? Coleta { get; set; }
    public Funcionario? Funcionario { get; set; }
}
