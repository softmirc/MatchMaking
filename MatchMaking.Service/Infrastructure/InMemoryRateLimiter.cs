using MatchMaking.Service.Options;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace MatchMaking.Service.Infrastructure
{
    public interface IRateLimiter
    {
        Task<bool> WaitAsync(string userId);
    }

    public class InMemoryRateLimiter(IOptions<RateLimiterOptions> _rateLimiterOptions) : IRateLimiter
    {
        private readonly TimeSpan _minInterval = TimeSpan.FromMilliseconds(_rateLimiterOptions.Value.MinInterval);
        private readonly ConcurrentDictionary<string, DateTime> _lastRequestTime = new();

        public Task<bool> WaitAsync(string userId)
        {
            var now = DateTime.UtcNow;

            if (_lastRequestTime.TryGetValue(userId, out var last))
            {
                if (now - last < _minInterval)
                {
                    return Task.FromResult(false);
                }
            }

            _lastRequestTime[userId] = now;

            return Task.FromResult(true);
        }
    }
}
