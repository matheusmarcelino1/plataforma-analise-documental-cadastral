using AnaliseDocumental.ApiCadastro.Aplicacao.Abstracoes;
using AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Commands;
using AnaliseDocumental.ApiCadastro.Dominio.Entidades;
using AnaliseDocumental.Contratos.Eventos;

namespace AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Handlers;

public sealed class CadastrarDocumentoHandler
{
    private readonly IArmazenadorDocumentoService _armazenadorDocumentoService;
    private readonly IRepositorioCadastroDocumental _repositorioCadastroDocumental;
    private readonly IPublicadorEventoDocumentoService _publicadorEventoDocumentoService;
    private readonly ILogger<CadastrarDocumentoHandler> _logger;

    public CadastrarDocumentoHandler(
        IArmazenadorDocumentoService armazenadorDocumentoService,
        IRepositorioCadastroDocumental repositorioCadastroDocumental,
        IPublicadorEventoDocumentoService publicadorEventoDocumentoService,
        ILogger<CadastrarDocumentoHandler> logger)
    {
        _armazenadorDocumentoService = armazenadorDocumentoService;
        _repositorioCadastroDocumental = repositorioCadastroDocumental;
        _publicadorEventoDocumentoService = publicadorEventoDocumentoService;
        _logger = logger;
    }

    public async Task<CadastroCriadoResponse> HandleAsync(
        CadastrarDocumentoCommand command,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var cadastroId = Guid.NewGuid();
        var documentoId = Guid.NewGuid();

        var resultadoUpload = await _armazenadorDocumentoService.SalvarAsync(
            cadastroId,
            documentoId,
            command.Documento,
            cancellationToken);

        var cadastro = CadastroDocumental.Criar(
            cadastroId,
            documentoId,
            command.NomeCompleto,
            command.Cpf,
            command.Email,
            resultadoUpload.Bucket,
            resultadoUpload.ChaveS3);

        await _repositorioCadastroDocumental.SalvarAsync(
            cadastro,
            cancellationToken);

        var evento = new DocumentoCadastralRecebidoV1(
            EventoId: Guid.NewGuid(),
            CadastroId: cadastro.Id,
            DocumentoId: cadastro.DocumentoId,
            Bucket: cadastro.Bucket,
            ChaveS3: cadastro.ChaveS3,
            CorrelationId: correlationId,
            OcorreuEm: DateTimeOffset.UtcNow);

        await _publicadorEventoDocumentoService.PublicarAsync(
            evento,
            cancellationToken);

        _logger.LogInformation(
            "Cadastro documental criado com sucesso. CadastroId: {CadastroId}, DocumentoId: {DocumentoId}, CorrelationId: {CorrelationId}",
            cadastro.Id,
            cadastro.DocumentoId,
            correlationId);

        return new CadastroCriadoResponse(
            cadastro.Id,
            cadastro.DocumentoId,
            cadastro.Status);
    }
}

public sealed record CadastroCriadoResponse(
    Guid CadastroId,
    Guid DocumentoId,
    string Status);