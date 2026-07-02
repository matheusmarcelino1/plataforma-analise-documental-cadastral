using AnaliseDocumental.Contratos.Eventos;

public interface IPublicadorEventoDocumentoService
{
    Task PublicarAsync(
        DocumentoCadastralRecebidoV1 evento,
        CancellationToken cancellationToken);
}