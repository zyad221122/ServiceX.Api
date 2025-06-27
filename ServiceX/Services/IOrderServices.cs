namespace ServiceX.Services;

public interface IOrderServices
{
    Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken);
}
