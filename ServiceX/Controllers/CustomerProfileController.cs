using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceX.Contracts.UserContract;
using System;
using System.Security.Claims;

namespace ServiceX.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CustomerProfileController(ApplicationDbContext context, IWebHostEnvironment env) : ControllerBase
{
  
    private readonly IWebHostEnvironment _env;
    private readonly ApplicationDbContext _context = context;

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync((userId));

        var customer = await _context.Customers
           .Where(t => t.UserId == userId)
           .Include(o => o.User)
           .FirstOrDefaultAsync();

        if (user == null)
            return NotFound("User not found");

        var result = new UserDto
        {
            FullName = customer!.User.FirstName + " " + customer.User.LastName,
            Email = customer.User.Email!,
            PhoneNumber = customer.User.Phone!,
            Address = customer.User.Address!,
            Balanace = customer.Balanace!,
            ProfileImageUrl = customer.User.ImageUrl!,
        };

        return Ok(result);
    }

    /*[Authorize]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMyAccount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(int.Parse(userId));

        if (user == null)
            return NotFound("User not found");

        if (!string.IsNullOrEmpty(user.ImageUrl))
        {
            var oldPath = Path.Combine(_env.WebRootPath, "images/Profiles", user.ImageUrl);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }*/
}
