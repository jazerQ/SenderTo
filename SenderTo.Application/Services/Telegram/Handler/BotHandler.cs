using Microsoft.Extensions.Options;
using SenderTo.Application.Services.PhotoService;
using SenderTo.Core;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SenderTo.Application.Services.Telegram.Handler;

public class BotHandler(
    IOptionsMonitor<TelegramSettings> options,
    IMediaService mediaService) : IBotHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (IsPhoto(update))
            {
                var responseMessage = await TryToSavePhoto(bot, update.Message!.Photo!.Last())
                    ? "Успешно Сохранил фотографию на диске"
                    : "Не смог сохранить фотографию на диске";
                
                await bot.SendMessage(
                    chatId: options.CurrentValue.AdminChat,
                    text: responseMessage,
                    cancellationToken: cancellationToken);
                return;
            }
            
            var msg = GetMessage(update);
            if (msg is null) return;
            
            ChatId chatId = msg.Chat.Id;
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
                    // await bot.SendMessage(chatId,
                    //     "Не понимаю",
                    //     cancellationToken: cancellationToken);
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
    
    private Message? GetMessage(Update update)
    {
        return update.Message!.Text is not null ? update.Message : null;
    }

    private bool IsPhoto(Update update)
    {
        return update.Message!.Photo is not null;
    }

    private async Task<bool> TryToSavePhoto(ITelegramBotClient bot, PhotoSize photo)
    {
        try
        {
            TGFile tgFile = await bot.GetFile(photo.FileId);

            await using var stream = new MemoryStream();
            await bot.DownloadFile(tgFile, stream);
            stream.Position = 0;

            var filename = await mediaService.SavePhoto(stream);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка {ex.Message}");
            return false;
        }
    }
}