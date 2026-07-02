using AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Commands;

namespace AnaliseDocumental.ApiCadastro.Aplicacao.Abstracoes;

public interface IArmazenadorDocumentoService
{
    Task<ResultadoUploadDocumento> SalvarAsync(
        Guid cadastroId,
        Guid documentoId,
        ArquivoDocumentoCommand documento,
        CancellationToken cancellationToken);
}

public sealed record ResultadoUploadDocumento(
    string Bucket,
    string ChaveS3);