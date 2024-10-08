﻿using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Mvc.Scheduler.Interfaces;

namespace Telegram.Bot.Mvc.Scheduler
{
    public class PerSecondScheduler : IScheduler
    {
        private ILogger _logger;
        private int _tasksCount;
        private int _inSeconds;
        private int _innerDelay;

        private MultiLevelPriorityQueue<Task> _queue;
        private Thread _thread;
        private Semaphore _semaphore;
        private ManualResetEvent _waitHandler = new ManualResetEvent(false);

        public PerSecondScheduler(ILogger<PerSecondScheduler> logger, IOptions<SchedulerOptions> options)
        {
            _logger = logger;
            _tasksCount = options.Value.TasksCount;
            _inSeconds = options.Value.InSeconds;
            _innerDelay = ((1000 * _inSeconds) / _tasksCount) + 1;
            _queue = new MultiLevelPriorityQueue<Task>();
            _semaphore = new Semaphore(_tasksCount, _tasksCount);
            _thread = new Thread(Start);
            _thread.Start();
        }

        public void Clear(uint priority)
        {
            _queue.Clear(priority);
        }

        public void Clear()
        {
            _queue.Clear(null);
        }

        public void Enqueue(Action action, uint priority = 0)
        {
            if (action == null) return;
            Enqueue(new Task(action), priority: priority);
        }

        public void Enqueue(int delay = 1000, uint priority = 0, params Action[] actions)
        {
            if (actions == null || actions.Length == 0) return;
            Enqueue(delay, priority, actions.Select(x => new Task(x)).ToArray());
        }

        public void Enqueue(Task task, uint priority = 0)
        {
            _queue.Enqueue(task, priority: priority);
            Resume();
        }

        public void Enqueue(int delay = 1000, uint priority = 0, params Task[] tasks)
        {
            var compiledTask = new Task(async () =>
            {
                for (int i = 0; i < tasks.Length; i++)
                {
                    if (i > 0) _semaphore.WaitOne();
                    tasks[i].Start();
                    await tasks[i];
                    if (i > 0) _semaphore.Release();
                    await Task.Delay(delay);
                }

                await Task.WhenAll(tasks);
            });
            Enqueue(compiledTask, priority);
        }

        public void Pause()
        {
            _waitHandler.Reset();
        }

        public void Resume()
        {
            _waitHandler.Set();
        }


        private volatile bool _runThread = true;

        private async void Start()
        {
            var handlers = new List<Task>();
            while (true)
            {
                _waitHandler.WaitOne();
                if (!_runThread) break;
                Task t = _queue.Dequeue();
                if (t == null)
                {
                    Pause();
                    continue;
                }

                _semaphore.WaitOne();
                var task = Task.Run(async () =>
                {
                    try
                    {
                        Task localTask = t;
                        localTask.Start();
                        _semaphore.Release();
                        await localTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error while start Scheduler");
                    }
                });

                lock (handlers)
                {
                    handlers.Add(task);
                    if (handlers.Count % 100 == 0) handlers.RemoveAll(x => x.IsCompleted);
                }

                await Task.Delay(_innerDelay);
            }

            handlers.Clear();
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _runThread = false;
                    _thread.Interrupt();
                    _thread.Join();
                    lock (_queue)
                    {
                        _queue.Dispose();
                    }
                }

                _thread = null;
                _queue = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }

    public class MultiLevelPriorityQueue<T> : IDisposable where T : class
    {
        private Queue<T>[] _queue;
        private const int PRIORITY_MAX = 5;
        private static readonly int[] QUEUE_SLOTS = new int[] { 15, 6, 5, 3, 1 };
        private static readonly int TASKS_COUNT = QUEUE_SLOTS.Sum();

        public MultiLevelPriorityQueue()
        {
            _queue = new Queue<T>[PRIORITY_MAX];
            for (int i = 0; i < _queue.Length; i++)
            {
                _queue[i] = new Queue<T>();
                _currantQueueSlots[i] = QUEUE_SLOTS[i];
            }
        }

        public void Enqueue(T item, uint priority = 0)
        {
            if (item == null) return;
            lock (_queue[priority])
            {
                _queue[priority].Enqueue(item);
            }
        }

        private int[] _currantQueueSlots = new int[PRIORITY_MAX];
        private volatile int _currentQueue = 0;

        private Queue<T> GetCurrentQueue()
        {
            int tries = 0;
            while (true)
            {
                int currentSlot = Volatile.Read(ref _currantQueueSlots[_currentQueue]);
                if (currentSlot > 0)
                    lock (_queue[_currentQueue])
                        if (_queue[_currentQueue].Count > 0)
                        {
                            var newValue = Interlocked.Decrement(ref _currantQueueSlots[_currentQueue]);
                            return _queue[_currentQueue];
                        }

                Volatile.Write(ref _currantQueueSlots[_currentQueue], QUEUE_SLOTS[_currentQueue]);
                _currentQueue = (_currentQueue + 1) % PRIORITY_MAX;
                tries++;
                if (tries >= PRIORITY_MAX * 2) return null;
            }
        }

        public T Dequeue()
        {
            T t = null;
            var queue = GetCurrentQueue();
            if (queue == null) return null;
            lock (queue)
            {
                if (queue.Count > 0) t = queue.Dequeue();
            }

            return t;
        }

        public void Clear(uint? priority)
        {
            if (priority.HasValue)
            {
                lock (_queue[priority.Value])
                {
                    _queue[priority.Value].Clear();
                }
            }
            else
            {
                for (uint i = 0; i < PRIORITY_MAX; i++) Clear(i);
            }
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var q in _queue)
                    {
                        q.Clear();
                    }

                    _queue = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}