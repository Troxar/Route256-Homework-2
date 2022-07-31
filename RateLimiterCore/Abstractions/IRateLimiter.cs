namespace RateLimiterCore.Abstractions;

public interface IRateLimiter<T>
{
    Task<Result<T>> Invoke(Func<Task<T>> action, CancellationToken cancellationToken);
}