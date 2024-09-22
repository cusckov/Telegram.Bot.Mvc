using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Mvc.Framework;
using Telegram.Bot.Mvc.Scheduler.Interfaces;
using Telegram.Bot.Mvc.Services;
using Telegram.Bot.Types;

namespace Telegram.Bot.Mvc.Example.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhooksController : Controller
    {
        private readonly ILogger<WebhooksController> _logger;
        private readonly ConcurrentDictionary<string, BotSession?> _sessions;

        public WebhooksController(
            BotSessionService sessionService, 
            ILogger<WebhooksController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sessions = sessionService.GetBotSessions();
        }

        [HttpPost("{botUsername}")]
        public async Task<IActionResult> Post(
            [FromRoute] string botUsername,
            [FromBody] Update update)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(update);

                _sessions.TryGetValue(botUsername, out var session);

                if (session == null)
                    throw new ArgumentNullException(nameof(session));

                var context = new BotContext(null, session, update);

                await session.Router.Route(context);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while POST query in WebhookController:");
            }

            return Ok(); 
        }
    }
}
