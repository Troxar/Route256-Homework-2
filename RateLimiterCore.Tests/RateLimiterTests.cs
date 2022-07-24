using RateLimiterCore.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RateLimiterCore.Tests;

public class RateLimiterTests
{
    private class SystemDateStub : ISystemDate
    {
        public DateTime Now { get; }
        public SystemDateStub(DateTime now) => Now = now;
    }
    
    [Theory]
    [ClassData(typeof(DataForTheoryWhenAllTasksComesInOneWindow))]
    public async void Invoke_WhenAllTasksComesInOneWindow_ShouldExecuteOnlyAllowedCount(DateTime now, int count, int duration, int tasksCount, int expected)
    {
        // Arrange
        var timeStub = new SystemDateStub(now);
        var config = new RateLimiterConfig(count, duration, timeStub);
        var cut = new RateLimiter<int>(config);
        var tasksList = new List<Task<Result<int>>>();

        for (int i = 0; i < tasksCount; i++)
        {
            var innerTask = Task.FromResult(1);
            var func = new Func<Task<int>>(() => innerTask);
            var task = Task.Run(async () => await cut.Invoke(func, CancellationToken.None));
            tasksList.Add(task);
        }
        
        // Act
        await Task.WhenAll(tasksList);
        
        // Assert
        Assert.Equal(expected, tasksList.Count(x => x.Result.Value == 1));
    }
    
    private class DataForTheoryWhenAllTasksComesInOneWindow : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new DateTime(2000, 1, 1, 1 ,1 ,1),
                5,
                1, 
                12,
                5
            };
            yield return new object[]
            {
                new DateTime(2000, 1, 1, 1 ,1 ,1),
                5,
                1, 
                3,
                3
            };
        }
    
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}