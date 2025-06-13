using Telegram.Bot;
using Telegram.Bot.Types;

namespace SenderTo.Application.Services.Telegram.Handler;

public interface IBotHandler
{
    Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken);

    Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken cancellationToken);
}