namespace RateLimiterCore.Abstractions;

public class SystemDate : ISystemDate
{
    public DateTime Now => DateTime.Now;
}