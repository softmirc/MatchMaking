using System.ComponentModel.DataAnnotations;

namespace MatchMaking.Service.Options
{
    public class RateLimiterOptions
    {
        public const string SectionName = "RateLimiter";

        [Required]
        public required int MinInterval { get; set; }
    }
}

