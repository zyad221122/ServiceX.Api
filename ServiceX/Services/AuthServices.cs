using ServiceX.Persistence;

namespace ServiceX.Services;

public class AuthServices(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment) : IAuthServices
{
    private readonly string _imagesPath = $"{webHostEnvironment.WebRootPath}/images/Profiles";//path of folder (images)
    private readonly ApplicationDbContext _context = context;
    public async Task<string> UploadImageAsync(IFormFile image)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
        var path = Path.Combine(_imagesPath, fileName);
        using var stream = File.Create(path);
        await image.CopyToAsync(stream);
        return "http://servicex.runasp.net/images/Profiles/" + fileName;
    }
}
