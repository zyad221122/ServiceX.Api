using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ServiceX.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AdminController(ApplicationDbContext context) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;

    [Authorize(Roles = "Admin")]
    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var usersCount = await _context.Customers
        .CountAsync();

        var techniciansCount = await _context.Technicians
        .CountAsync();

        var servicesCount = await _context.Services.CountAsync();

        var completedOrders = await _context.Orders
            .Where(o => o.Status == "Completed")
        .CountAsync();

        var notCompletedOrders = await _context.Orders
            .Where(o => o.Status != "Completed")
            .CountAsync();

        var result = new
        {
            UsersCount = usersCount,
            TechniciansCount = techniciansCount,
            ServicesCount = servicesCount,
            Orders = completedOrders + notCompletedOrders
        };

        return Ok(result);
    }

}
