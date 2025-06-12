using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SenderTo.Application.Services.Telegram.Handler;
using SenderTo.Core;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace SenderTo.Application.Services.Telegram;

public class TelegramService(
    IOptionsMonitor<TelegramSettings> settings,
    IBotHandler botHandler) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bot = new TelegramBotClient(
            settings.CurrentValue.Token,
            cancellationToken: stoppingToken);

        var receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = [UpdateType.Message]
        };

        bot.StartReceiving(
            botHandler.HandleUpdateAsync,
            botHandler.HandleErrorAsync,
            receiverOptions,
            cancellationToken: stoppingToken);

        await SendMessageAboutBeginJob(bot, stoppingToken);
    }
    
    private async Task SendMessageAboutBeginJob(ITelegramBotClient bot, CancellationToken cs)
    {
        var me = await bot.GetMe(cs);
        Console.WriteLine($"Бот под названием {me.FirstName} запущен");
    }
}