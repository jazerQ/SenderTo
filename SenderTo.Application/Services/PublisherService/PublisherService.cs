using Grpc.Core;
using GrpcPublisherServiceApp;
using Microsoft.Extensions.Options;
using SenderTo.Core.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SenderTo.Application.Services.PublisherService;

public class PublisherService(
    IOptionsMonitor<TelegramSettings> optionsTelegram,
    ITelegramBotClient bot) : Publisher.PublisherBase
{
    public override async Task<CreatePostResponse> CreatePost(CreatePostRequest request, ServerCallContext context)
    {
        using (var ms = new MemoryStream(request.Image.ToByteArray()))
        {
            await bot.SendPhoto(chatId: optionsTelegram.CurrentValue.PublishChannel,
                InputFile.FromStream(ms),
                caption: request.Content);
        }

        return new CreatePostResponse();
    }
}