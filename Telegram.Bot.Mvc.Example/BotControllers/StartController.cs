using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Mvc.Core;
using Telegram.Bot.Mvc.Core.Interfaces;
using Telegram.Bot.Mvc.Framework;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Mvc.Example.BotControllers
{
    public class StartController : BotController
    {
        private readonly ILogger _logger;
        
        public StartController(ILogger<StartController> logger)
        {
            _logger = logger;
        }

        [BotPath("/start", UpdateType.Message)]
        public async Task Start()
        {
            Logger.LogInformation(User.Username + " Joined The Channel!");
            await Bot.SendTextMessageAsync(Chat.Id, "Welcome!");
        }

        [AnyPath(UpdateType.Message)]
        public async Task Echo()
        {
            Logger.LogInformation("Message: (" + Message.Text + ")\nReceived From (" + User.Username + ")");
            await Bot.SendTextMessageAsync(Chat.Id, Message.Text);
        }

        [BotPath("/test", UpdateType.Message)]
        public Task TestScheduler()
        {
            Logger.LogInformation(User.Username + " TestScheduler!");
            var actions = new List<Func<Task>>();
            for (int i = 0; i < 100; i++)
            {
                int localIndex = i;
                var chatId = Chat.Id;
                actions.Add(async () =>
                {
                    //await Bot.SendTextMessageAsync(chatId, "Welcome " + localIndex + "!");
                    
                    _logger.LogInformation("Welcome " + localIndex + "!");
                });
            }
            Scheduler.EnqueueSequential(delay: 1000, priority: 0, CancellationToken.None, actions.ToArray());
            return Task.FromResult(0);
        }
    }
}
