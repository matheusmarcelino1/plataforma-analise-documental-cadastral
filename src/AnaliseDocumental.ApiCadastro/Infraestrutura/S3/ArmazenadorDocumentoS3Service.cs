using Amazon.S3;
using Amazon.S3.Model;
using AnaliseDocumental.ApiCadastro.Aplicacao.Abstracoes;
using AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Commands;
using AnaliseDocumental.ApiCadastro.Infraestrutura.Configuracoes;
using Microsoft.Extensions.Options;

namespace AnaliseDocumental.ApiCadastro.Infraestrutura.S3;

public sealed class ArmazenadorDocumentoS3Service : IArmazenadorDocumentoService
{
    private readonly IAmazonS3 _amazonS3;
    private readonly S3Options _s3Options;
    private readonly ILogger<ArmazenadorDocumentoS3Service> _logger;

    public ArmazenadorDocumentoS3Service(
        IAmazonS3 amazonS3,
        IOptions<S3Options> s3Options,
        ILogger<ArmazenadorDocumentoS3Service> logger)
    {
        _amazonS3 = amazonS3;
        _s3Options = s3Options.Value;
        _logger = logger;
    }

    public async Task<ResultadoUploadDocumento> SalvarAsync(
        Guid cadastroId,
        Guid documentoId,
        ArquivoDocumentoCommand documento,
        CancellationToken cancellationToken)
    {
        var chaveS3 = CriarChaveS3(
            cadastroId,
            documentoId,
            documento.NomeArquivo);

        var request = new PutObjectRequest
        {
            BucketName = _s3Options.Bucket,
            Key = chaveS3,
            InputStream = documento.Conteudo,
            ContentType = documento.TipoConteudo
        };

        request.Metadata.Add("cadastro-id", cadastroId.ToString());
        request.Metadata.Add("documento-id", documentoId.ToString());
        request.Metadata.Add("nome-original", documento.NomeArquivo);

        await _amazonS3.PutObjectAsync(
            request,
            cancellationToken);

        _logger.LogInformation(
            "Documento salvo no S3. Bucket: {Bucket}, ChaveS3: {ChaveS3}",
            _s3Options.Bucket,
            chaveS3);

        return new ResultadoUploadDocumento(
            _s3Options.Bucket,
            chaveS3);
    }

    private static string CriarChaveS3(
        Guid cadastroId,
        Guid documentoId,
        string nomeArquivo)
    {
        var dataAtual = DateTimeOffset.UtcNow;

        var nomeArquivoSeguro = Path.GetFileName(nomeArquivo)
            .Replace(" ", "-")
            .ToLowerInvariant();

        return $"documentos-cadastrais/{dataAtual:yyyy}/{dataAtual:MM}/{cadastroId}/{documentoId}-{nomeArquivoSeguro}";
    }
}