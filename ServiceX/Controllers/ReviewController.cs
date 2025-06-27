using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceX.Contracts.ReviewContrctFileName;
using ServiceX.Contracts.Service;

namespace ServiceX.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ReviewController  (ApplicationDbContext _context) : ControllerBase
{
    private readonly ApplicationDbContext context = _context;
    
    [HttpPost]
    public async Task<IActionResult> AddReview([FromBody] ReviewRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        var order = await context.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.OrderId == request.OrderId && o.CustomerId == userId);

        if (order == null)
            return NotFound("Order not found or you are not authorized");

        // تأكد إن الأوردر مكتمل
        if (order.Status != "Completed")
            return BadRequest("You can only review completed orders");

        // تأكد إنه مفيش تقييم سابق على نفس الأوردر
        var existingReview = await context.Reviews
            .FirstOrDefaultAsync(r => r.OrderId == request.OrderId);

        if (existingReview != null)
            return BadRequest(new {message="أنت بالعفل قيمت هذا الأوردر من قبل"});

        var review = new Review
        {
            OrderId = request.OrderId,
            RatingValue = request.RatingValue,
            Comments = request.Comments,
            UserId = userId
        };

        context.Reviews.Add(review);
        await context.SaveChangesAsync();

        return Ok(new { message = "تم إضافة التقييم بنجاح" });
    }

    [HttpGet("my-reviews")]
    public async Task<IActionResult> GetMyReviews()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var reviews = await context.Reviews
            .Where(r => r.UserId == userId)
            .ToListAsync();

        return Ok(reviews.Adapt<IEnumerable<ReviewResponse>>());
    }

    [HttpGet("average-rating/{technicianId}")]
    public async Task<IActionResult> GetAverageRatingForTechnician([FromRoute]string technicianId)
    {
        var orders = await context.Orders
            .Where(o => o.TechnicianID == technicianId)
            .Select(o => o.OrderId)
            .ToListAsync();

        if (!orders.Any())
            return NotFound("This technician has no orders");

        var reviews = await context.Reviews
            .Where(r => orders.Contains(r.OrderId))
            .ToListAsync();

        if (!reviews.Any())
            return Ok(new { average = 0, message = "No reviews yet" });

        var averageRating = reviews.Average(r => r.RatingValue);

        return Ok(new
        {
            technicianId,
            average = Math.Round(averageRating, 2),
            totalReviews = reviews.Count
        });
    }

    [HttpGet("technician-reviews/{technicianId}")]
    public async Task<IActionResult> GetReviewsForTechnician(string technicianId)
    {
        var orders = await context.Orders
            .Where(o => o.TechnicianID == technicianId)
            .Select(o => o.OrderId)
            .ToListAsync();

        if (!orders.Any())
            return NotFound("This technician has no orders");

        var reviews = await context.Reviews
            .Where(r => orders.Contains(r.OrderId))
            .Include(r => r.Order)
            .Include(r => r.Customer)
            .ThenInclude(c => c.User)
            .ToListAsync();

        return Ok(reviews.Select(r => new
        {
            r.RatingValue,
            r.Comments,
            CustomerName = r.Customer?.User?.FirstName + " " + r.Customer?.User?.LastName,
            r.OrderId
        }));
    }


    [HttpGet("service/{serviceId}/technicians-with-reviews")]
    public async Task<IActionResult> GetTechniciansWithReviewsByService(int serviceId)
    {
        var technicians = await _context.Technicians
            .Where(t => t.ServiceId == serviceId)
            .Include(t => t.User)
            .Include(t => t.Service)
            .ToListAsync();

        if (!technicians.Any())
            return NotFound(new { message = "No technicians found for this service" });

        var technicianIds = technicians.Select(t => t.UserId).ToList();

        var orders = await context.Orders
            .Where(o => technicianIds.Contains(o.TechnicianID))
            .ToListAsync();

        var reviews = await context.Reviews
            .Where(r => orders.Select(o => o.OrderId).Contains(r.OrderId))
            .Include(r => r.Customer).ThenInclude(c => c.User)
            .ToListAsync();

        var result = technicians.Select(t =>
        {
            var technicianOrders = orders.Where(o => o.TechnicianID == t.UserId).Select(o => o.OrderId).ToList();
            var technicianReviews = reviews.Where(r => technicianOrders.Contains(r.OrderId)).ToList();

            var averageRating = technicianReviews.Any()
                ? Math.Round(technicianReviews.Average(r => r.RatingValue), 2)
                : 0;

            return new
            {
                TechnicalId = t.UserId,
                FullName = $"{t.User.FirstName} {t.User.LastName}",
                t.User.Email,
                t.User.Phone,
                t.Address,
                t.User.ImageUrl,
                t.PayByHour,
                ServiceName = t.Service.Name,
                AverageRating = averageRating,
                TotalReviews = technicianReviews.Count,
                Reviews = technicianReviews.Select(r => new
                {
                    r.RatingValue,
                    r.Comments,
                    CustomerName = r.Customer?.User?.FirstName + " " + r.Customer?.User?.LastName,
                    r.OrderId
                })
            };
        });

        return Ok(result);
    }


}
