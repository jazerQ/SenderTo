using SenderTo.Application.Services.PhotoService;
using SenderTo.Application.Services.Telegram.Handler;
using SenderTo.Core.Settings;

namespace SenderTo;

public static class Injection
{
    public static WebApplicationBuilder GetServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient();
        #region Settings
        builder.Services.Configure<TelegramSettings>(
            builder.Configuration.GetSection("TelegramBot"));

        builder.Services.Configure<YandexSettings>(
            builder.Configuration.GetSection("YandexSettings"));
        
        builder.Services.Configure<RabbitSettings>(
            builder.Configuration.GetSection("RabbitSettings"));
        #endregion
        #region TransientServices
        builder.Services.AddTransient<IBotHandler, BotHandler>();
        #endregion
        #region SingletonServices
        builder.Services.AddSingleton<IMediaService, MediaService>();
        #endregion
        return builder;
    }
}