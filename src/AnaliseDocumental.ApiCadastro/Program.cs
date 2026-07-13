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
using AnaliseDocumental.ApiCadastro.Aplicacao.Cadastros.Validacoes;
using AnaliseDocumental.ApiCadastro.Infraestrutura.Kafka;
using Confluent.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbOptions>(
    builder.Configuration.GetSection(MongoDbOptions.SectionName));

builder.Services.Configure<AwsOptions>(
    builder.Configuration.GetSection(AwsOptions.SectionName));

builder.Services.Configure<S3Options>(
    builder.Configuration.GetSection(S3Options.SectionName));

builder.Services.Configure<KafkaOptions>(
    builder.Configuration.GetSection(KafkaOptions.SectionName));

var kafkaOptions = builder.Configuration
    .GetSection(KafkaOptions.SectionName)
    .Get<KafkaOptions>()!;

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

builder.Services.AddSingleton<IProducer<string, string>>(_ =>
{
    var producerConfig = new ProducerConfig
    {
        BootstrapServers = kafkaOptions.BootstrapServers,
        ClientId = kafkaOptions.ClientId,
        Acks = Acks.All,
        EnableIdempotence = true
    };

    return new ProducerBuilder<string, string>(producerConfig)
        .Build();
});

builder.Services.AddScoped<IRepositorioCadastroDocumental, RepositorioCadastroDocumentalMongoDb>();
builder.Services.AddScoped<IArmazenadorDocumentoService, ArmazenadorDocumentoS3Service>();
builder.Services.AddScoped<IPublicadorEventoDocumentoService, PublicadorEventoDocumentoKafkaService>();
builder.Services.AddScoped<CadastrarDocumentoHandler>();

builder.Services.AddSingleton<ValidadorCadastroDocumento>();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Análise Documental - API Cadastro v1");

        options.RoutePrefix = "swagger";
        options.DocumentTitle =
            "Análise Documental - Ambiente de Testes";

        options.DisplayRequestDuration();
        options.EnableTryItOutByDefault();
    });

    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();
}

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "AnaliseDocumental.ApiCadastro",
    timestamp = DateTimeOffset.UtcNow
}))
.WithName("HealthCheck")
.WithSummary("Verifica se a API está funcionando")
.WithTags("Monitoramento");

app.MapCadastrosDocumentaisEndpoints();

app.Run();