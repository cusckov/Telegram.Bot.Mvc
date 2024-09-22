using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Mvc.Core.Interfaces;
using Telegram.Bot.Mvc.Scheduler.Interfaces;
using Telegram.Bot.Types;

namespace Telegram.Bot.Mvc.Framework
{
    public class BotSession
    {
        public IScheduler<long> Scheduler;
        public string Username => BotInfo.Username;
        public User BotInfo { get; protected set; }

        public ILogger Logger { get; protected set; }
        public IBotRouter Router { get; protected set; }
        public ITelegramBotClient Bot { get; protected set; }

        public string Token { get; protected set; }

        public IDictionary<long, ChatSession> ChatSessions { get; protected set; } = new Dictionary<long, ChatSession>();

        public BotSession(ITelegramBotClient client, IBotRouter router, ILogger logger, string token, IScheduler<long> scheduler)
        {
            Scheduler = scheduler;
            Bot = client;
            Logger = logger;
            Router = router;
            Token = token;
            BotInfo = client.GetMeAsync().Result;
            Clear();
        }

        public async Task RegisterCertificate(string certificatePath, string webHookPath)
        {
            using (var stream = new System.IO.FileStream(certificatePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                await Bot.SetWebhookAsync(webHookPath, new InputFileStream(stream, "cer.pem"));
            }
        }


        public void Clear()
        {
            _bag = new Dictionary<string, object>();
        }

        private IDictionary<string, object> _bag;

        public object this[string key]
        {
            get
            {
                _bag.TryGetValue(key, out object result);
                return result;
            }
            set
            {
                _bag[key] = value;
            }
        }
    }
}
