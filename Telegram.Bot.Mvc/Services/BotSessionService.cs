using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Mvc.Core.Interfaces;
using Telegram.Bot.Mvc.Framework;
using Telegram.Bot.Mvc.Scheduler;
using Telegram.Bot.Mvc.Scheduler.Interfaces;
using Telegram.Bot.Mvc.Services.Settings;

namespace Telegram.Bot.Mvc.Services
{
    public class BotSessionService
    {
        private readonly BotRouter _router;
        private readonly ILogger<BotSessionService> _logger;
        private readonly ITokenStorage _tokenStorage;
        private readonly ConcurrentDictionary<string, BotSession> _sessions;
        private readonly string _certificateFilePath;
        private readonly string _publicBaseUrl;
        private readonly bool _registerCertificate;

        public BotSessionService(
            BotRouter router,
            ILogger<BotSessionService> logger,
            ITokenStorage tokenStorage,
            IOptions<BotSessionServiceSettings> options)
        {
            _router = router;
            _logger = logger;
            _tokenStorage = tokenStorage;

            _certificateFilePath = options.Value.CertificateFilePath;
            _publicBaseUrl = options.Value.PublicBaseUrl;
            _registerCertificate = options.Value.RegisterCertificate;

            _sessions = new ConcurrentDictionary<string, BotSession>();
        }

        public ConcurrentDictionary<string, BotSession> GetBotSessions()
        {
            var tokens = _tokenStorage.GetTokens();

            foreach (var token in tokens)
            {
                var session = new BotSession(new TelegramBotClient(token), _router, _logger, token,
                    new PriorityScheduler<long>(_logger));

                if (_registerCertificate)
                {
                    var webHookPath = Path.Combine(_publicBaseUrl, session.Username);

                    session
                        .RegisterCertificate(_certificateFilePath, webHookPath)
                        .Wait();
                }

                _sessions.AddOrUpdate(session.Username, session, (_, _) => session);
            }

            return _sessions;
        }
    }
}