using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using ServiceX.Entites;
using ServiceX.Persistence;

namespace ServiceX.Services;
public class ServiceSeervices(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment) : IServiceServices
{

    private readonly string _imagesPath = $"{webHostEnvironment.WebRootPath}/images/services";//path of folder (images)
    private readonly ApplicationDbContext _context = context;

    //public async Task<Service> AddProductAsync(Service service)
    //{
    //    _context.Services.Add(service);
    //    await _context.SaveChangesAsync();
    //    return service;
    //}
    public async Task<string> UploadImageAsync(IFormFile image, CancellationToken cancellationToken)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
        var path = Path.Combine(_imagesPath, fileName);
        using var stream = File.Create(path);
        await image.CopyToAsync(stream, cancellationToken);
        return "http://servicex.runasp.net/images/services/" + fileName;
    }
}
