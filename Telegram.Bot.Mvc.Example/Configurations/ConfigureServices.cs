using Telegram.Bot.Mvc.Core.Interfaces;
using Telegram.Bot.Mvc.Example.BotControllers;
using Telegram.Bot.Mvc.Extensions;
using Telegram.Bot.Mvc.Framework;
using Telegram.Bot.Mvc.Scheduler;
using Telegram.Bot.Mvc.Services;
using Telegram.Bot.Mvc.Services.Settings;
using ILogger = Telegram.Bot.Mvc.Core.Interfaces.ILogger;

namespace Telegram.Bot.Mvc.Example.Configurations
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddBotMvc(this IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<StartController>()
                .AddClasses(classes => classes.AssignableTo<BotController>())
                .AsSelf()
                .WithTransientLifetime());

            services.AddSingleton<ILogger, Logger>();
            services.AddSingleton<BotRouter>();

            services.Configure<SchedulerOptions>(options =>
            {
                options.InSeconds = 1;
                options.TasksCount = 30;
            });
            services.AddSingleton<PerSecondScheduler>();

            services.Configure<LocalTokens>(options =>
            {
                options.Tokens = ["5656932562:AAHvXyvs5mLh8kDCwuDLzacBdRybPm9ARns"];
            });
            services.AddSingleton<ITokenStorage, LocalTokenStorage>();

            services.Configure<BotSessionServiceSettings>(options =>
            {
                options.PublicBaseUrl = "https://example.com/api/Webhooks/";
                options.RegisterCertificate = false;
                options.CertificateFilePath = Path.Combine(Environment.CurrentDirectory, "Certificate", "cer.pem");
            });
            services.AddSingleton<BotSessionService>();

            services.AddSingleton<IBotControllerFactory, BotControllerFactory>();

            return services;
        }
    }
}
