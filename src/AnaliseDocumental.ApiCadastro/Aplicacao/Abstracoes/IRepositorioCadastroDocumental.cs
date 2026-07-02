using AnaliseDocumental.ApiCadastro.Dominio.Entidades;

namespace AnaliseDocumental.ApiCadastro.Aplicacao.Abstracoes;

public interface IRepositorioCadastroDocumental
{
    Task SalvarAsync(
        CadastroDocumental cadastro,
        CancellationToken cancellationToken);

    Task<CadastroDocumental?> BuscarPorIdAsync(
        Guid cadastroId,
        CancellationToken cancellationToken);
}