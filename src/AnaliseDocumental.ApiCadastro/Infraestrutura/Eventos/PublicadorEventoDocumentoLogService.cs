using AnaliseDocumental.ApiCadastro.Aplicacao.Abstracoes;
using AnaliseDocumental.Contratos.Eventos;

namespace AnaliseDocumental.ApiCadastro.Infraestrutura.Eventos;

public sealed class PublicadorEventoDocumentoLogService : IPublicadorEventoDocumentoService
{
    private readonly ILogger<PublicadorEventoDocumentoLogService> _logger;

    public PublicadorEventoDocumentoLogService(
        ILogger<PublicadorEventoDocumentoLogService> logger)
    {
        _logger = logger;
    }

    public Task PublicarAsync(
        DocumentoCadastralRecebidoV1 evento,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Evento pronto para publicação futura no Kafka. EventoId: {EventoId}, CadastroId: {CadastroId}, DocumentoId: {DocumentoId}, ChaveS3: {ChaveS3}, CorrelationId: {CorrelationId}",
            evento.EventoId,
            evento.CadastroId,
            evento.DocumentoId,
            evento.ChaveS3,
            evento.CorrelationId);

        return Task.CompletedTask;
    }
}