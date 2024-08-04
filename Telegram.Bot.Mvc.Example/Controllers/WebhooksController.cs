using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Mvc.Framework;
using Telegram.Bot.Types;

namespace Telegram.Bot.Mvc.Example.Controllers
{
    [ApiController]
    [Route("/")]
    public class WebhooksController : Controller
    {
        private readonly Dictionary<string, BotSession> _sessions;

        public WebhooksController()
        {
            //_sessions = sessionService.GetBotSessions();
        }

        [HttpPost]
        public void Post(Update update) //—юда будут приходить апдейты
        {
            Console.WriteLine(update.Message.Text);
        }

        // POST api/Webhooks/[botUsername]
        //[HttpPost]
        //public async Task<IActionResult> Post(
        //    [FromBody] Update update)
        //{
        //    try
        //    {
        //        if (update == null)
        //            throw new ArgumentException("update is null!");

        //        //_sessions.TryGetValue(botUsername, out BotSession session);

        //        //if (session is null)
        //        //    throw new ArgumentException("session is null, bot token is not registered!");

        //        //var context = new BotContext(null, session, update);

        //        //await session.Router.Route(context);
        //    }
        //    catch (Exception ex)
        //    {
        //        //logger.LogError(ex, $"Error during {nameof(WebhooksController)} working:"); // context?.RouteData
        //    }

        //    return Ok(); // Suppress Errors ...
        //}
    }
}
