using AnaliseDocumental.WorkerProcessor.Infraestrutura.Kafka;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace AnaliseDocumental.WorkerProcessor;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly KafkaOptions _kafkaOptions;

    public Worker(
        ILogger<Worker> logger,
        IOptions<KafkaOptions> kafkaOptions)
    {
        _logger = logger;
        _kafkaOptions = kafkaOptions.Value;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => ConsumirMensagens(stoppingToken), stoppingToken);
    }

    private void ConsumirMensagens(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
    "Kafka Config -> Servers: {Servers}, Topic: {Topic}, Group: {Group}",
    _kafkaOptions.BootstrapServers,
    _kafkaOptions.Topic,
    _kafkaOptions.GroupId);

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _kafkaOptions.BootstrapServers,
            GroupId = _kafkaOptions.GroupId,
            ClientId = _kafkaOptions.ClientId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig)
            .SetErrorHandler((_, erro) =>
            {
                _logger.LogError("Erro Kafka: {Reason}", erro.Reason);
            })
            .Build();

        consumer.Subscribe(_kafkaOptions.Topic);

        _logger.LogInformation(
            "Worker Kafka iniciado. Topic: {Topic}. GroupId: {GroupId}. BootstrapServers: {BootstrapServers}",
            _kafkaOptions.Topic,
            _kafkaOptions.GroupId,
            _kafkaOptions.BootstrapServers);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var resultado = consumer.Consume(stoppingToken);

                    _logger.LogInformation(
                        "Mensagem Kafka recebida. Topic: {Topic}. Partition: {Partition}. Offset: {Offset}. Key: {Key}. Value: {Value}",
                        resultado.Topic,
                        resultado.Partition.Value,
                        resultado.Offset.Value,
                        resultado.Message.Key,
                        resultado.Message.Value);

                    consumer.Commit(resultado);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Erro ao consumir mensagem Kafka.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Worker Kafka cancelado.");
        }
        finally
        {
            consumer.Close();
            _logger.LogInformation("Consumer Kafka fechado.");
        }
    }
}