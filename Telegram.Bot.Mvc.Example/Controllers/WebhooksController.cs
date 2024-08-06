using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Mvc.Framework;
using Telegram.Bot.Mvc.Services;
using Telegram.Bot.Types;

namespace Telegram.Bot.Mvc.Example.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhooksController : Controller
    {
        private readonly Dictionary<string, BotSession> _sessions;

        public WebhooksController(BotSessionService sessionService)
        {
            _sessions = sessionService.GetBotSessions();
        }

        [HttpPost("{botUsername}")]
        public async Task<IActionResult> Post(
            [FromRoute] string botUsername,
            [FromBody] Update update)
        {

            try
            {
                if (update == null)
                    throw new ArgumentException("update is null!");

                _sessions.TryGetValue(botUsername, out BotSession session);

                if (session == null)
                    throw new ArgumentException("session is null, bot token is not registered!");

                BotContext context = new BotContext(null, session, update);

                await session.Router.Route(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return Ok(); 
        }
    }
}
