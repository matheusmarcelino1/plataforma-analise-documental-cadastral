using AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Validacoes;

namespace AnaliseDocumental.ApiCadastro.UnitTests.Aplicacao.Cadastros.Validacoes;

public sealed class ValidadorCadastroDocumentoTests
{
    [Fact]
    public void Validar_DeveRetornarSucesso_QuandoDadosForemValidos()
    {
        // Arrange
        var validador = new ValidadorCadastroDocumento();

        var dados = new DadosValidacaoCadastroDocumento(
            NomeCompleto: "Maria Silva",
            Cpf: "12345678909",
            Email: "maria.silva@email.com",
            NomeArquivo: "documento.pdf",
            TipoConteudo: "application/pdf",
            TamanhoBytes: 1024);

        // Act
        var resultado = validador.Validar(dados);

        // Assert
        Assert.True(resultado.Valido);
        Assert.Null(resultado.MensagemErro);
    }

    [Fact]
    public void Validar_DeveRetornarFalha_QuandoNomeCompletoNaoForInformado()
    {
        // Arrange
        var validador = new ValidadorCadastroDocumento();

        var dados = new DadosValidacaoCadastroDocumento(
            NomeCompleto: "",
            Cpf: "12345678909",
            Email: "maria.silva@email.com",
            NomeArquivo: "documento.pdf",
            TipoConteudo: "application/pdf",
            TamanhoBytes: 1024);

        // Act
        var resultado = validador.Validar(dados);

        // Assert
        Assert.False(resultado.Valido);
        Assert.Equal("O nome completo é obrigatório.", resultado.MensagemErro);
    }

    [Fact]
    public void Validar_DeveRetornarFalha_QuandoDocumentoEstiverVazio()
    {
        // Arrange
        var validador = new ValidadorCadastroDocumento();

        var dados = new DadosValidacaoCadastroDocumento(
            NomeCompleto: "Maria Silva",
            Cpf: "12345678909",
            Email: "maria.silva@email.com",
            NomeArquivo: "documento.pdf",
            TipoConteudo: "application/pdf",
            TamanhoBytes: 0);

        // Act
        var resultado = validador.Validar(dados);

        // Assert
        Assert.False(resultado.Valido);
        Assert.Equal("O documento está vazio.", resultado.MensagemErro);
    }

    [Fact]
    public void Validar_DeveRetornarFalha_QuandoDocumentoForMaiorQueLimitePermitido()
    {
        // Arrange
        var validador = new ValidadorCadastroDocumento();

        var tamanhoMaiorQueDezMb = (10 * 1024 * 1024) + 1;

        var dados = new DadosValidacaoCadastroDocumento(
            NomeCompleto: "Maria Silva",
            Cpf: "12345678909",
            Email: "maria.silva@email.com",
            NomeArquivo: "documento.pdf",
            TipoConteudo: "application/pdf",
            TamanhoBytes: tamanhoMaiorQueDezMb);

        // Act
        var resultado = validador.Validar(dados);

        // Assert
        Assert.False(resultado.Valido);
        Assert.Equal("O documento deve ter no máximo 10 MB.", resultado.MensagemErro);
    }

    [Fact]
    public void Validar_DeveRetornarFalha_QuandoTipoDocumentoNaoForPermitido()
    {
        // Arrange
        var validador = new ValidadorCadastroDocumento();

        var dados = new DadosValidacaoCadastroDocumento(
            NomeCompleto: "Maria Silva",
            Cpf: "12345678909",
            Email: "maria.silva@email.com",
            NomeArquivo: "documento.txt",
            TipoConteudo: "text/plain",
            TamanhoBytes: 1024);

        // Act
        var resultado = validador.Validar(dados);

        // Assert
        Assert.False(resultado.Valido);
        Assert.Equal("Tipo de documento não permitido. Envie PDF, JPG, JPEG ou PNG.", resultado.MensagemErro);
    }
}