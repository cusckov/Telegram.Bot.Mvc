using System.Collections.Generic;

namespace Telegram.Bot.Mvc.Core.Interfaces
{
    public interface ITokenStorage
    {
        IEnumerable<string> GetTokens();
    }
}