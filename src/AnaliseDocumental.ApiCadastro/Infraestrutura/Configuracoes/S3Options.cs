namespace AnaliseDocumental.ApiCadastro.Infraestrutura.Configuracoes;

public sealed class S3Options
{
    public const string SectionName = "S3";

    public string Bucket { get; init; } = string.Empty;
}