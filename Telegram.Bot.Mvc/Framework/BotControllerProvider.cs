using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Telegram.Bot.Mvc.Core.Interfaces;

namespace Telegram.Bot.Mvc.Framework;

public class BotControllerProvider : IBotControllerProvider
{
    private readonly Assembly _assembly;

    public BotControllerProvider(Assembly assembly)
    {
        _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
    }
    
    public IReadOnlyList<Type> GetBotControllers()
    {
        return _assembly
            .GetTypes()
            .Where(c => c.BaseType == typeof(BotController))
            .ToList()
            .AsReadOnly();
    }
}