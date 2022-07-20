namespace RateLimiterCore.Configurations.Abstractions;

public interface IRateLimiterConfiguration
{
    int Threshold { get; }
    TimeSpan LimitInterval { get; }
    TimeSpan WaitInterval { get; }
}