using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceX.Contracts.Service;
using ServiceX.Entites;
using ServiceX.Persistence;
using ServiceX.Services;
using System.Threading;

[Route("api/[controller]")]
[ApiController]
public class ServiceController(ApplicationDbContext context, IServiceServices _serviceServices) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;
    private readonly IServiceServices serviceServices = _serviceServices;


    // ✅ 1️⃣ Get All Services (Everyone can access)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var services = await _context.Services.ToListAsync();
        return Ok(services.Adapt<IEnumerable<ServiceResponse>>());
    }

    // ✅ 2️⃣ Get Service By ID (Everyone can access)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null)
            return NotFound("Service not found");

        return Ok(service.Adapt<ServiceResponse>());
    }
    // ✅ البحث عن خدمة بالاسم (متاح للجميع)
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Service name is required");

        var services = await _context.Services
            .Where(s => s.Name.Contains(name))
            .ToListAsync();

        if (services.Count == 0)
            return NotFound("No matching services found");

        return Ok(services);
    }

    // ✅ 3️⃣ Create a New Service (Only Admin)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromForm] ServoceRequest service, CancellationToken cancellationToken)
    {
        if (service is null)
            return BadRequest("Invalid data");
        var imgname = await serviceServices.UploadImageAsync(service.Cover!, cancellationToken);
        var addedService = service.Adapt<Service>();
        addedService.ImageUrl = imgname;
        _context.Services.Add(addedService);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = addedService.Id }, addedService.Adapt<ServiceResponse>());
    }
    // ✅ 4️⃣ Update Service (Only Admin)
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] ServoceRequest updatedService)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null)
            return NotFound("Service not founded");

        service.Name = updatedService.Name;
        service.Description = updatedService.Description;

        await _context.SaveChangesAsync();
        return Ok(service);
    }
    [HttpGet("{serviceId}/technicians")]
    public async Task<IActionResult> GetTechniciansByService(int serviceId)
    {
        var technicians = await _context.Technicians
            .Where(t => t.ServiceId == serviceId)
            .Include(t => t.User) // جلب بيانات المستخدم لكل فني
            .Select(t => new
            {
                TechnicalId = t.UserId,
                FullName = $"{t.User.FirstName} {t.User.LastName}", // الاسم بالكامل
                t.User.Email,
                t.User.Phone,
                t.Address,
                t.Service.Name
            })
            .ToListAsync();

        if (!technicians.Any())
        {
            return NotFound(new { message = "No technicians found for this service" });
        }

        return Ok(technicians);
    }

    // ✅ 5️⃣ Delete Service (Only Admin)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null)
            return NotFound("Service not found");

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
