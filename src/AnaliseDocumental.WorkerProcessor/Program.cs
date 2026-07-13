using AnaliseDocumental.WorkerProcessor;
using AnaliseDocumental.WorkerProcessor.Infraestrutura.Kafka;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaOptions>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();