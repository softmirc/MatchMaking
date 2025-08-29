using Confluent.Kafka;
using Confluent.Kafka.Admin;
using MatchMaking.Service.Options;

namespace MatchMaking.Service.Infrastructure
{
    public static class KafkaTopicInitializer
    {
        public static async Task EnsureKafkaTopicsExistAsync(KafkaOptions kafkaOptions)
        {
            using var adminClient = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = kafkaOptions.BootstrapServers
            }).Build();

            var topics = new List<TopicSpecification>
            {
                new TopicSpecification { Name = kafkaOptions.RequestTopic, NumPartitions = 1, ReplicationFactor = 1 },
                new TopicSpecification { Name = kafkaOptions.CompleteTopic, NumPartitions = 1, ReplicationFactor = 1 }
            };

            try
            {
                await adminClient.CreateTopicsAsync(topics);
            }
            catch (CreateTopicsException e)
            {
                foreach (var result in e.Results)
                {
                    if (result.Error.Code != ErrorCode.TopicAlreadyExists)
                        throw;
                }
            }
        }
    }
}