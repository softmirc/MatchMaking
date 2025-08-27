using Confluent.Kafka;
using MatchMaking.Worker.Infrastructure;
using MatchMaking.Worker.Models;
using MatchMaking.Worker.Options;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace MatchMaking.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IProducer<Null, string> _producer;
        private readonly IMatchBuffer _buffer;
        private readonly int _matchSize;
        private KafkaOptions _kafkaOptions;

        public Worker(IOptions<KafkaOptions> opts, IMatchBuffer buffer, ILogger<Worker> logger)
        {
            var consumerConfig = new ConsumerConfig { BootstrapServers = opts.Value.BootstrapServers, GroupId = opts.Value.GroupId };

            _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();

            _consumer.Subscribe(opts.Value.RequestTopic);

            _producer = new ProducerBuilder<Null, string>(new ProducerConfig { BootstrapServers = opts.Value.BootstrapServers }).Build();

            _buffer = buffer;
            _logger = logger;
            _matchSize = opts.Value.MatchSize;
            _kafkaOptions = opts.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(stoppingToken);

                _buffer.Add(result.Message.Value);

                if (_buffer.Count >= _matchSize)
                {
                    var users = _buffer.DequeueBatch(_matchSize);

                    var matchId = Guid.NewGuid().ToString();

                    var matchMsg = JsonSerializer.Serialize(new MatchMessage { MatchId = matchId, UserIds = users });

                    await _producer.ProduceAsync(_kafkaOptions.CompleteTopic, new Message<Null, string> { Value = matchMsg });

                    _logger.LogInformation("Match created {matchId} for users {users}", matchId, string.Join(',', users));
                }
            }
        }
    }
}
