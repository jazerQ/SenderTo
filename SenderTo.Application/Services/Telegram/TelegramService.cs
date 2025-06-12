using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SenderTo.Core;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace SenderTo.Application.Services.Telegram;

public class TelegramService(IOptionsMonitor<TelegramSettings> settings) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bot = new TelegramBotClient(
            settings.CurrentValue.Token,
            cancellationToken: stoppingToken);

        var receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = [UpdateType.Message]
        };
        
        
    }
    
    private async Task SendMessageAboutBeginJob(ITelegramBotClient bot, CancellationToken cs)
    {
        var me = await bot.GetMe(cs);
        Console.WriteLine($"Бот под названием {me.FirstName} запущен");
    }
}