using System.Text;
using Google.Protobuf;
using Grpc.Net.Client;
using GrpcDiskServiceApp;
using GrpcPublisherClientApp;
using Microsoft.Extensions.Options;
using QuoteService.Settings;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace QuoteService.Services;

public class RabbitMqListener : BackgroundService
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private readonly IOptionsMonitor<RabbitSettings> _options;
    private readonly IOptionsMonitor<PublisherSettings> _publisherSettings;
    private readonly IOptionsMonitor<DiskSettings> _diskSettings;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly NeuroService _neuroService;
    
    public RabbitMqListener(
        IOptionsMonitor<RabbitSettings> options,
        IOptionsMonitor<PublisherSettings> publisherSettings,
        IOptionsMonitor<DiskSettings> diskSettings,
        NeuroService neuroService)
    {
        _options = options;
        _publisherSettings = publisherSettings;
        _diskSettings = diskSettings;
        _factory = new ConnectionFactory
        {
            HostName = _options.CurrentValue.HostName,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
        _neuroService = neuroService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection = await GetConnectionAsync();
        var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
        
        await channel.QueueDeclareAsync(
            queue: _options.CurrentValue.QueueQuote,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        Console.WriteLine("Waiting Message");
        
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            byte[] body = eventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"Получено сообщение - {message}");
            var quote = await _neuroService.GetQuote();
            using var channelGrpcPublish = GrpcChannel.ForAddress(_publisherSettings.CurrentValue.HostName);
            using var channelGrpcDisk = GrpcChannel.ForAddress(_diskSettings.CurrentValue.HostName);
            var publishClient = new Publisher.PublisherClient(channelGrpcPublish);
            var imageClient = new DiskImager.DiskImagerClient(channelGrpcDisk);
            var downloadRequest = new ImageDownloadRequest { Filename = message };
                
            var downloadResponse = await imageClient.ImageDownloadAsync(downloadRequest);

            var publicRequest = new CreatePostRequest { Image = downloadResponse.Image, Content  = quote };

            var publicResponse = await publishClient.CreatePostAsync(publicRequest);

            Console.WriteLine($"{quote} - post - {publicResponse.PostId}");
            await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(
                eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken: stoppingToken);
        };
        await channel.BasicConsumeAsync(
            _options.CurrentValue.QueueQuote,
            autoAck: false,
            consumer,
            cancellationToken: stoppingToken);

    }
    
    private async Task<IConnection> GetConnectionAsync()
    {
        if (_connection is { IsOpen: true }) return _connection;
        
        try
        {
            await _lock.WaitAsync();
            if (_connection?.IsOpen == false || _connection is null)
            {
                _connection = await _factory.CreateConnectionAsync();
            }
        }
        finally
        {
            _lock.Release();
        }

        return _connection;
    }
}