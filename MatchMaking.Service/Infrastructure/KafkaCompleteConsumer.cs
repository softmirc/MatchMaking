namespace MatchMaking.Service.Infrastructure
{
    using System.Text.Json;
    using Confluent.Kafka;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Caching.Distributed;
    using MatchMaking.Service.Models;
    using MatchMaking.Service.Options;
    using Microsoft.Extensions.Logging;

    public class KafkaCompleteConsumer : BackgroundService
    {
        private readonly KafkaOptions _kafkaOptions;
        private readonly ILogger<KafkaCompleteConsumer> _logger;
        private readonly IDistributedCache _redis;
        private IConsumer<Ignore, string>? _consumer;

        public KafkaCompleteConsumer(IOptions<KafkaOptions> kafkaOptions, ILogger<KafkaCompleteConsumer> logger, IDistributedCache redis)
        {
            _kafkaOptions = kafkaOptions.Value;
            _logger = logger;
            _redis = redis;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _kafkaOptions.BootstrapServers,
                GroupId = _kafkaOptions.GroupId + "-complete-consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();

            _consumer.Subscribe(_kafkaOptions.CompleteTopic);

            return Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
        }

        private async Task ConsumeLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = _consumer!.Consume(cancellationToken);

                        if (result == null || string.IsNullOrWhiteSpace(result.Value))
                            continue;

                        var match = JsonSerializer.Deserialize<MatchMessage>(result.Value);

                        if (match is null || match.UserIds == null)
                        {
                            _logger.LogWarning("match is null or UserIds == null");
                            continue;
                        }

                        if (match.UserIds.Count == 0)
                            throw new InvalidDataException("UserIds is null or empty");

                        if (string.IsNullOrEmpty(match.MatchId))
                            throw new InvalidDataException("MatchId is null or empty");

                        foreach (var userId in match.UserIds)
                        {
                            var value = JsonSerializer.Serialize(new MatchInfo(match.MatchId, match.UserIds));

                            await _redis.SetStringAsync($"match:{userId}", value, new DistributedCacheEntryOptions
                            {
                                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) // Timeout store
                            }, cancellationToken);
                        }

                        _logger.LogInformation("Match stored: {MatchId} for users: {Users}", match.MatchId, string.Join(", ", match.UserIds));
                    }
                    catch (ConsumeException e)
                    {
                        _logger.LogError(e, "Kafka consume error");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error in KafkaCompleteConsumer");
                    }
                }
            }
            finally
            {
                _consumer?.Close();
            }
        }
    }
}