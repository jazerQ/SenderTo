using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace SenderTo.Application.Services;

public class TelegramService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bot = new TelegramBotClient()
    }
}