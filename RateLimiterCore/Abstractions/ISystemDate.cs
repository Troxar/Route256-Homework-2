namespace RateLimiterCore.Abstractions;

public interface ISystemDate
{
    DateTime Now { get; }
}