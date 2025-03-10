namespace ServiceX.Services;

public interface IServiceServices
{
    Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken);
}
