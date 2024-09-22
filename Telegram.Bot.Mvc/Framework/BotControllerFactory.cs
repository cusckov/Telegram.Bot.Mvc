using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Mvc.Core.Interfaces;
using Telegram.Bot.Mvc.Scheduler.Interfaces;

namespace Telegram.Bot.Mvc.Framework
{
    public class BotControllerFactory : IBotControllerFactory
    {
        private readonly IScheduler _scheduler;
        private readonly IServiceProvider _serviceProvider;
        private readonly IBotControllerProvider _controllerProvider;

        public BotControllerFactory(IScheduler scheduler, 
            IServiceProvider serviceProvider,
            IBotControllerProvider controllerProvider)
        {
            _scheduler = scheduler;
            _serviceProvider = serviceProvider;
            _controllerProvider = controllerProvider;
        }
        public BotController Create<TController>(BotContext context) where TController : BotController, new()
        {
            var controller = _serviceProvider.GetService<TController>();

            if (controller == null) 
                throw new Exception("Could Not Resolve Controller From Type!");

            controller.Context = context;
            controller.Scheduler = _scheduler;
            return controller;
        }

        public BotController Create(Type type, BotContext context)
        {
            if (type == null) 
                throw new Exception("Controller Type Not Found!");

            var controller = _serviceProvider.GetService(type) as BotController;
            if (controller == null) 
                throw new Exception("Could Not Create Controller From Type!");

            controller.Context = context;
            controller.Scheduler = _scheduler;
            return controller;
        }

        public IReadOnlyCollection<Type> GetControllers()
        {
            return _controllerProvider.GetBotControllers();
        }
    }
}