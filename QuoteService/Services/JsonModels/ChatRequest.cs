namespace QuoteService.Services.JsonModels;

public class ChatRequest
{
    public string model { get; set; } = string.Empty;

    public List<Message> messages { get; set; } 
}

public class Message
{
    public string role { get; set; } = string.Empty;

    public string content { get; set; } = string.Empty;
}