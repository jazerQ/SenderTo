using SenderTo.Application.Services.Telegram.Handler;
using SenderTo.Core;

namespace SenderTo;

public static class Injection
{
    public static WebApplicationBuilder GetServices(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<TelegramSettings>(
            builder.Configuration.GetSection("TelegramBot"));

        builder.Services.AddTransient<IBotHandler, BotHandler>();
        
        return builder;
    }
}