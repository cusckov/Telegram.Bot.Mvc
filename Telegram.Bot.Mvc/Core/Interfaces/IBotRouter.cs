using System.Threading.Tasks;
using Telegram.Bot.Mvc.Framework;

namespace Telegram.Bot.Mvc.Core.Interfaces
{
    public interface IBotRouter
    {
        Task Route(BotContext context);
    }
}