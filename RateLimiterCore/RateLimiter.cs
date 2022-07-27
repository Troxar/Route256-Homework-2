using RateLimiterCore.Abstractions;

namespace RateLimiterCore;

public class RateLimiter<T> : IRateLimiter<T>, IDisposable
{
    private readonly IRateLimiterConfig _config;
    private readonly SemaphoreSlim _semaphore;
    private readonly object _sync = new object();
    private DateTime _nextWindow;
    private readonly ISystemDate _systemDate;
    
    public RateLimiter(IRateLimiterConfig config, ISystemDate systemDate)
    {
        _config = config;
        _semaphore = new SemaphoreSlim(_config.Count, _config.Count);
        _systemDate = systemDate;
        SetNextWindow();
    }
    
    private void SetNextWindow()
    {
        _nextWindow = _systemDate.Now.AddSeconds(_config.Duration);
    }
    
    public async Task<Result<T>> Invoke(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        if (_systemDate.Now >= _nextWindow)
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
            var now = _systemDate.Now;
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