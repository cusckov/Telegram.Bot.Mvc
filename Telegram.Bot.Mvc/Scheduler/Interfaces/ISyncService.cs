using System;
using System.Threading.Tasks;

namespace Telegram.Bot.Mvc.Scheduler.Interfaces;

public interface ISyncService<in T> : IDisposable
{
    Task SyncByChannel(T key, Func<Task> func);
}