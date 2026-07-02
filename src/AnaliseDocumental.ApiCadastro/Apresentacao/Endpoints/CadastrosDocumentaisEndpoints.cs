using AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Commands;
using AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace AnaliseDocumental.ApiCadastro.Apresentacao.Endpoints;

public static class CadastrosDocumentaisEndpoints
{
    private const long TamanhoMaximoDocumentoBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> TiposPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/jpg",
        "image/png"
    };

    public static IEndpointRouteBuilder MapCadastrosDocumentaisEndpoints(
        this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/cadastros-documentais")
            .WithTags("Cadastros Documentais");

        grupo.MapPost("/", async (
                [FromForm] string nomeCompleto,
                [FromForm] string cpf,
                [FromForm] string email,
                IFormFile documento,
                HttpContext httpContext,
                CadastrarDocumentoHandler handler,
                CancellationToken cancellationToken) =>
        {
            var erroValidacao = ValidarRequisicao(
                nomeCompleto,
                cpf,
                email,
                documento);

            if (erroValidacao is not null)
            {
                return erroValidacao;
            }

            var correlationId = ObterOuCriarCorrelationId(httpContext);

            await using var conteudoDocumento = documento.OpenReadStream();

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

    private static IResult? ValidarRequisicao(
        string nomeCompleto,
        string cpf,
        string email,
        IFormFile documento)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
        {
            return Results.BadRequest("O nome completo é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(cpf))
        {
            return Results.BadRequest("O CPF é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest("O e-mail é obrigatório.");
        }

        if (documento is null)
        {
            return Results.BadRequest("O documento é obrigatório.");
        }

        if (documento.Length <= 0)
        {
            return Results.BadRequest("O documento está vazio.");
        }

        if (documento.Length > TamanhoMaximoDocumentoBytes)
        {
            return Results.BadRequest("O documento deve ter no máximo 10 MB.");
        }

        if (!TiposPermitidos.Contains(documento.ContentType))
        {
            return Results.BadRequest("Tipo de documento não permitido. Envie PDF, JPG, JPEG ou PNG.");
        }

        return null;
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