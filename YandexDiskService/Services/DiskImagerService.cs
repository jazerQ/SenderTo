using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Grpc.Core;
using GrpcDiskServiceApp;
using Microsoft.Extensions.Options;

namespace YandexDiskService.Services;

public class DiskImagerService : DiskImager.DiskImagerBase
{
    private readonly HttpClient _client;
    private readonly string _uploadedUrl =
        "https://cloud-api.yandex.net/v1/disk/resources/upload?path=disk:/SenderToPhotos/";

    public DiskImagerService(IHttpClientFactory httpClientFactory, IOptionsMonitor<YandexSettings> options)
    {
        _client = httpClientFactory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("OAuth", options.CurrentValue.Token);
    }
        
    public override async Task<ImageUploadResponse> ImageUpload(ImageUploadRequest request, ServerCallContext context)
    {
        var filename = $"{Guid.NewGuid().ToString()}.png";
        
        var response = await _client.GetAsync(_uploadedUrl + filename);
        var json = await response.Content.ReadAsStringAsync();
        var uploadedLink = JsonDocument.Parse(json).RootElement.GetProperty("href").GetString();

        if (uploadedLink is null)
            throw new RpcException(Status.DefaultCancelled, "не смог загрузить фотографию, не удалось получить ссылку для загрузки");

        var content = new ByteArrayContent(request.Image.ToByteArray());
        var result = await _client.PutAsync(uploadedLink, content);

        if (result.StatusCode != HttpStatusCode.Created)
            throw new RpcException(Status.DefaultCancelled, "фотография не была загружена на Сервер");

        return new ImageUploadResponse()
        {
            Filename = filename
        };
    }
}