using Mapster;
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
            .Include(o => o.User)
            .Include(o => o.Service)
            .FirstOrDefaultAsync();
        var customer = await _context.Customers
            .Where(t => t.UserId == userId)
            .Include(o => o.User)
            .FirstOrDefaultAsync();
        if (technician == null)
        {
            return BadRequest("No available technician for this service.");
        }
        var order = new Order
        {
            CustomerId = userId,
            Customer = customer,
            Technician = technician,
            TechnicianID = technician.UserId, // ✅ إضافة الفني
            ProblemDescription = request.ProblemDescription, // ✅ إضافة الفني
            Status = "Pending",
            createdOn = DateTime.UtcNow
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, order.Adapt<OrderResponset>());
    }
    [HttpGet("{id}")]
    [Authorize] 
    public async Task<IActionResult> GetOrderById(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        var order = await _context.Orders
            .Include(o => o.Technician)
            .Include(o => o.Technician.User)
            .Include(o => o.Technician.Service)
            .Include(o => o.Customer)
            .Include(o => o.Customer.User)
            .FirstOrDefaultAsync(o => o.OrderId == id && o.CustomerId == userId);

        if (order == null)
            return NotFound("Order not found or you don't have access");
        
        return Ok(order.Adapt<OrderResponset>());    
    }
    [HttpGet("my-orders")]
    [Authorize] // لضمان أن المستخدم مسجل الدخول
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        var orders = await _context.Orders
            .Where(o => o.CustomerId == userId)
            .Include(o => o.Technician)
            .Include(o => o.Technician.User)
            .Include(o => o.Technician.Service)
            .Include(o => o.Customer)
            .Include(o => o.Customer.User)
            .ToListAsync();
        return Ok(orders.Adapt<IEnumerable<OrderResponset>>());
    }
}
