using ReciclaFacil.Core.Models;

namespace ReciclaFacil.Core.Services;

public interface ICooperativaService
{
    IReadOnlyList<CooperativaResumo> Pesquisar(
        string? razaoSocial,
        string? cidade,
        string? estado);
}
