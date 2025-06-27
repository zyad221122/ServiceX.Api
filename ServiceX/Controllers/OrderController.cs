namespace ServiceX.Controllers;
[Route("api/[controller]")]
[ApiController]

public class OrderController(ApplicationDbContext _context,  IHttpContextAccessor _httpContextAccessor, IOrderServices _orderServices) : ControllerBase
{
    private readonly ApplicationDbContext context = _context;
    private readonly IHttpContextAccessor httpContextAccessor = _httpContextAccessor;
    private readonly IOrderServices orderServices = _orderServices;

    [HttpPost("{techId}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateOrder([FromRoute]string techId, [FromForm] OrderRequest request, CancellationToken cancellationToken)
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

        //if (customer!.Balanace < 200)
        //{
        //    return BadRequest(new { message = "برجاء شحن المحفظه" });
        //}

        if (technician == null)
        {
            return BadRequest("No available technician for this service.");
        }
        
        DateTime requestDateTime = DateTime.Parse($"{request.date} {request.time}");
        if (requestDateTime < DateTime.Now)
        {
            return BadRequest(new  { message = "برجاء التأكد من التاريخ" });
        }

        var imgname = await orderServices.UploadImageAsync(request.Cover!, cancellationToken);
        var order = new Order
        {
            CustomerId = userId,
            Customer = customer,
            Technician = technician,
            TechnicianID = technician.UserId, // ✅ إضافة الفني
            ProblemDescription = request.ProblemDescription, // ✅ إضافة الفني
            //time = TimeOnly.Parse(request.time),
            time = request.time,
            //date = DateOnly.Parse(request.date),
            date = request.date,
            Phone = customer.User.Phone,
            Address = request.Address,
            ImageUrl = imgname,
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
    
    
    [HttpGet("my-orders/completed")]
    [Authorize]
    public async Task<IActionResult> GetMyCompletedOrders()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        var orders = await _context.Orders
            .Where(o => o.CustomerId == userId && o.Status == "Completed")
            .Include(o => o.Technician)
            .Include(o => o.Technician.User)
            .Include(o => o.Technician.Service)
            .Include(o => o.Customer)
            .Include(o => o.Customer.User)
            .ToListAsync();

        return Ok(orders.Adapt<IEnumerable<OrderResponset>>());
    }

    [HttpGet("technician-orders/completed")]
    [Authorize(Roles = "Technician")]
    public async Task<IActionResult> GetTechnicianCompletedOrders()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        var orders = await _context.Orders
            .Where(o => o.TechnicianID == userId && o.Status == "Completed")
            .Include(o => o.Customer)
            .Include(o => o.Customer.User)
            .Include(o => o.Technician)
            .Include(o => o.Technician.User)
            .Include(o => o.Technician.Service)
            .ToListAsync();

        return Ok(orders.Adapt<IEnumerable<OrderResponset>>());
    }


    [HttpGet("my-orders/pending")]
    [Authorize]
    public async Task<IActionResult> GetMyPendingOrders()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        var orders = await _context.Orders
            .Where(o => o.CustomerId == userId && o.Status == "Pending")
            .Include(o => o.Technician)
            .Include(o => o.Technician.User)
            .Include(o => o.Technician.Service)
            .Include(o => o.Customer)
            .Include(o => o.Customer.User)
            .ToListAsync();

        return Ok(orders.Adapt<IEnumerable<OrderResponset>>());
    }

    
    [HttpGet("technician-orders/pending")]
    [Authorize(Roles = "Technician")]
    public async Task<IActionResult> GetTechnicianPendingOrders()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        var orders = await _context.Orders
            .Where(o => o.TechnicianID == userId && o.Status == "Pending")
            .Include(o => o.Customer)
            .Include(o => o.Customer.User)
            .Include(o => o.Technician)
            .Include(o => o.Technician.User)
            .Include(o => o.Technician.Service)
            .ToListAsync();

        return Ok(orders.Adapt<IEnumerable<OrderResponset>>());
    }


    [HttpPut("complete-by-customer/{orderId}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CompleteByCustomer(int orderId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var order = await context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == userId);

        if (order == null)
            return NotFound("Order not found or you don't have access.");

        order.isCompletedByCustomer = true;

        var customer = await _context.Customers
            .Where(t => t.UserId == userId)
            .Include(o => o.User)
            .FirstOrDefaultAsync();
        if (order.isCompletedByTechnician)
        {
            order.Status = "Completed";
        }

        await context.SaveChangesAsync();
        return Ok(new { message = "Order marked as completed by customer." });
    }

   
    [HttpPut("complete-by-technician/{orderId}")]
    [Authorize(Roles = "Technician")]
    public async Task<IActionResult> CompleteByTechnician(int orderId, [FromBody] CompleteOrderDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // نجيب الأوردر مع الفني والعميل
        var order = await context.Orders
            .Include(o => o.Technician)
            .Include(o => o.Customer)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.TechnicianID == userId);

        if (order == null)
            return NotFound("Order not found or you don't have access.");

        if (order.isCompletedByTechnician)
            return BadRequest("Order is already completed by technician.");

        if (request.Period <= 0)
            return BadRequest(new { message = "برجاء إدخال مده أكبر من ال0" });

        // حساب السعر
        var price = order.Technician.PayByHour * request.Period;

        if (order.Customer.Balanace < price)
            return BadRequest(new { message  = "رصيد العميل غير كافي."});

        // تحديث البيانات
        order.Period = request.Period;
        order.Price = price;
        order.isCompletedByTechnician = true;
        order.Customer.Balanace -= price;

        if (order.isCompletedByCustomer)
            order.Status = "Completed";

        await context.SaveChangesAsync();

        return Ok(new
        {
            message = "تم الاكتمال من الأوردر.",
            totalPrice = price
        });
    }


    [HttpGet("top-services")]
    public async Task<IActionResult> GetTopServices()
    {
        var topServices = await context.Orders
            .Include(o => o.Technician)
            .ThenInclude(t => t.Service)
            .Where(o => o.Status == "Completed" && o.Technician != null && o.Technician.Service != null)
            .GroupBy(o => new { o.Technician.Service!.Name, o.Technician.Service.ImageUrl }) // نجمع حسب الاسم والصورة
            .Select(group => new
            {
                ServiceName = group.Key.Name,
                OrderCount = group.Count(),
                Image = group.Key.ImageUrl
            })
            .OrderByDescending(g => g.OrderCount)
            .ToListAsync();

        return Ok(topServices);
    }


}
