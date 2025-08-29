using MatchMaking.Service.Infrastructure;
using MatchMaking.Service.Models;
using MatchMaking.Service.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;

[ApiController]
[Route("match")]
public class MatchController : ControllerBase
{
    private readonly IRateLimiter _rateLimiter;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly IDistributedCache _cache;
    private readonly ILogger<MatchController> _logger;
    private KafkaOptions _kafkaOptions;

    public MatchController(IRateLimiter rateLimiter, IKafkaProducer kafkaProducer, IDistributedCache cache, ILogger<MatchController> logger, IOptions<KafkaOptions> kafkaOptions)
    {
        _rateLimiter = rateLimiter;
        _kafkaProducer = kafkaProducer;
        _cache = cache;
        _logger = logger;
        _kafkaOptions = kafkaOptions.Value;
    }

    [HttpPost("search")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchMatch([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("userId is required.");
        }

        var allowed = await _rateLimiter.WaitAsync(userId);

        if (!allowed)
        {
            _logger.LogWarning("Rate limit exceeded for user: {userId}", userId);

            return BadRequest("Too many requests. Please wait.");
        }

        await _kafkaProducer.ProduceAsync(_kafkaOptions.RequestTopic, userId);

        _logger.LogInformation("Match search request sent for user: {userId}", userId);

        return NoContent();
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(MatchInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMatchInfo([FromRoute] string userId)
    {
        var matchInfoValue = await _cache.GetStringAsync($"match:{userId}");

        if (matchInfoValue == null)
        {
            _logger.LogWarning("GetMatchInfo not found by UserId: {userId}", userId);

            return NotFound();
        }

        var matchInfo = JsonSerializer.Deserialize<MatchInfo>(matchInfoValue);

        return Ok(matchInfo);
    }
}
