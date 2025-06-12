namespace SenderTo.Application.Services.PhotoService;

public interface IMediaService
{
    Task SavePhoto(MemoryStream memoryStream);
}