using MatchMaking.Worker;
using MatchMaking.Worker.Infrastructure;
using MatchMaking.Worker.Options;
using Serilog;
using Serilog.Events;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Warning)
    .CreateLogger();

builder.Services.AddSerilog(Log.Logger);

builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<IMatchBuffer, InMemoryMatchBuffer>();

builder.Services.AddOptions<KafkaOptions>().Bind(builder.Configuration.GetSection(KafkaOptions.SectionName)).ValidateDataAnnotations();

var host = builder.Build();

host.Run();
