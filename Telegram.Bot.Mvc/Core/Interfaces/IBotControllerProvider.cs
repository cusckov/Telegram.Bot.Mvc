﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.Bot.Mvc.Core.Interfaces
{
    public interface IBotControllerProvider
    {
        IReadOnlyList<Type> GetBotControllers();
    }


}
