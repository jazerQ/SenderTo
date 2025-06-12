namespace SenderTo.Application.Services.RabbitService;

public interface IBrokerService
{
    Task SendMessage(string message);
}