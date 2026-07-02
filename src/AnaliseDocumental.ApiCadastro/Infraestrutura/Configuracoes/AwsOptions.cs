namespace AnaliseDocumental.ApiCadastro.Infraestrutura.Configuracoes;

public sealed class AwsOptions
{
    public const string SectionName = "Aws";

    public string Region { get; init; } = "math-math";
    public string ServiceUrl { get; init; } = string.Empty;
    public string AccessKey { get; init; } = "math";
    public string SecretKey { get; init; } = "math";
}