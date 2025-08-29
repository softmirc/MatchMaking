using MatchMaking.Service.Options;
using MatchMaking.Service.Infrastructure;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Warning)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddOptions<KafkaOptions>().Bind(builder.Configuration.GetSection(KafkaOptions.SectionName)).ValidateDataAnnotations();

builder.Services.AddOptions<RateLimiterOptions>().Bind(builder.Configuration.GetSection(RateLimiterOptions.SectionName)).ValidateDataAnnotations();

builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

builder.Services.AddSingleton<IRateLimiter, InMemoryRateLimiter>();

builder.Services.AddHostedService<KafkaCompleteConsumer>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ExceptionLoggingMiddleware>();

var kafkaOptions = app.Services.GetRequiredService<IOptions<KafkaOptions>>().Value;

await KafkaTopicInitializer.EnsureKafkaTopicsExistAsync(kafkaOptions);

app.Run();