using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Telegram.Bot.Mvc.Scheduler.Interfaces
{
    /// <summary>
    /// Used to throttle tasks according to their priority and the current load.
    /// </summary>
    public interface IScheduler<in T> : IDisposable
    {
        public Task AddTask(T syncKey, uint priority, Func<Task> action);

        public Task AddTasks(T syncKey, uint priority, params Func<Task>[] actions);
    }
}