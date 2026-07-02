namespace AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Validacoes;

public sealed record DadosValidacaoCadastroDocumento(
    string? NomeCompleto,
    string? Cpf,
    string? Email,
    string? NomeArquivo,
    string? TipoConteudo,
    long TamanhoBytes);