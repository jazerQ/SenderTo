using SenderTo.Core;

namespace SenderTo;

public static class Injection
{
    public static WebApplicationBuilder GetServices(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<TelegramSettings>(
            builder.Configuration.GetSection("TelegramBot"));

        return builder;
    }
}