using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SenderTo.Core;
using SenderTo.Core.Settings;

namespace SenderTo.Application.Services.PhotoService;

public class MediaService : IMediaService
{
    private readonly HttpClient _client;

    private readonly string _uploadedUrl =
        "https://cloud-api.yandex.net/v1/disk/resources/upload?path=disk:/SenderToPhotos/";
    
    public MediaService(IHttpClientFactory factory, IOptionsMonitor<YandexSettings> options)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("OAuth", options.CurrentValue.Token);
    }
    
    public async Task<string> SavePhoto(MemoryStream memoryStream)
    {
        var filename = $"{Guid.NewGuid().ToString()}.png";
        
        var response = await _client.GetAsync(_uploadedUrl + filename);
        var json = await response.Content.ReadAsStringAsync();
        var uploadedLink = JsonDocument.Parse(json).RootElement.GetProperty("href").GetString();

        if (uploadedLink is null)
            throw new Exception("не смог загрузить фотографию, не удалось получить ссылку для загрузки");

        memoryStream.Position = 0;
        var result = await _client.PutAsync(uploadedLink, new StreamContent(memoryStream));

        if (result.StatusCode != HttpStatusCode.Created)
            throw new Exception("фотография не была загружена на Сервер");

        return filename;

    }
}