namespace WatermarkService.Settings;

public class RabbitSettings
{
    public string HostName { get; set; } = string.Empty;

    public string QueueName { get; set; } = string.Empty;

    public string ExchangeName { get; set; } = string.Empty;
}