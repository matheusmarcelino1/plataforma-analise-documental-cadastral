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
                IFormFile? documento,
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

            var resultadoValidacao = validadorCadastroDocumento.Validar(dadosValidacao);

            if (!resultadoValidacao.Valido)
            {
                return Results.BadRequest(resultadoValidacao.MensagemErro);
            }

            var correlationId = ObterOuCriarCorrelationId(httpContext);

            await using var conteudoDocumento = documento!.OpenReadStream();

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
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<CadastroCriadoResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .DisableAntiforgery();

        return app;
    }

    private static string ObterOuCriarCorrelationId(HttpContext httpContext)
    {
        var correlationId = httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        httpContext.Response.Headers["X-Correlation-Id"] = correlationId;

        return correlationId;
    }
}