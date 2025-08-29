namespace MatchMaking.Service.Infrastructure
{
    using Confluent.Kafka;
    using MatchMaking.Service.Options;
    using Microsoft.Extensions.Options;

    public interface IKafkaProducer
    {
        Task ProduceAsync(string topic, string value);
    }

    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<Null, string> _producer;

        public KafkaProducer(IOptions<KafkaOptions> options)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = options.Value.BootstrapServers
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task ProduceAsync(string topic, string value)
        {
            await _producer.ProduceAsync(topic, new Message<Null, string>
            {
                Value = value
            });
        }
    }
}
