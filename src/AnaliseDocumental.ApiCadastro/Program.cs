using Amazon.Runtime;
using Amazon.S3;
using AnaliseDocumental.ApiCadastro.Aplicacao.Abstracoes;
using AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Handlers;
using AnaliseDocumental.ApiCadastro.Apresentacao.Endpoints;
using AnaliseDocumental.ApiCadastro.Infraestrutura.Configuracoes;
using AnaliseDocumental.ApiCadastro.Infraestrutura.Eventos;
using AnaliseDocumental.ApiCadastro.Infraestrutura.MongoDb;
using AnaliseDocumental.ApiCadastro.Infraestrutura.S3;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbOptions>(
    builder.Configuration.GetSection(MongoDbOptions.SectionName));

builder.Services.Configure<AwsOptions>(
    builder.Configuration.GetSection(AwsOptions.SectionName));

builder.Services.Configure<S3Options>(
    builder.Configuration.GetSection(S3Options.SectionName));

var mongoDbOptions = builder.Configuration
    .GetSection(MongoDbOptions.SectionName)
    .Get<MongoDbOptions>()!;

builder.Services.AddSingleton<IMongoClient>(
    _ => new MongoClient(mongoDbOptions.ConnectionString));

var awsOptions = builder.Configuration
    .GetSection(AwsOptions.SectionName)
    .Get<AwsOptions>()!;

builder.Services.AddSingleton<IAmazonS3>(_ =>
{
    var credenciais = new BasicAWSCredentials(
        awsOptions.AccessKey,
        awsOptions.SecretKey);

    var configuracaoS3 = new AmazonS3Config
    {
        ServiceURL = awsOptions.ServiceUrl,
        AuthenticationRegion = awsOptions.Region,
        ForcePathStyle = true
    };

    return new AmazonS3Client(
        credenciais,
        configuracaoS3);
});

builder.Services.AddScoped<IRepositorioCadastroDocumental, RepositorioCadastroDocumentalMongoDb>();
builder.Services.AddScoped<IArmazenadorDocumentoService, ArmazenadorDocumentoS3Service>();
builder.Services.AddScoped<IPublicadorEventoDocumentoService, PublicadorEventoDocumentoLogService>();

builder.Services.AddScoped<CadastrarDocumentoHandler>();

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "AnaliseDocumental.ApiCadastro",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapCadastrosDocumentaisEndpoints();

app.Run();