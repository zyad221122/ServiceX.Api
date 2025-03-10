using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceX.Entites;
using ServiceX.Persistence;

namespace ServiceX.Controllers;
[Route("api/[controller]")]
[ApiController]
public class TechniciansController(ApplicationDbContext context) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;

    [HttpGet]
    public async Task<IActionResult> GetTechnicians()
    {
        var technicians = await _context.Technicians
            .Include(t => t.User) // نجلب بيانات المستخدم المرتبطة
            .Select(t => new
            {
                t.UserId,
                t.User.FirstName,
                t.User.LastName,
                t.User.Email,
                t.User.Address,
            })
            .ToListAsync();

        return Ok(technicians);
    }
    [HttpGet("search")]
    public async Task<IActionResult> SearchTechnicians([FromQuery] string? name, [FromQuery] string? address)
    {
        var query = _context.Technicians.Include(t => t.User).AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(t => t.User.FirstName.Contains(name) || t.User.LastName.Contains(name));
        }

        if (!string.IsNullOrEmpty(address))
        {
            query = query.Where(t => t.Address.Contains(address));
        }

        var technicians = await query.Select(t => new
        {
            t.UserId,
            t.User.FirstName,
            t.User.LastName,
            t.User.Email,
            t.Address
        }).ToListAsync();

        return Ok(technicians);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTechnician(string id, [FromBody] Technician updatedTechnician)
    {
        var technician = await _context.Technicians.FindAsync(id);
        if (technician == null)
        {
            return NotFound("Technician not found.");
        }

        technician.Address = updatedTechnician.Address;
        await _context.SaveChangesAsync();

        return Ok("Technician updated successfully.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTechnician(string id)
    {
        var technician = await _context.Technicians.FindAsync(id);
        if (technician == null)
        {
            return NotFound("Technician not found.");
        }

        _context.Technicians.Remove(technician);
        await _context.SaveChangesAsync();

        return Ok("Technician deleted successfully.");
    }
}