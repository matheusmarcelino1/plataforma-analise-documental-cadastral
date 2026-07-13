using System.Text;
using System.Text.Json;
using AnaliseDocumental.ApiCadastro.Aplicacao.Abstracoes;
using AnaliseDocumental.ApiCadastro.Infraestrutura.Configuracoes;
using AnaliseDocumental.Contratos.Eventos;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace AnaliseDocumental.ApiCadastro.Infraestrutura.Kafka;

public sealed class PublicadorEventoDocumentoKafkaService : IPublicadorEventoDocumentoService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IProducer<string, string> _producer;
    private readonly KafkaOptions _kafkaOptions;
    private readonly ILogger<PublicadorEventoDocumentoKafkaService> _logger;

    public PublicadorEventoDocumentoKafkaService(
        IProducer<string, string> producer,
        IOptions<KafkaOptions> kafkaOptions,
        ILogger<PublicadorEventoDocumentoKafkaService> logger)
    {
        _producer = producer;
        _kafkaOptions = kafkaOptions.Value;
        _logger = logger;
    }

    public async Task PublicarAsync(
        DocumentoCadastralRecebidoV1 evento,
        CancellationToken cancellationToken)
    {
        var chave = evento.CadastroId.ToString();

        var valor = JsonSerializer.Serialize(
            evento,
            JsonSerializerOptions);

        var mensagem = new Message<string, string>
        {
            Key = chave,
            Value = valor,
            Headers = new Headers
            {
                { "correlation-id", Encoding.UTF8.GetBytes(evento.CorrelationId) },
                { "event-type", Encoding.UTF8.GetBytes(nameof(DocumentoCadastralRecebidoV1)) },
                { "event-version", Encoding.UTF8.GetBytes("1") }
            }
        };

        try
        {
            var resultado = await _producer.ProduceAsync(
                _kafkaOptions.TopicoDocumentoRecebido,
                mensagem,
                cancellationToken);

            _logger.LogInformation(
                "Evento publicado no Kafka. Topico: {Topico}, Partition: {Partition}, Offset: {Offset}, CadastroId: {CadastroId}, DocumentoId: {DocumentoId}, CorrelationId: {CorrelationId}",
                resultado.Topic,
                resultado.Partition.Value,
                resultado.Offset.Value,
                evento.CadastroId,
                evento.DocumentoId,
                evento.CorrelationId);
        }
        catch (ProduceException<string, string> exception)
        {
            _logger.LogError(
                exception,
                "Falha ao publicar evento no Kafka. Topico: {Topico}, CadastroId: {CadastroId}, DocumentoId: {DocumentoId}, Motivo: {Motivo}, CorrelationId: {CorrelationId}",
                _kafkaOptions.TopicoDocumentoRecebido,
                evento.CadastroId,
                evento.DocumentoId,
                exception.Error.Reason,
                evento.CorrelationId);

            throw new InvalidOperationException(
                "Não foi possível publicar o evento de documento cadastral no Kafka.",
                exception);
        }
    }
}