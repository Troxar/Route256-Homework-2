// See https://aka.ms/new-console-template for more information

using RateLimiterCore;
using RateLimiterCore.Abstractions;

public class Programm
{
    private const int REQUESTSCOUNT = 5;
    private const int WINDOWDURATION = 1;
    private const int TASKSCOUNT = 20;
    public static async Task Main(string[] args)
    {
        var random = new Random();

        var config = new RateLimiterConfig(REQUESTSCOUNT, WINDOWDURATION, new SystemDate());
        var limiter = new RateLimiter<int>(config);

        var tasksList = new List<Task<Result<int>>>();

        for (int i = 0; i < TASKSCOUNT; i++)
        {
            var task = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                
                return await limiter.Invoke(async () =>
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        
                        return random.Next();
                    }, 
                    CancellationToken.None);
            });
            
            tasksList.Add(task);
        }

        await Task.WhenAll(tasksList);
    }
}