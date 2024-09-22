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
        public StartController()
        {
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
        public async Task TestScheduler()
        {
            Logger.LogInformation(User.Username + " TestScheduler!");
            var actions = new List<Func<Task>>();
            for (int i = 0; i < 10; i++)
            {
                int localIndex = i;
                var chatId = Chat.Id;
                var botId = Context.BotSession.Username;
                actions.Add(async () =>
                {
                    await Bot.SendTextMessageAsync(chatId, $"Welcome {Chat.Username} {localIndex} от {botId}");
                });
            }

            await Scheduler.AddTasks(Bot.BotId!.Value, 0, actions.ToArray());
        }

        [BotPath("/test2", UpdateType.Message)]
        public async Task Test2Scheduler()
        {
            Logger.LogInformation(User.Username + " Test2Scheduler!");
            var actions2 = new List<Func<Task>>();
            for (int i = 0; i < 5; i++)
            {
                int localIndex = i;
                var chatId = Chat.Id;
                var botId = Context.BotSession.Username;
                actions2.Add(async () =>
                {
                    await Bot.SendTextMessageAsync(chatId,
                        $"Супер важное сообщение {localIndex} для {Chat.Username} от {botId}");
                });
            }

            await Scheduler.AddTasks(Bot.BotId!.Value, 1, actions2.ToArray());
        }
    }
}