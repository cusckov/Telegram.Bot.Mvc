using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Mvc.Core.Interfaces;
using Telegram.Bot.Mvc.Scheduler.Interfaces;

namespace Telegram.Bot.Mvc.Framework
{
    public abstract class BotController : IDisposable
    {
        public BotContext Context { get; set; }
        public IScheduler Scheduler { get; set; }

        private ChatSession _chatSession;
        private bool _disposed;

        public ChatSession ChatSession
        {
            get
            {
                if (_chatSession == null)
                {
                    if (Context.BotSession.ChatSessions.ContainsKey(Context.Chat.Id))
                    {
                        _chatSession = Context.BotSession.ChatSessions[Context.Chat.Id];
                    }
                    else
                    {
                        _chatSession = new ChatSession(Context.Chat.Id);
                        Context.BotSession.ChatSessions[Context.Chat.Id] = _chatSession;
                    }
                }
                return _chatSession;
            }
        }

        public User User => Context.User;
        public Chat Chat => Context.Chat;

        public Update Update => Context.Update;
        public Message Message => Context.Update.Message;
        public CallbackQuery Query => Context.Update.CallbackQuery;
        public ILogger Logger => Context.BotSession.Logger;
        public ITelegramBotClient Bot => Context.Bot;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
    }
}