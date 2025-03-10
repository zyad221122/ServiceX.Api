using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServiceX.Contracts;
using ServiceX.Entites;
using ServiceX.Persistence;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ServiceX.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AuthController(UserManager<ApplicationUser> userManager, 
                            SignInManager<ApplicationUser> signInManager,
                            RoleManager<IdentityRole> roleManager,
                            IConfiguration configuration,
                            ApplicationDbContext context) : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly ApplicationDbContext _context = context;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // التحقق من صحة الدور
        var validRoles = new HashSet<string> { "Technician", "Customer", "Admin" };
        if (!validRoles.Contains(model.Role))
            return BadRequest(new { message = "Invalid role specified." });

        // التحقق من أن ServiceId مطلوب فقط إذا كان المستخدم "Technician"
        if (model.Role == "Technician" && !model.ServiceId.HasValue)
            return BadRequest(new { message = "ServiceId is required for Technicians." });

        // إنشاء المستخدم
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Phone = model.Phone
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, model.Role);

        // إنشاء الكيان المناسب بناءً على الدور
        if (model.Role == "Technician")
        {
            _context.Technicians.Add(new Technician
            {
                UserId = user.Id,
                Address = model.Address,
                ServiceId = model.ServiceId!.Value
            });
        }
        else if (model.Role == "Customer")
        {
            _context.Customers.Add(new Customer
            {
                UserId = user.Id
            });
        }

        // لا حاجة لإنشاء كيان إضافي للإدمن
        await _context.SaveChangesAsync();
        return Ok(new { message = "User registered successfully." });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return Unauthorized("Invalid email or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
            return Unauthorized("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Customer";

        var token = GenerateJwtToken(user, role);
        return Ok(new { Token = token, Role = role });
    }

    private string GenerateJwtToken(ApplicationUser user, string role)
    {
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var singingCredentials = new SigningCredentials(symmetricSecurityKey/*key*/,
                                                        SecurityAlgorithms.HmacSha256/*Algorithm we used*/);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.FirstName),
            new Claim(ClaimTypes.GivenName, user.LastName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, role)
        };
        // مجموعه الكليمز اللي محتاج اضيفها مع التوكين وانا بعمله ريتيرن
        Claim[] claimss = [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("role", JsonSerializer.Serialize(role),JsonClaimValueTypes.JsonArray),
            //new(nameof(roles), string.Join(',', roles)),
        ];


        var token = new JwtSecurityToken(
            issuer : _configuration["Jwt:Issuer"],
            audience : _configuration["Jwt:Audience"],
            claims : claimss,
            expires: DateTime.UtcNow.AddMonths(2),
            signingCredentials: singingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

