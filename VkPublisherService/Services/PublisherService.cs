using System.Net.Http.Headers;
using System.Text.Json;
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
    private readonly string _savePhoto = "https://api.vk.com/method/photos.saveWallPhoto";
    
    public override async Task<CreatePostResponse> CreatePost(CreatePostRequest request, ServerCallContext context)
    {
        Console.WriteLine("Начало работы");
        var link = await GetUrl();
        
        var settings = await UploadFile(link, request.Image.ToByteArray());

        var ids = await SavePhoto(settings);
        
        var parameters = new Dictionary<string, string>()
        {
            { "owner_id", options.CurrentValue.GroupId },
            { "message", request.Content },
            {"attachments", $"photo{ids.ownerId}_{ids.photoId}"},
            { "access_token", options.CurrentValue.Token },
            { "v", "5.154" }
        };

        var content = new FormUrlEncodedContent(parameters);

        var response = await _client.PostAsync(_postUrl, content);

        return new CreatePostResponse()
        {
            PostId = JsonDocument.Parse(await response.Content.ReadAsStringAsync())
                .RootElement
                .GetProperty("response")
                .GetProperty("post_id")
                .GetInt32()
        };
    }

    private async Task<string?> GetUrl()
    {
        var paramets = new Dictionary<string, string>()
        {
            { "access_token", options.CurrentValue.UserToken },
            { "group_id", options.CurrentValue.GroupId },
            { "v", "5.131" }
        };
    
        var content = string.Join("&", paramets.Select(pr => $"{pr.Key}={Uri.EscapeDataString(pr.Value)}"));

        var response = await _client.GetAsync($"{_uploadPhoto}?{content}");
        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine(json);
        var link = JsonDocument.Parse(json).RootElement.GetProperty("upload_url").GetString();
        
        Console.WriteLine($"получил ссылку - {link}");
        return link;
    }

    private async Task<PostSettings> UploadFile(string link, byte[] image)
    {
        using (var form = new MultipartFormDataContent())
        {
            ByteArrayContent stream = new ByteArrayContent(image);
            stream.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            
            form.Add(stream, "photo", "upload.png");

            var response = await _client.PostAsync(link, form);

            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            return new PostSettings
            {
                Server = json.GetProperty("server").GetInt32(),
                Photo = json.GetProperty("photo").GetString(),
                Hash = json.GetProperty("hash").GetString()
            };

        }
    }

    private async Task<(int photoId, int ownerId)> SavePhoto(PostSettings settings)
    {
        var parameters = new Dictionary<string, string>
        {
            { "access_token", options.CurrentValue.UserToken },
            { "photo", settings.Photo },
            { "server", settings.Server.ToString() },
            { "hash", settings.Hash },
            { "v", "5.131" }
        };

        var content = new FormUrlEncodedContent(parameters);

        var response = await _client.PostAsync(_savePhoto, content);

        var responseString = await response.Content.ReadAsStringAsync();
        
        var photoId = JsonDocument.Parse(responseString).RootElement
            .GetProperty("response")[0]
            .GetProperty("id")
            .GetInt32();
        var ownerId = JsonDocument.Parse(responseString).RootElement
            .GetProperty("response")[0]
            .GetProperty("owner_id")
            .GetInt32();
        
        return (photoId, ownerId);
    }
}