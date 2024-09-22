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
    public class PriorityScheduler : IScheduler
    {
        private readonly ISyncService<long> _syncService;
        private readonly ConcurrentDictionary<uint, Channel<Func<Task>>> _priorityChannels = new();
        private readonly SemaphoreSlim _semaphore = new(30); // Ограничение на 30 сообщений в секунду
        private readonly ILogger<PriorityScheduler> _logger;
        private readonly CancellationTokenSource _cts = new();

        public PriorityScheduler(ISyncService<long> syncService,
            ILogger<PriorityScheduler> logger)
        {
            _syncService = syncService;
            _logger = logger;

            _ = Task.Run(async () => await BackgroundProcessingPriorityChannel());
        }

        // Асинхронная версия метода Enqueue
        public async Task Enqueue(Func<Task> action, uint priority = 0, CancellationToken cancellationToken = default)
        {
            var channel = _priorityChannels.GetOrAdd(priority, _ => Channel.CreateUnbounded<Func<Task>>());

            await channel.Writer.WriteAsync(action, cancellationToken);
        }


        // Метод для последовательного выполнения с задержкой
        public async Task EnqueueSequential(int delay = 1000, uint priority = 0,
            CancellationToken cancellationToken = default, params Func<Task>[] actions)
        {
            foreach (var action in actions)
            {
                await Enqueue(action, priority, _cts.Token);
            }
        }

        private async Task BackgroundProcessingPriorityChannel()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    if (_priorityChannels.IsEmpty)
                        continue;

                    var key = _priorityChannels.Keys.Max();

                    _priorityChannels.TryGetValue(key, out var channel);

                    if (channel is null)
                        continue;
                    
                    if (!channel.Reader.TryRead(out var func))
                    {
                        _priorityChannels.TryRemove(key, out var removedFunc);

                        if (removedFunc == channel)
                            continue;
                    }

                    if(func is null)
                        continue;

                    try
                    {
                        await _semaphore.WaitAsync(_cts.Token); // Ждем доступности слота для отправки сообщения
                        await func();
                        await Task.Delay(1000,
                            _cts.Token); // Ограничение в 1 сообщение в секунду для одного пользователя
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing task with priority {Priority}", key);
                    }
                    finally
                    {
                        _semaphore.Release(); // Освобождаем слот
                    }
                }
                catch (OperationCanceledException)
                {
                    //TODO: завершить работу
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unknown exception:");
                }
            }
        }

        // Освобождение ресурсов
        public void Dispose()
        {
            _semaphore.Dispose();
            _cts?.Cancel();
        }
    }
}