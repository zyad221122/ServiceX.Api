using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceX.Contracts.SupportMessage2;

namespace ServiceX.Controllers;
[Route("api/[controller]")]
[ApiController]
public class SupportController(ApplicationDbContext _context) : ControllerBase
{
    private readonly ApplicationDbContext context = _context;
    
    [HttpPost("send")]
    public async Task<IActionResult> SendSupportMessage([FromBody] SupportMessageRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("User not authenticated");

        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required");

        var message = new SupportMessage
        {
            Message = request.Message,
            CustomerId = userId
        };

        context.SupportMessages.Add(message);
        await context.SaveChangesAsync();

        return Ok(new { message = "تم إرسال رسالتك للدعم بنجاح" });
    }
}