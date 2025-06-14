using Google.Protobuf;
using Grpc.Net.Client;
using GrpcDiskClientApp;
using Microsoft.Extensions.Options;
using SenderTo.Application.Services.RabbitService;
using SenderTo.Core.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SenderTo.Application.Services.Telegram.Handler;

public class BotHandler(
    IOptionsMonitor<TelegramSettings> optionsTelegram,
    IOptionsMonitor<DiskSettings> optionsDisk,
    IBrokerService brokerService) : IBotHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message is null) return;
            
            if (IsPhoto(update))
            {
                await HandlePhotoAsync(bot, update.Message.Photo!.Last(), cancellationToken);
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

    private async Task HandlePhotoAsync(ITelegramBotClient bot, PhotoSize photo, CancellationToken cancellationToken)
    {
        var responseMessage = await TryToSavePhoto(bot, photo)
            ? "Успешно Сохранил фотографию на диске"
            : "Не смог сохранить фотографию на диске";
                
        await bot.SendMessage(
            chatId: optionsTelegram.CurrentValue.AdminChat,
            text: responseMessage,
            cancellationToken: cancellationToken);
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

            using var channel = GrpcChannel.ForAddress(optionsDisk.CurrentValue.HostName);
            
            var client = new DiskImager.DiskImagerClient(channel);
            var request = new ImageUploadRequest { Image = await ByteString.FromStreamAsync(stream) };
            
            var uploadResponse = await client.ImageUploadAsync(request);
            
            await brokerService.SendMessage(uploadResponse.Filename);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка {ex.Message}");
            return false;
        }
    }
}