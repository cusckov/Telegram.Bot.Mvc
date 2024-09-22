using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Mvc.Scheduler.Interfaces;

namespace Telegram.Bot.Mvc.Scheduler;

public class PriorityService : IPriorityService
{
    private readonly ConcurrentDictionary<uint, Channel<Func<Task>>> _priorityChannels = new();
    private readonly SemaphoreSlim _semaphore = new(30);
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _cts = new();

    public PriorityService(ILogger logger)
    {
        _logger = logger;

        _ = Task.Run(async () => await BackgroundProcessingPriorityChannel());
    }

    public async Task EnqueueAsync(Func<Task> action, uint priority = 0)
    {
        var channel = _priorityChannels.GetOrAdd(priority, _ => Channel.CreateUnbounded<Func<Task>>());

        await channel.Writer.WriteAsync(action, _cts.Token);
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
                    if (_priorityChannels.TryRemove(key, out _))
                        continue;
                }

                if (func is null)
                    continue;

                try
                {
                    await _semaphore.WaitAsync(_cts.Token);
                    await func();
                    await Task.Delay(1000, _cts.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing task with priority {Priority}", key);
                }
                finally
                {
                    _semaphore.Release();
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