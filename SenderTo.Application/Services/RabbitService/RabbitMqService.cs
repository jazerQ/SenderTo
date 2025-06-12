using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SenderTo.Core.Settings;

namespace SenderTo.Application.Services.RabbitService;

public class RabbitMqService : IBrokerService
{
    private readonly BasicProperties _basicProperties = new(){ Persistent = true};
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private readonly IOptionsMonitor<RabbitSettings> _options;
    private readonly SemaphoreSlim _lock = new(1, 1);
    
    public RabbitMqService(IOptionsMonitor<RabbitSettings> options)
    {
        _options = options; 
        _factory = new ConnectionFactory()
        {
            HostName = _options.CurrentValue.HostName,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
    }
    
    public async Task SendMessage(string message)
    {
        try
        {
            var connection = await GetConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            
            channel.BasicReturnAsync += async (sender, @event) =>
            {
                Console.WriteLine("Return from broker:");
                Console.WriteLine($"Reply Code: {@event.ReplyCode}");
                Console.WriteLine($"Reply Text: {@event.ReplyText}");
                Console.WriteLine($"Exchange: {@event.Exchange}");
                Console.WriteLine($"Routing Key: {@event.RoutingKey}");
                Console.WriteLine($"{message} - message with Error");
                Console.WriteLine(DateTime.Now);
            };

            await channel.ExchangeDeclareAsync(
                exchange: _options.CurrentValue.ExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                arguments: null);

            await channel.QueueDeclareAsync(
                queue: _options.CurrentValue.QueueName,
                durable: true, // означает что очередь будет сохраняться даже при перезапуске брокера
                exclusive: false, //exclusive: false означает что к данной очереди можно подключиться с других каналов
                autoDelete: false, // очередь не удалится автоматически даже если к ней никто не подключен
                arguments: null // доп настройки
            );

            await channel.QueueBindAsync(
                queue: _options.CurrentValue.QueueName,
                exchange: _options.CurrentValue.ExchangeName,
                routingKey: _options.CurrentValue.QueueName);

            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: _options.CurrentValue.ExchangeName,
                routingKey: _options.CurrentValue.QueueName,
                mandatory: true,
                basicProperties: _basicProperties,
                body: body
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RabbitMqException] - {ex.Message}");
            throw;
        }
    }

    private async Task<IConnection> GetConnectionAsync()
    {
        if (_connection?.IsOpen == false || _connection is null)
        {
            _connection = await _factory.CreateConnectionAsync();
        }

        return _connection;
    }
}