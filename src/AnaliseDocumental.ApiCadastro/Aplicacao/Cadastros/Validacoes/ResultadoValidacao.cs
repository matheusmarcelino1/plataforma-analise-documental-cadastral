namespace AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Validacoes;

public sealed record ResultadoValidacao(
    bool Valido,
    string? MensagemErro)
{
    public static ResultadoValidacao Sucesso()
    {
        return new ResultadoValidacao(
            Valido: true,
            MensagemErro: null);
    }

    public static ResultadoValidacao Falha(string mensagemErro)
    {
        return new ResultadoValidacao(
            Valido: false,
            MensagemErro: mensagemErro);
    }
}