namespace ServiceX.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TechnicalProfileController(ApplicationDbContext context) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<TechDto>> GetMyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var technician = await _context.Technicians
           .Where(t => t.UserId == userId)
           .Include(o => o.User)
           .Include(o => o.Service)
           .FirstOrDefaultAsync();

        if (technician == null)
            return NotFound("User not found");
        var result = new TechDto
        {
            FullName = technician.User.FirstName + " " + technician.User.LastName,
            Email = technician.User.Email!,
            PhoneNumber = technician.User.Phone!,
            Address = technician.Address!,
            ProfileImageUrl = technician.User.ImageUrl!,
            ServiceName = technician.Service.Name
        };

        return Ok(result);
    }

}
