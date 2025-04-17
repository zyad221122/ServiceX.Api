namespace ServiceX.Services;

public interface IAuthServices
{
    Task<string> UploadImageAsync(IFormFile file);
}
