namespace AnaliseDocumental.ApiCadastro.Infraestrutura.Configuracoes;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string TopicoDocumentoRecebido { get; init; } = string.Empty;
}