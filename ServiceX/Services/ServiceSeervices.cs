using Microsoft.AspNetCore.Hosting;

namespace ServiceX.Services;
public class ServiceSeervices(IWebHostEnvironment webHostEnvironment) : IServiceServices
{

    private readonly string _imagesPath = $"{webHostEnvironment.WebRootPath}/images/services";//path of folder (images)
    public async Task<string> UploadImageAsync(IFormFile image, CancellationToken cancellationToken)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
        var path = Path.Combine(_imagesPath, fileName);
        using var stream = File.Create(path);
        await image.CopyToAsync(stream, cancellationToken);
        return  fileName;
    }
}
