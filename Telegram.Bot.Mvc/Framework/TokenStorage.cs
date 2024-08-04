using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Telegram.Bot.Mvc.Core.Interfaces;

namespace Telegram.Bot.Mvc.Framework
{
    public class LocalTokenStorage : ITokenStorage
    {
        private readonly List<string> _tokens;

        public LocalTokenStorage(IOptions<LocalTokens> options)
        {
            _tokens = new List<string>(options.Value.Tokens);
        }
        public IEnumerable<string> GetTokens()
        {
            return _tokens;
        }
    }
}