using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ServiceX.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CodesForChargesController(ApplicationDbContext _context, IHttpContextAccessor _httpContextAccessor) : ControllerBase
{
    private readonly ApplicationDbContext context = _context;
    private readonly IHttpContextAccessor httpContextAccessor = _httpContextAccessor;

    [HttpPost]
    public async Task<IActionResult> AddCode()
    {
        var newCode = new CodesForCharge
        {
            Code = Guid.NewGuid().ToString("N").Substring(0, 15)
        };

        context.CodesForCharges.Add(newCode);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCode), new { id = newCode.Id }, newCode);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CodesForCharge>> GetCode(int id)
    {
        var code = await context.CodesForCharges.FindAsync(id);

        if (code == null)
            return NotFound();

        return code;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CodesForCharge>>> GetAllCodes()
    {
        var codes = await context.CodesForCharges.ToListAsync();
        return Ok(codes);
    }

    [HttpDelete("deleteByCode/{code}")]
    public async Task<IActionResult> DeleteCodeByCode(string code)
    {
        var codeToDelete = await context.CodesForCharges
                                         .FirstOrDefaultAsync(c => c.Code == code);

        if (codeToDelete == null)
        {
            return NotFound();
        }

        context.CodesForCharges.Remove(codeToDelete);
        await context.SaveChangesAsync();

        return NoContent(); // يعيد 204 No Content بعد الحذف
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("applyCode/{code}")]
    public async Task<IActionResult> ApplyCodeToBalance(string code)
    {
        // البحث عن الكود في قاعدة البيانات
        var codeInDb = await context.CodesForCharges.FirstOrDefaultAsync(c => c.Code == code);

        if (codeInDb == null)
        {
            return NotFound(new { message = "الكود غير صالح" });
        }

        // الحصول على المستخدم الحالي
        var currentUserId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var customer = await context.Customers.Include(c => c.User)
                                                .FirstOrDefaultAsync(c => c.UserId == currentUserId);

        if (customer == null)
        {
            return NotFound(new { message = "العميل غير موجود" });
        }

        customer.Balanace += 200;

        context.CodesForCharges.Remove(codeInDb);

        await context.SaveChangesAsync();

        return Ok(new { Message = "تم إضافة 200 إلى الرصيد بنجاح وتم مسح الكود", NewBalance = customer.Balanace });
    }
}
