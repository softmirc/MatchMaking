namespace MatchMaking.Service.Models
{
    public class MatchMessage
    {
        public string MatchId { get; set; } = string.Empty;
        public List<string> UserIds { get; set; } = new();
    }
}
