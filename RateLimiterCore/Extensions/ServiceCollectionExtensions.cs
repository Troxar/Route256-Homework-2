using Microsoft.Extensions.Configuration;
using RateLimiterCore;
using RateLimiterCore.Configurations;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRateLimiter<TResultValue>(this IServiceCollection services, 
        IConfiguration? configuration = null,
        Action<RateLimiterConfiguration>? rateLimiterConfiguration = null)
    {
        RateLimiterConfiguration cfg = new RateLimiterConfiguration();
        if(configuration is not null)
            cfg = configuration.GetSection(nameof(RateLimiterConfiguration)).Get<RateLimiterConfiguration>();
        if(rateLimiterConfiguration is not null)
            rateLimiterConfiguration.Invoke(cfg);

        services.Configure<RateLimiterConfiguration>(limiterConfiguration =>
        {
            limiterConfiguration.Threshold = cfg.Threshold;
            limiterConfiguration.LimitInterval = cfg.LimitInterval;
            limiterConfiguration.WaitInterval = cfg.WaitInterval;
        });

        services.AddSingleton<IRateLimiter<TResultValue>, RateLimiter<TResultValue>>();
        
        return services;
    }
}