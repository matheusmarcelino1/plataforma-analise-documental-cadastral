namespace AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Commands;

public sealed record CadastrarDocumentoCommand(
    string NomeCompleto,
    string Cpf,
    string Email,
    ArquivoDocumentoCommand Documento
);

public sealed record ArquivoDocumentoCommand(
    string NomeArquivo,
    string TipoConteudo,
    long TamanhoBytes,
    Stream Conteudo
);