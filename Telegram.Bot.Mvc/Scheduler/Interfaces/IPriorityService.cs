using System;
using System.Threading;
using System.Threading.Tasks;

namespace Telegram.Bot.Mvc.Scheduler.Interfaces;

public interface IPriorityService: IDisposable
{
    Task EnqueueAsync(Func<Task> action, uint priority = 0);
}