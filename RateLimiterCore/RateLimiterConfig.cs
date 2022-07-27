using RateLimiterCore.Abstractions;

namespace RateLimiterCore;

public class RateLimiterConfig : IRateLimiterConfig
{
    public int Count { get; init; }
    public int Duration { get; init; }
    
    public RateLimiterConfig(int count, int duration)
    {
        Count = count;
        Duration = duration;
    }
}