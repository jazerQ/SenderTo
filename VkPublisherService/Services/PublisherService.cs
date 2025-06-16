using Grpc.Core;
using GrpcPublisherServiceApp;
using Microsoft.Extensions.Options;
using VkPublisherService.Settings;

namespace VkPublisherService.Services;

public class PublisherService(IHttpClientFactory factory, IOptionsMonitor<VkSettings> options) : Publisher.PublisherBase
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly string _postUrl = "https://api.vk.com/method/wall.post";
    private readonly string _uploadPhoto = "https://api.vk.com/method/photos.getWallUploadServer";
    
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

    private string getUrl()
    {
        HttpRequestMessage request = new HttpRequestMessage();
        curl 'https://api.vk.com/method/photos.getWallUploadServer' \
        -F 'access_token=vk1.a.i1ds9XIqwke3EoxhHwifUi4_UVy-KTj9GP4C_jqmBYa-a7SSVpmbFN2OoJQar6Ql75KqX9zqnTe7ffbh9sg3u7WHQwAPxTSb2lD5DOv1GXdI5lT1S4xkljD_WfYa5kHi_LNq6Z7wTVDHU0xMy_WMrE2AJ9TpZsuOdPhNUcnsjG1NPJgC_TZWrMLoMSKEya7ASNxmjrb7EVnqUP8mhYdfMA' \
        -F 'v=5.131'
    }
}