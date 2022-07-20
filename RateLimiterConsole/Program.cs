// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using RateLimiterCore;
using RateLimiterCore.Configurations;
using RateLimiterCore.Models;

public class Programm
{
    public static async Task Main(string[] args)
    {
        var serviceCollection = new ServiceCollection()
            .AddLogging(opt =>
            {
                opt.AddConsole();
            });
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<RateLimiter<int>>>();
        var random = new Random();

        var limiter = new RateLimiter<int>(new RateLimiterConfiguration(),
            logger);

        var tasksList = new List<Task<Result<int>>>();

        for (int i = 0; i < 50; i++)
        {
            var task = Task.Run(async () =>
            {
                return await limiter.Invoke(async () =>
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        
                        return random.Next();
                    }, 
                    CancellationToken.None);
            });
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            
            tasksList.Add(task);
        }

        await Task.WhenAll(tasksList);
    }
}