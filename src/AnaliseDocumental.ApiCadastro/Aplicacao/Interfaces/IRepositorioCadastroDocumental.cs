using AnaliseDocumental.ApiCadastro.Dominio.Entidades;

public interface IRepositorioCadastroDocumental
{
    Task SalvarAsync(
        CadastroDocumental cadastro,
        CancellationToken cancellationToken);

    Task<CadastroDocumental?> BuscarPorIdAsync(
        Guid cadastroId,
        CancellationToken cancellationToken);
}