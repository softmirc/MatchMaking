using System.ComponentModel.DataAnnotations;

namespace MatchMaking.Service.Options
{
    public class KafkaOptions
    {
        public const string SectionName = "Kafka";

        [Required]
        public string BootstrapServers { get; set; }

        [Required]
        public string RequestTopic { get; set; }

        [Required]
        public string CompleteTopic { get; set; }

        [Required]
        public string GroupId { get; set; }
    }
}

