using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Mvc.Scheduler.Interfaces;

namespace Telegram.Bot.Mvc.Scheduler
{
    public class PriorityScheduler<T> : IScheduler<T>
    {
        private readonly ConcurrentDictionary<T, IPriorityService> _channelDict = new();
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cts = new();

        public PriorityScheduler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task AddTask(T syncKey, uint priority, Func<Task> action)
        {
            var priorityService = _channelDict.GetOrAdd(syncKey, 
                _ => new PriorityService(_logger));

            await priorityService.EnqueueAsync(action, priority);
        }


        // Метод для последовательного выполнения с задержкой
        public async Task AddTasks(T syncKey, uint priority, params Func<Task>[] actions)
        {
            foreach (var action in actions)
            {
                await AddTask(syncKey, priority, action);
            }
        }

        // Освобождение ресурсов
        public void Dispose()
        {
            foreach (var service in _channelDict)
                service.Value.Dispose();

            _cts?.Cancel();
        }
    }
}