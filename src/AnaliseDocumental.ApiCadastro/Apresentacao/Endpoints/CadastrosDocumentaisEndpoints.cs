using AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Commands;
using AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Handlers;
using AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Validacoes;
using Microsoft.AspNetCore.Mvc;

namespace AnaliseDocumental.ApiCadastro.Apresentacao.Endpoints;

public static class CadastrosDocumentaisEndpoints
{
    public static IEndpointRouteBuilder MapCadastrosDocumentaisEndpoints(
        this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/cadastros-documentais")
            .WithTags("Cadastros Documentais");

        grupo.MapPost("/", async (
                [FromForm] string nomeCompleto,
                [FromForm] string cpf,
                [FromForm] string email,
                [FromForm] IFormFile? documento,
                [FromHeader(Name = "X-Correlation-Id")] string? correlationId,
                HttpContext httpContext,
                CadastrarDocumentoHandler handler,
                ValidadorCadastroDocumento validadorCadastroDocumento,
                CancellationToken cancellationToken) =>
        {
            var dadosValidacao = new DadosValidacaoCadastroDocumento(
                NomeCompleto: nomeCompleto,
                Cpf: cpf,
                Email: email,
                NomeArquivo: documento?.FileName,
                TipoConteudo: documento?.ContentType,
                TamanhoBytes: documento?.Length ?? 0);

            var resultadoValidacao =
                validadorCadastroDocumento.Validar(dadosValidacao);

            if (!resultadoValidacao.Valido)
            {
                return Results.BadRequest(resultadoValidacao.MensagemErro);
            }

            correlationId = ObterOuCriarCorrelationId(
                httpContext,
                correlationId);

            await using var conteudoDocumento =
                documento!.OpenReadStream();

            var command = new CadastrarDocumentoCommand(
                NomeCompleto: nomeCompleto,
                Cpf: cpf,
                Email: email,
                Documento: new ArquivoDocumentoCommand(
                    NomeArquivo: documento.FileName,
                    TipoConteudo: documento.ContentType,
                    TamanhoBytes: documento.Length,
                    Conteudo: conteudoDocumento));

            var response = await handler.HandleAsync(
                command,
                correlationId,
                cancellationToken);

            return Results.Created(
                $"/api/cadastros-documentais/{response.CadastroId}",
                response);
        })
        .WithName("CadastrarDocumento")
        .WithSummary("Cadastra uma pessoa e envia um documento")
        .WithDescription(
            """
            Recebe dados cadastrais e um documento em PDF, JPG ou PNG.

            Fluxo realizado:

            1. Valida os dados recebidos;
            2. Armazena o documento no S3;
            3. Salva o cadastro no MongoDB;
            4. Publica um evento no Kafka;
            5. O Worker consome o evento de forma assíncrona.

            O header X-Correlation-Id é opcional. Caso não seja informado,
            a API gera automaticamente um identificador de rastreamento.
            """)
        .Produces<CadastroCriadoResponse>(
            StatusCodes.Status201Created,
            "application/json")
        .Produces<string>(
            StatusCodes.Status400BadRequest,
            "application/json")
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .DisableAntiforgery();

        return app;
    }

    private static string ObterOuCriarCorrelationId(
        HttpContext httpContext,
        string? correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        httpContext.Response.Headers["X-Correlation-Id"] =
            correlationId;

        return correlationId;
    }
}