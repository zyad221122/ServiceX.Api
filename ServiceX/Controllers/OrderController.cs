using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceX.Contracts.Order;
using ServiceX.Entites;
using ServiceX.Persistence;
using System;
using System.Security.Claims;

namespace ServiceX.Controllers;
[Route("api/[controller]")]
[ApiController]

public class OrderController(ApplicationDbContext _context,  IHttpContextAccessor _httpContextAccessor) : ControllerBase
{
    private readonly ApplicationDbContext context = _context;
    private readonly IHttpContextAccessor httpContextAccessor = _httpContextAccessor;

    [HttpPost("{techId}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateOrder([FromRoute]string techId, [FromBody] OrderRequest request)
    {
        if (request == null)
            return BadRequest("Invalid order data");
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");
        var technician = await _context.Technicians
            .Where(t => t.UserId == techId)
            .FirstOrDefaultAsync();
        if (technician == null)
        {
            return BadRequest("No available technician for this service.");
        }
        var order = new Order
        {
            UserId = userId,
            ServiceId = technician.ServiceId,
            TechnicianID = technician.UserId, // ✅ إضافة الفني
            ProblemDescription = request.ProblemDescription, // ✅ إضافة الفني
            Status = "Pending",
            createdOn = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, order);
    }
    [HttpGet("{id}")]
    [Authorize] // تأكد من أن المستخدم مسجل الدخول
    public async Task<IActionResult> GetOrderById(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        var order = await _context.Orders
            .Include(o => o.Service)
            .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

        if (order == null)
            return NotFound("Order not found or you don't have access");

        return Ok(order);
    }
    [HttpGet("my-orders")]
    [Authorize] // لضمان أن المستخدم مسجل الدخول
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Service)
            .ToListAsync();

        return Ok(orders);
    }


}
