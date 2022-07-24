namespace RateLimiterCore.Abstractions;

public interface IRateLimiterConfig
{
    public int Count { get; init; }
    public int Duration { get; init; }
    public ISystemDate SystemDate { get; init; }
}