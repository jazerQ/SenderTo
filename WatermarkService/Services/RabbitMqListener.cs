using System.Text;
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
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RabbitMqListener(IOptionsMonitor<RabbitSettings> options)
    {
        _options = options;
        _factory = new ConnectionFactory
        {
            HostName = _options.CurrentValue.HostName,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
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
    
            Console.WriteLine($"Received {message}");

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