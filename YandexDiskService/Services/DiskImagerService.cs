using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Google.Protobuf;
using Grpc.Core;
using GrpcDiskServiceApp;
using Microsoft.Extensions.Options;

namespace YandexDiskService.Services;

public class DiskImagerService : DiskImager.DiskImagerBase
{
    private readonly HttpClient _client;
    private readonly string _uploadedUrl =
        "https://cloud-api.yandex.net/v1/disk/resources/upload?path=disk:/SenderToPhotos/";
    private readonly string _downloadedUrl =
        "https://cloud-api.yandex.net/v1/disk/resources/download?path=disk:/SenderToPhotos/";

    public DiskImagerService(IHttpClientFactory httpClientFactory, IOptionsMonitor<YandexSettings> options)
    {
        _client = httpClientFactory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("OAuth", options.CurrentValue.Token);
    }
        
    public override async Task<ImageUploadResponse> ImageUpload(ImageUploadRequest request, ServerCallContext context)
    {
        var filename = $"{Guid.NewGuid().ToString()}.png";
        var uploadedLink = await GetLink(_uploadedUrl + filename);
        
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

    public override async Task<ImageDownloadResponse> ImageDownload(ImageDownloadRequest request, ServerCallContext context)
    {
        var downloadedUrl = await GetLink(_downloadedUrl + request.Filename);
        if (downloadedUrl is null)
            throw new RpcException( Status.DefaultCancelled, "не смог загрузить фотографию, не удалось получить ссылку для загрузки");

        var fileBytesResponseMessage = await _client.GetAsync(downloadedUrl);
        if(fileBytesResponseMessage.StatusCode != HttpStatusCode.OK)
            throw new RpcException(Status.DefaultCancelled, "не смог загрузить фотографию");

        var fileBytes = await fileBytesResponseMessage.Content.ReadAsByteArrayAsync();

        return new ImageDownloadResponse()
        {
            Image = ByteString.CopyFrom(fileBytes)
        };
    }
    
    private async Task<string?> GetLink(string url)
    {
        var response = await _client.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        var link = JsonDocument.Parse(json).RootElement.GetProperty("href").GetString();

        return link;
    }
}