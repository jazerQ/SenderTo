using Telegram.Bot;
using Telegram.Bot.Types;

namespace SenderTo.Application.Services.Telegram.Handler;

public class BotHandler : IBotHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        try
        {
            var msg = GetMessage(update);
            var chatId = msg.Chat.Id;
            var user = msg.From;
            
            await bot.SendMessage(chatId, $"Id этого чата - {chatId}", cancellationToken: cancellationToken);

            switch (msg.Text?.ToLower())
            {
                #region StartCommand

                case "/start":
                    await bot.SendMessage(chatId,
                        "Привет я твой помощник для ведения паблика Вконтакте",
                        cancellationToken: cancellationToken);
                    break;

                #endregion

                #region DefaultCommand

                default:
                    await bot.SendMessage(chatId,
                        "Не понимаю",
                        cancellationToken: cancellationToken);
                    break;

                #endregion
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"В чате произошла ошибка - {ex.Message}");
        }
    }

    public async Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
        Console.WriteLine($"StackTrace: {ex.StackTrace}");
        if (ex.InnerException != null)
            Console.WriteLine($"Inner: {ex.InnerException.Message}");
    }
    
    private Message GetMessage(Update update)
    {
        if (update is null ||
            update.Message is null ||
            update.Message.From is null ||
            string.IsNullOrEmpty(update.Message.Text)) 
            throw new Exception("не нашел обновления или сообщения");
        
        return update.Message!;
    }
}