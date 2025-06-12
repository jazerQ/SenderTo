namespace SenderTo.Application.Services.PhotoService;

public interface IMediaService
{
    Task<string> SavePhoto(MemoryStream memoryStream);
}