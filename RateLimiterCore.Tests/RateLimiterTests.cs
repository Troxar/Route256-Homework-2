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
        public DateTime Now { get; set; }
        public SystemDateStub(DateTime now) => Now = now;
    }
    
    [Theory]
    [ClassData(typeof(DataForTheoryWhenAllTasksComeInOneSecond))]
    public async void Invoke_WhenAllTasksComeInOneSecond_ShouldExecuteOnlyAllowedCount(DateTime startDate, 
        int configCount, int configDuration, int tasksCount, int expected)
    {
        // Arrange
        var timeStub = new SystemDateStub(startDate);
        var config = new RateLimiterConfig(configCount, configDuration);
        var cut = new RateLimiter<int>(config, timeStub);
        var tasksList = new List<Task<Result<int>>>();

        for (int i = 0; i < tasksCount; i++)
        {
            var innerTask = Task.FromResult(1);
            var func = new Func<Task<int>>(() => innerTask);
            
            // Act
            var task = Task.Run(async () => await cut.Invoke(func, CancellationToken.None));
            
            tasksList.Add(task);
        }
        
        await Task.WhenAll(tasksList);
        
        // Assert
        Assert.Equal(expected, tasksList.Count(x => x.Result.Value == 1));
    }
    
    private class DataForTheoryWhenAllTasksComeInOneSecond : IEnumerable<object[]>
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
    
    [Theory]
    [ClassData(typeof(DataForTheoryWhenTasksComeInDifferentWindows))]
    public async void Invoke_WhenTasksComeInDifferentWindows_ShouldExecuteOnlyAllowedToWindow(DateTime startDate,
        int portions, DateTime[] dates, int[] tasksCount, int configCount, int configDuration, int expected)
    {
        // Arrange
        var timeStub = new SystemDateStub(startDate);
        var config = new RateLimiterConfig(configCount, configDuration);
        var cut = new RateLimiter<int>(config, timeStub);
        var tasksList = new List<Task<Result<int>>>();

        for (int i = 0; i < portions; i++)
        {
            timeStub.Now = dates[i];

            for (int j = 0; j < tasksCount[i]; j++)
            {
                var innerTask = Task.FromResult(1);
                var func = new Func<Task<int>>(() => innerTask);
                
                // Act
                var task = Task.Run(async () => await cut.Invoke(func, CancellationToken.None));
                
                tasksList.Add(task);    
            }
            
            await Task.WhenAll(tasksList);
        }
        
        // Assert
        Assert.Equal(expected, tasksList.Count(x => x.Result.Value == 1));
    }
    
    private class DataForTheoryWhenTasksComeInDifferentWindows : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new DateTime(2000, 1, 1, 1 ,1 ,1),
                3,
                new []
                { 
                    new DateTime(2000, 1, 1, 1 ,1 ,1),
                    new DateTime(2000, 1, 1, 1 ,1 ,2),
                    new DateTime(2000, 1, 1, 1 ,1 ,3)
                },
                new [] { 5, 4, 3 },
                10, 
                1,
                12
            };
            yield return new object[]
            {
                new DateTime(2000, 1, 1, 1 ,1 ,1),
                3,
                new []
                { 
                    new DateTime(2000, 1, 1, 1 ,1 ,1),
                    new DateTime(2000, 1, 1, 1 ,1 ,2),
                    new DateTime(2000, 1, 1, 1 ,1 ,3)
                },
                new [] { 5, 4, 3 },
                3, 
                1,
                9
            };
            yield return new object[]
            {
                new DateTime(2000, 1, 1, 1 ,1 ,1),
                3,
                new []
                { 
                    new DateTime(2000, 1, 1, 1 ,1 ,1),
                    new DateTime(2000, 1, 1, 1 ,1 ,2),
                    new DateTime(2000, 1, 1, 1 ,1 ,3)
                },
                new [] { 5, 4, 3 },
                4, 
                2,
                7
            };
            yield return new object[]
            {
                new DateTime(2000, 1, 1, 1 ,1 ,1),
                3,
                new []
                { 
                    new DateTime(2000, 1, 1, 1 ,1 ,1),
                    new DateTime(2000, 1, 1, 1 ,1 ,2),
                    new DateTime(2000, 1, 1, 1 ,1 ,3)
                },
                new [] { 5, 4, 3 },
                10, 
                3,
                10
            };
        }
    
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    
    [Theory]
    [ClassData(typeof(DataForTheoryWhenTasksComeOneByOne))]
    public void Invoke_WhenTasksComeOneByOne_ShouldExecuteOnlyAllowedToWindow(DateTime startDate,
        int portions, DateTime[] dates, int[] tasksCount, int configCount, int configDuration)
    {
        // Arrange
        var timeStub = new SystemDateStub(startDate);
        var config = new RateLimiterConfig(configCount, configDuration);
        var cut = new RateLimiter<int>(config, timeStub);
        
        for (int i = 0; i < portions; i++)
        {
            timeStub.Now = dates[i];
            
            for (int j = 0; j < tasksCount[i]; j++)
            {
                var innerTask = Task.FromResult(1);
                var func = new Func<Task<int>>(() => innerTask);
                
                // Act
                var task = Task.Run(async () => await cut.Invoke(func, CancellationToken.None));

                task.Wait();
                var result = task.Result.Value;
                
                // Assert
                Assert.Equal(j < configCount ? 1 : 0, result);
            }
        }
    }
    
    private class DataForTheoryWhenTasksComeOneByOne : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new DateTime(2000, 1, 1, 1 ,1 ,1),
                2,
                new []
                { 
                    new DateTime(2000, 1, 1, 1 ,1 ,1),
                    new DateTime(2000, 1, 1, 1 ,1 ,2),
                },
                new [] { 6, 6 },
                5, 
                1
            };
        }
    
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    
    [Fact]
    public async void Invoke_WhenActionThrowsException_ShouldTaskReturnFail()
    {
        // Arrange
        const int tasksCount = 5;
        var timeStub = new SystemDateStub(new DateTime(2000, 1, 1, 1 ,1 ,1));
        var config = new RateLimiterConfig(tasksCount, 1);
        var cut = new RateLimiter<int>(config, timeStub);
        var tasksList = new List<Task<Result<int>>>();

        for (int i = 0; i < tasksCount; i++)
        {
            var innerTask = Task.FromException<int>(new Exception("boom!"));
            var func = new Func<Task<int>>(() => innerTask);
            var task = Task.Run(async () => await cut.Invoke(func, CancellationToken.None));
            tasksList.Add(task);
        }
        
        // Act
        await Task.WhenAll(tasksList);
        
        // Assert
        Assert.Equal(tasksCount, tasksList.Count(x => x.Result.Value == 0));
    }
}