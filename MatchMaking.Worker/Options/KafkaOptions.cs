using System.ComponentModel.DataAnnotations;

namespace MatchMaking.Worker.Options
{
    public class KafkaOptions
    {
        public const string SectionName = "Kafka";

        [Required]
        public required string BootstrapServers { get; set; }

        [Required]
        public required string RequestTopic { get; set; }

        [Required]
        public required string CompleteTopic { get; set; }

        [Required]
        public required string GroupId { get; set; }

        [Required]
        public int MatchSize { get; set; }
    }
}
