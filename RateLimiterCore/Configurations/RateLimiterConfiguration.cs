using RateLimiterCore.Configurations.Abstractions;

namespace RateLimiterCore.Configurations;

public class RateLimiterConfiguration : IRateLimiterConfiguration
{
    public int Threshold { get; set; } = 5;
    public TimeSpan LimitInterval { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan WaitInterval { get; set; } = TimeSpan.FromMilliseconds(5);
}