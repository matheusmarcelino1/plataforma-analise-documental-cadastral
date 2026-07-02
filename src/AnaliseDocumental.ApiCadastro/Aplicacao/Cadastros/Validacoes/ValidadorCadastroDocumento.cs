namespace AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Validacoes;

public sealed class ValidadorCadastroDocumento
{
    private const long TamanhoMaximoDocumentoBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> TiposPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/jpg",
        "image/png"
    };

    public ResultadoValidacao Validar(DadosValidacaoCadastroDocumento dados)
    {
        if (string.IsNullOrWhiteSpace(dados.NomeCompleto))
        {
            return ResultadoValidacao.Falha("O nome completo é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(dados.Cpf))
        {
            return ResultadoValidacao.Falha("O CPF é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(dados.Email))
        {
            return ResultadoValidacao.Falha("O e-mail é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(dados.NomeArquivo))
        {
            return ResultadoValidacao.Falha("O documento é obrigatório.");
        }

        if (dados.TamanhoBytes <= 0)
        {
            return ResultadoValidacao.Falha("O documento está vazio.");
        }

        if (dados.TamanhoBytes > TamanhoMaximoDocumentoBytes)
        {
            return ResultadoValidacao.Falha("O documento deve ter no máximo 10 MB.");
        }

        if (string.IsNullOrWhiteSpace(dados.TipoConteudo) ||
            !TiposPermitidos.Contains(dados.TipoConteudo))
        {
            return ResultadoValidacao.Falha("Tipo de documento não permitido. Envie PDF, JPG, JPEG ou PNG.");
        }

        return ResultadoValidacao.Sucesso();
    }
}