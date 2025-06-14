using System.Text;
using Google.Protobuf;
using Grpc.Net.Client;
using GrpcDiskClientApp;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WatermarkService.Settings;

namespace WatermarkService.Services;

public class RabbitMqListener : BackgroundService
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private readonly IOptionsMonitor<RabbitSettings> _options;
    private readonly IOptionsMonitor<DiskSettings> _diskSettings;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly MarkService _markService;

    public RabbitMqListener(IOptionsMonitor<RabbitSettings> options, IOptionsMonitor<DiskSettings> diskSettings,MarkService markService)
    {
        _options = options;
        _factory = new ConnectionFactory
        {
            HostName = _options.CurrentValue.HostName,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
        _markService = markService;
        _diskSettings = diskSettings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection = await GetConnectionAsync();
        var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync(  // создает новую очередь сообщений если ее еще нет
            queue: _options.CurrentValue.QueueName,
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
            using (var channel = GrpcChannel.ForAddress(_diskSettings.CurrentValue.HostName))
            {
                var client = new DiskImager.DiskImagerClient(channel);
                var downloadRequest = new ImageDownloadRequest { Filename = message };
                Console.WriteLine("Test!");
                var downloadResponse = await client.ImageDownloadAsync(downloadRequest);
                Console.WriteLine("HUI");
                var bytes = _markService.SetWatermark(downloadResponse.Image.ToByteArray());
                Console.WriteLine("Testgfregfre");
                var uploadRequest = new ImageUploadRequest { Image = ByteString.CopyFrom(bytes) };
                var uploadResponse = await client.ImageUploadAsync(uploadRequest);
                
                Console.WriteLine(uploadResponse.Filename);
            }

            await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(
                eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken: stoppingToken);
        };
        await channel.BasicConsumeAsync(
            _options.CurrentValue.QueueName,
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