using AnaliseDocumental.ApiCadastro.Aplicacao.Abstracoes;
using AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Commands;
using AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Handlers;
using AnaliseDocumental.ApiCadastro.Dominio.Entidades;
using AnaliseDocumental.Contratos.Eventos;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;

namespace AnaliseDocumental.ApiCadastro.UnitTests.Aplicacao.Cadastros.Handlers;

public sealed class CadastrarDocumentoHandlerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task HandleAsync_DeveCadastrarDocumento_QuandoCommandForValido()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var correlationId = Guid.NewGuid().ToString("N");

        var bucket = "analise-documental-dev";
        var chaveS3 = "documentos-cadastrais/2026/07/cadastro/documento.pdf";

        var conteudoDocumento = new MemoryStream(new byte[] { 1, 2, 3, 4 });

        var command = new CadastrarDocumentoCommand(
            NomeCompleto: "Maria Silva",
            Cpf: "12345678909",
            Email: "maria.silva@email.com",
            Documento: new ArquivoDocumentoCommand(
                NomeArquivo: "documento.pdf",
                TipoConteudo: "application/pdf",
                TamanhoBytes: conteudoDocumento.Length,
                Conteudo: conteudoDocumento));

        var armazenadorDocumentoServiceMock = new Mock<IArmazenadorDocumentoService>();
        var repositorioCadastroDocumentalMock = new Mock<IRepositorioCadastroDocumental>();
        var publicadorEventoDocumentoServiceMock = new Mock<IPublicadorEventoDocumentoService>();
        var loggerMock = new Mock<ILogger<CadastrarDocumentoHandler>>();

        Guid cadastroIdGerado = Guid.Empty;
        Guid documentoIdGerado = Guid.Empty;
        CadastroDocumental? cadastroSalvo = null;
        DocumentoCadastralRecebidoV1? eventoPublicado = null;

        armazenadorDocumentoServiceMock
            .Setup(service => service.SalvarAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<ArquivoDocumentoCommand>(),
                cancellationToken))
            .Callback<Guid, Guid, ArquivoDocumentoCommand, CancellationToken>(
                (cadastroId, documentoId, _, _) =>
                {
                    cadastroIdGerado = cadastroId;
                    documentoIdGerado = documentoId;
                })
            .ReturnsAsync(new ResultadoUploadDocumento(
                Bucket: bucket,
                ChaveS3: chaveS3));

        repositorioCadastroDocumentalMock
            .Setup(repositorio => repositorio.SalvarAsync(
                It.IsAny<CadastroDocumental>(),
                cancellationToken))
            .Callback<CadastroDocumental, CancellationToken>(
                (cadastro, _) =>
                {
                    cadastroSalvo = cadastro;
                })
            .Returns(Task.CompletedTask);

        publicadorEventoDocumentoServiceMock
            .Setup(publicador => publicador.PublicarAsync(
                It.IsAny<DocumentoCadastralRecebidoV1>(),
                cancellationToken))
            .Callback<DocumentoCadastralRecebidoV1, CancellationToken>(
                (evento, _) =>
                {
                    eventoPublicado = evento;
                })
            .Returns(Task.CompletedTask);

        var handler = new CadastrarDocumentoHandler(
            armazenadorDocumentoServiceMock.Object,
            repositorioCadastroDocumentalMock.Object,
            publicadorEventoDocumentoServiceMock.Object,
            loggerMock.Object);

        // Act
        var response = await handler.HandleAsync(
            command,
            correlationId,
            cancellationToken);

        // Assert
        Assert.NotEqual(Guid.Empty, cadastroIdGerado);
        Assert.NotEqual(Guid.Empty, documentoIdGerado);

        Assert.NotNull(cadastroSalvo);
        Assert.Equal(cadastroIdGerado, cadastroSalvo.Id);
        Assert.Equal(documentoIdGerado, cadastroSalvo.DocumentoId);
        Assert.Equal(command.NomeCompleto, cadastroSalvo.NomeCompleto);
        Assert.Equal(command.Cpf, cadastroSalvo.Cpf);
        Assert.Equal(command.Email, cadastroSalvo.Email);
        Assert.Equal(bucket, cadastroSalvo.Bucket);
        Assert.Equal(chaveS3, cadastroSalvo.ChaveS3);
        Assert.Equal("Recebido", cadastroSalvo.Status);

        Assert.NotNull(eventoPublicado);
        Assert.Equal(cadastroSalvo.Id, eventoPublicado.CadastroId);
        Assert.Equal(cadastroSalvo.DocumentoId, eventoPublicado.DocumentoId);
        Assert.Equal(bucket, eventoPublicado.Bucket);
        Assert.Equal(chaveS3, eventoPublicado.ChaveS3);
        Assert.Equal(correlationId, eventoPublicado.CorrelationId);

        Assert.Equal(cadastroSalvo.Id, response.CadastroId);
        Assert.Equal(cadastroSalvo.DocumentoId, response.DocumentoId);
        Assert.Equal("Recebido", response.Status);

        armazenadorDocumentoServiceMock.Verify(
            service => service.SalvarAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<ArquivoDocumentoCommand>(),
                cancellationToken),
            Times.Once);

        repositorioCadastroDocumentalMock.Verify(
            repositorio => repositorio.SalvarAsync(
                It.IsAny<CadastroDocumental>(),
                cancellationToken),
            Times.Once);

        publicadorEventoDocumentoServiceMock.Verify(
            publicador => publicador.PublicarAsync(
                It.IsAny<DocumentoCadastralRecebidoV1>(),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NaoDeveSalvarCadastroNemPublicarEvento_QuandoUploadFalhar()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var correlationId = _fixture.Create<Guid>().ToString("N");

        var conteudoDocumento = new MemoryStream(new byte[] { 1, 2, 3, 4 });

        var command = new CadastrarDocumentoCommand(
            NomeCompleto: "Maria Silva",
            Cpf: "12345678909",
            Email: "maria.silva@email.com",
            Documento: new ArquivoDocumentoCommand(
                NomeArquivo: "documento.pdf",
                TipoConteudo: "application/pdf",
                TamanhoBytes: conteudoDocumento.Length,
                Conteudo: conteudoDocumento));

        var armazenadorDocumentoServiceMock = new Mock<IArmazenadorDocumentoService>();
        var repositorioCadastroDocumentalMock = new Mock<IRepositorioCadastroDocumental>();
        var publicadorEventoDocumentoServiceMock = new Mock<IPublicadorEventoDocumentoService>();
        var loggerMock = new Mock<ILogger<CadastrarDocumentoHandler>>();

        armazenadorDocumentoServiceMock
            .Setup(service => service.SalvarAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<ArquivoDocumentoCommand>(),
                cancellationToken))
            .ThrowsAsync(new InvalidOperationException("Falha ao salvar documento no S3."));

        var handler = new CadastrarDocumentoHandler(
            armazenadorDocumentoServiceMock.Object,
            repositorioCadastroDocumentalMock.Object,
            publicadorEventoDocumentoServiceMock.Object,
            loggerMock.Object);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(
                command,
                correlationId,
                cancellationToken));

        // Assert
        Assert.Equal("Falha ao salvar documento no S3.", exception.Message);

        repositorioCadastroDocumentalMock.Verify(
            repositorio => repositorio.SalvarAsync(
                It.IsAny<CadastroDocumental>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        publicadorEventoDocumentoServiceMock.Verify(
            publicador => publicador.PublicarAsync(
                It.IsAny<DocumentoCadastralRecebidoV1>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}