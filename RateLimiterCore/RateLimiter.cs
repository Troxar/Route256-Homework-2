using RateLimiterCore.Abstractions;

namespace RateLimiterCore;

public class RateLimiter<T> : IRateLimiter<T>, IDisposable
{
    private readonly IRateLimiterConfig _config;
    private readonly SemaphoreSlim _semaphore;
    private readonly object _sync = new object();
    public DateTime _nextWindow;
    
    public RateLimiter(IRateLimiterConfig config)
    {
        _config = config;
        _semaphore = new SemaphoreSlim(_config.Count, _config.Count);
        SetNextWindow();
    }
    
    private void SetNextWindow()
    {
        _nextWindow = _config.SystemDate.Now.AddSeconds(_config.Duration);
    }
    
    public async Task<Result<T>> Invoke(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        if (_config.SystemDate.Now >= _nextWindow)
            PrepareNextWindow();

        if (!_semaphore.Wait(100, cancellationToken)) 
            return Result<T>.Fail();
        
        var result = await action.Invoke();
        return Result<T>.Success(result);
    }
    
    private void PrepareNextWindow()
    {
        lock (_sync)
        {
            var now = _config.SystemDate.Now;
            if (_nextWindow > now)
            {
                return;
            }
            
            _nextWindow = now.AddSeconds(_config.Duration);
            
            int releaseCount = _config.Count - _semaphore.CurrentCount;
            if (releaseCount > 0)
            {
                _semaphore.Release(releaseCount);    
            }
        }   
    }
    
    public void Dispose()
    {
        _semaphore.Dispose();
    }
}