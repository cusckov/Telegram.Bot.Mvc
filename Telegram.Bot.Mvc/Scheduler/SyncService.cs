using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Mvc.Scheduler.Interfaces;

namespace Telegram.Bot.Mvc.Scheduler;

public class SyncService<T> : ISyncService<T>
{
    private readonly ConcurrentDictionary<T, Lazy<Channel<Func<Task>>>> _channelDict = new();

    private readonly CancellationTokenSource _cts = new();

    private readonly ILogger _logger;

    public SyncService(ILogger<SyncService<T>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SyncByChannel(T key, Func<Task> func)
    {
        var channel = _channelDict.GetOrAdd(key, _ =>
        {
            var newChannel = new Lazy<Channel<Func<Task>>>(() => Channel.CreateBounded<Func<Task>>(1),
                LazyThreadSafetyMode.ExecutionAndPublication);
            
            BackgroundProcessingChannel(key, newChannel.Value, _cts.Token);
            
            return newChannel;
        });

        await channel.Value.Writer.WriteAsync(func);
    }

    private async Task BackgroundProcessingChannel(T key, Channel<Func<Task>> channel, CancellationToken token)
    {
        try
        {
            await foreach (var func in channel.Reader.ReadAllAsync(token))
            {
                _logger.LogInformation("Execute method with key: {key}", key);

                try
                {
                    await func();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error while execution method:");
                    throw;
                }
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

    public void Dispose()
    {
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}