namespace QuoteService.Settings;

public class RabbitSettings
{
    public string HostName { get; set; } = string.Empty;

    public string QueueQuote { get; set; } = string.Empty;

    public string ExchangeName { get; set; } = string.Empty;
}