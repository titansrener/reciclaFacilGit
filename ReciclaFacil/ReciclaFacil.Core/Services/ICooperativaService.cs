using ReciclaFacil.Core.Models;

namespace ReciclaFacil.Core.Services;

public interface ICooperativaService
{
    Task<IReadOnlyList<CooperativaResumo>> PesquisarAsync(
        string? razaoSocial,
        string? cidade,
        string? estado,
        CancellationToken cancellationToken = default);

    Task<CooperativaDetalheViewModel?> ObterAsync(
        string id,
        CancellationToken cancellationToken = default);
}
