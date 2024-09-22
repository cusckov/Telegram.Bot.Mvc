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
    public interface IScheduler : IDisposable
    {
        /// <summary>
        /// Enqueue an action and schedule it to start at a later time depending on the load and its priority.
        /// </summary>
        /// <param name="action">An asynchronous action that will be executed.</param>
        /// <param name="priority">Zero-based value specifying the priority of the task, zero is the highest.</param>
        /// <param name="cancellationToken">Token to cancel the scheduled task.</param>
        /// <returns>A task representing the enqueuing process.</returns>
        Task Enqueue(Func<Task> action, uint priority = 0, CancellationToken cancellationToken = default);

        /// <summary>
        /// Enqueue an array of tasks to be executed sequentially with a specified delay between each task.
        /// </summary>
        /// <param name="delay">Minimum delay time in milliseconds between each action.</param>
        /// <param name="priority">Zero-based value specifying the priority of the tasks, zero is the highest.</param>
        /// <param name="actions">An array of asynchronous actions.</param>
        /// <param name="cancellationToken">Token to cancel the scheduled tasks.</param>
        /// <returns>A task representing the enqueuing process.</returns>
        Task EnqueueSequential(int delay = 1000, uint priority = 0, CancellationToken cancellationToken = default,
            params Func<Task>[] actions);

        /// <summary>
        /// Enqueue a task with a timeout, after which it will be cancelled if not completed.
        /// </summary>
        /// <param name="action">An asynchronous action to be executed.</param>
        /// <param name="timeout">Maximum time allowed for the task to complete.</param>
        /// <param name="priority">Zero-based value specifying the priority of the task, zero is the highest.</param>
        /// <returns>A task representing the enqueuing process.</returns>
        Task EnqueueWithTimeout(Func<Task> action, TimeSpan timeout, uint priority = 0);

        /// <summary>
        /// Pause the current execution of enqueued tasks gracefully.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resume the current execution of enqueued tasks from where it is paused.
        /// </summary>
        void Resume();

        /// <summary>
        /// Remove all unstarted tasks that are of a certain priority.
        /// </summary>
        /// <param name="priority">Zero-based value specifying the priority of the tasks to be removed, zero is the highest.</param>
        void Clear(uint priority);

        /// <summary>
        /// Remove all unstarted tasks for a specific user.
        /// </summary>
        /// <param name="userId">The identifier of the user whose tasks should be removed.</param>
        void ClearByUserId(long userId);

        /// <summary>
        /// Remove all unstarted tasks.
        /// </summary>
        void Clear();

        /// <summary>
        /// Get the count of tasks currently pending execution.
        /// </summary>
        /// <returns>The number of pending tasks.</returns>
        int GetPendingTaskCount();
    }
}