using AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Commands;

public interface IArmazenadorDocumentoService
{
    Task<ResultadoUploadDocumento> SalvarAsync(
        ArquivoDocumentoCommand documento,
        CancellationToken cancellationToken);
}

public sealed record ResultadoUploadDocumento(
    string Bucket,
    string ChaveS3);