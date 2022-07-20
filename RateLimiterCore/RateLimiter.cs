using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RateLimiterCore.Configurations.Abstractions;
using RateLimiterCore.Models;

namespace RateLimiterCore;

public class RateLimiter<T> : IRateLimiter<T>, IDisposable
{
    private readonly IRateLimiterConfiguration _rateLimiterConfiguration;
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly CancellationTokenSource _cts;
    private readonly ILogger<RateLimiter<T>> _logger;

    public RateLimiter(IRateLimiterConfiguration rateLimiterOptions,
        ILogger<RateLimiter<T>>? logger = null)
    {
        _logger = logger ?? NullLogger<RateLimiter<T>>.Instance;
        _rateLimiterConfiguration = rateLimiterOptions;
        _cts = new CancellationTokenSource();
        _semaphoreSlim = new SemaphoreSlim(_rateLimiterConfiguration.Threshold);
        Task.Run(() => ReleaseJob(_cts.Token), _cts.Token);
    }

    private async Task ReleaseJob(CancellationToken token)
    {
        while (true)
        {
            if (token.IsCancellationRequested)
            {
                _logger.LogInformation("Jobs cancelled");
                break;
            }
            
            await Task.Delay(_rateLimiterConfiguration.LimitInterval, token);
            _logger.LogInformation("Release all jobs job");
            _semaphoreSlim.Release(_rateLimiterConfiguration.Threshold);
        }
    }
    
    public async Task<Result<T>> Invoke(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        if (!await _semaphoreSlim.WaitAsync(_rateLimiterConfiguration.WaitInterval, cancellationToken))
        {
            _logger.LogError("Fail add new job");
            return Result<T>.Fail();
        }
        
        _logger.LogInformation("Success add new job");
        return Result<T>.Success(await action.Invoke());
    }

    public void Dispose() => _cts.Cancel();
}