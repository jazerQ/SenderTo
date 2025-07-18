using SenderTo.Application.Services.RabbitService;
using SenderTo.Application.Services.Telegram.Handler;
using SenderTo.Core.Settings;
using Telegram.Bot;

namespace SenderTo;

public static class Injection
{
    public static WebApplicationBuilder GetServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient();
        #region Settings
        builder.Services.Configure<TelegramSettings>(
            builder.Configuration.GetSection("TelegramBot"));
        builder.Services.Configure<DiskSettings>(
            builder.Configuration.GetSection("DiskSettings"));
        builder.Services.Configure<RabbitSettings>(
            builder.Configuration.GetSection("RabbitSettings"));
        #endregion
        #region TransientServices
        builder.Services.AddTransient<IBotHandler, BotHandler>();
        builder.Services.AddTransient<ITelegramBotClient, TelegramBotClient>(x => new TelegramBotClient(
            builder.Configuration.GetSection("TelegramBot").GetValue<string>("Token") ?? string.Empty));
        #endregion
        #region SingletonServices
        builder.Services.AddSingleton<IBrokerService, RabbitMqService>();
        #endregion
        return builder;
    }
}