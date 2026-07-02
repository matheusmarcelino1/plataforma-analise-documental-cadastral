using AnaliseDocumental.ApiCadastro.Aplicacao.Abstracoes;
using AnaliseDocumental.ApiCadastro.Dominio.Entidades;
using AnaliseDocumental.ApiCadastro.Infraestrutura.Configuracoes;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AnaliseDocumental.ApiCadastro.Infraestrutura.MongoDb;

public sealed class RepositorioCadastroDocumentalMongoDb : IRepositorioCadastroDocumental
{
    private const string NomeColecao = "cadastros_documentais";

    private readonly IMongoCollection<CadastroDocumental> _colecao;

    public RepositorioCadastroDocumentalMongoDb(
        IMongoClient mongoClient,
        IOptions<MongoDbOptions> mongoDbOptions)
    {
        var database = mongoClient.GetDatabase(mongoDbOptions.Value.DatabaseName);
        _colecao = database.GetCollection<CadastroDocumental>(NomeColecao);
    }

    public async Task SalvarAsync(
        CadastroDocumental cadastro,
        CancellationToken cancellationToken)
    {
        await _colecao.InsertOneAsync(
            cadastro,
            cancellationToken: cancellationToken);
    }

    public async Task<CadastroDocumental?> BuscarPorIdAsync(
        Guid cadastroId,
        CancellationToken cancellationToken)
    {
        return await _colecao
            .Find(cadastro => cadastro.Id == cadastroId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}