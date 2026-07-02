using AnaliseDocumental.Contratos.Eventos;

namespace AnaliseDocumental.ApiCadastro.Aplicacao.Abstracoes;

public interface IPublicadorEventoDocumentoService
{
    Task PublicarAsync(
        DocumentoCadastralRecebidoV1 evento,
        CancellationToken cancellationToken);
}