namespace ServiceX.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TechnicalProfileController(ApplicationDbContext context) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;

    //[Authorize]
    //[HttpGet("me")]
    //public async Task<ActionResult<TechDto>> GetMyProfile()
    //{
    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    //    var technician = await _context.Technicians
    //       .Where(t => t.UserId == userId)
    //       .Include(o => o.User)
    //       .Include(o => o.Service)
    //       .FirstOrDefaultAsync();

    //    if (technician == null)
    //        return NotFound("User not found");

    //    var result = new TechDto
    //    {
    //        FullName = technician.User.FirstName + " " + technician.User.LastName,
    //        Email = technician.User.Email!,
    //        PhoneNumber = technician.User.Phone!,
    //        Address = technician.Address!,
    //        ProfileImageUrl = technician.User.ImageUrl!,
    //        ServiceName = technician.Service.Name,
    //        PayByHour = technician.PayByHour
    //    };

    //    return Ok(result);
    //}

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<object>> GetMyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var technician = await _context.Technicians
            .Where(t => t.UserId == userId)
            .Include(t => t.User)
            .Include(t => t.Service)
            .FirstOrDefaultAsync();

        if (technician == null)
            return NotFound("User not found");

        // Get all orders for this technician
        var technicianOrders = await _context.Orders
            .Where(o => o.TechnicianID == userId)
            .Select(o => o.OrderId)
            .ToListAsync();

        // Get all reviews for technician's orders
        var reviews = await _context.Reviews
            .Where(r => technicianOrders.Contains(r.OrderId))
            .Include(r => r.Customer)
            .ThenInclude(c => c.User)
            .ToListAsync();

        // Calculate average rating
        var averageRating = reviews.Any()
            ? Math.Round(reviews.Average(r => r.RatingValue), 2)
            : 0;

        var profile = new
        {
            FullName = technician.User.FirstName + " " + technician.User.LastName,
            Email = technician.User.Email!,
            PhoneNumber = technician.User.Phone!,
            Address = technician.Address!,
            ProfileImageUrl = technician.User.ImageUrl!,
            ServiceName = technician.Service.Name,
            PayByHour = technician.PayByHour,
            AverageRating = averageRating,
            TotalReviews = reviews.Count,
            Reviews = reviews.Select(r => new
            {
                r.RatingValue,
                r.Comments,
                CustomerName = r.Customer?.User?.FirstName + " " + r.Customer?.User?.LastName,
                r.OrderId
            })
        };

        return Ok(profile);
    }


}
