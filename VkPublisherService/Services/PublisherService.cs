using Grpc.Core;
using GrpcPublisherServiceApp;
using Microsoft.Extensions.Options;
using VkPublisherService.Settings;

namespace VkPublisherService.Services;

public class PublisherService(IHttpClientFactory factory, IOptionsMonitor<VkSettings> options) : Publisher.PublisherBase
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly string _postUrl = "https://api.vk.com/method/wall.post";
    
    public override async Task<CreatePostResponse> CreatePost(CreatePostRequest request, ServerCallContext context)
    {
        var parameters = new Dictionary<string, string>()
        {
            { "owner_id", options.CurrentValue.GroupId },
            { "message", request.Content },
            { "access_token", options.CurrentValue.Token },
            { "v", "5.154" }
        };

        var content = new FormUrlEncodedContent(parameters);

        var response = await _client.PostAsync(_postUrl, content);
    }
}