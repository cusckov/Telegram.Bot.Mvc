using System;
using System.Collections.Concurrent;
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

        public PriorityScheduler(ISyncService<long> syncService, ILogger<PriorityScheduler> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        // Асинхронная версия метода Enqueue
        public async Task Enqueue(Func<Task> action, uint priority = 0, CancellationToken cancellationToken = default)
        {
            var channel = _priorityChannels.GetOrAdd(priority, _ =>
            {
                var newChannel = Channel.CreateBounded<Func<Task>>(1);
            
                BackgroundProcessingChannel(priority, newChannel, cancellationToken);
            
                return newChannel;
            });

            await channel.Writer.WriteAsync(async () => await ExecuteTask(action, priority, cancellationToken), cancellationToken);
        }

        private async Task BackgroundProcessingChannel(uint priority, Channel<Func<Task>> channel, CancellationToken token)
        {
            try
            {
                await foreach (var func in channel.Reader.ReadAllAsync(token))
                {
                    _logger.LogInformation("Execute method with priority: {key}", priority);

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

        // Метод для последовательного выполнения с задержкой
        public async Task EnqueueSequential(int delay = 1000, uint priority = 0, CancellationToken cancellationToken = default, params Func<Task>[] actions)
        {
            foreach (var action in actions)
            {
                await Enqueue(action, priority, cancellationToken);
                await Task.Delay(delay, cancellationToken); // Задержка между действиями
            }
        }

        // Метод для задания таймаута выполнения
        public async Task EnqueueWithTimeout(Func<Task> action, TimeSpan timeout, uint priority = 0)
        {
            var cts = new CancellationTokenSource(timeout);
            await Enqueue(action, priority, cts.Token);
        }

        // Метод для приостановки выполнения задач
        public void Pause()
        {
            _logger.LogInformation("Scheduler paused.");
            _semaphore.Wait(); // Заблокируем семафор для временной приостановки
        }

        // Метод для возобновления выполнения задач
        public void Resume()
        {
            _logger.LogInformation("Scheduler resumed.");
            _semaphore.Release(); // Освобождаем семафор для продолжения
        }

        // Очистка задач по приоритету
        public void Clear(uint priority)
        {
            if (_priorityChannels.TryRemove(priority, out var channel))
            {
                channel.Writer.Complete();
            }
        }

        // Очистка задач по идентификатору пользователя
        public void ClearByUserId(long userId)
        {
            //_syncService.Clear(userId);
        }

        // Очистка всех задач
        public void Clear()
        {
            foreach (var channel in _priorityChannels.Values)
            {
                channel.Writer.Complete(); // Закрываем все каналы
            }
            _priorityChannels.Clear();
        }

        // Метод для получения количества задач в очереди
        public int GetPendingTaskCount()
        {
            int count = 0;
            foreach (var channel in _priorityChannels.Values)
            {
                count += channel.Reader.Count;
            }
            return count;
        }

        // Вспомогательный метод для выполнения задачи с учетом лимитов Telegram
        private async Task ExecuteTask(Func<Task> task, uint priority, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken); // Ждем доступности слота для отправки сообщения
                await task();
                await Task.Delay(1000, cancellationToken); // Ограничение в 1 сообщение в секунду для одного пользователя
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing task with priority {Priority}", priority);
            }
            finally
            {
                _semaphore.Release(); // Освобождаем слот
            }
        }

        // Освобождение ресурсов
        public void Dispose()
        {
            _semaphore.Dispose();
        }
    }
}
