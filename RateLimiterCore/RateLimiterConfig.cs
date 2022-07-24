using RateLimiterCore.Abstractions;

namespace RateLimiterCore;

public class RateLimiterConfig : IRateLimiterConfig
{
    public int Count { get; init; }
    public int Duration { get; init; }
    public ISystemDate SystemDate { get; init; }

    public RateLimiterConfig(int count, int duration, ISystemDate systemDate)
    {
        Count = count;
        Duration = duration;
        SystemDate = systemDate;
    }
}