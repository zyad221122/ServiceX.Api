namespace ServiceX.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
 #region Parameters
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        ApplicationDbContext context,
        IAuthServices _authenticationService, IEmailService emailService 
    #endregion
    ) : ControllerBase
{
    #region Fields
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly ApplicationDbContext _context = context;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IAuthServices authenticationService = _authenticationService;
    private readonly IEmailService emailService = emailService; 
    #endregion

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] RegisterModel model)
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

        var imgname = await authenticationService.UploadImageAsync(model.Cover!);

        // إنشاء المستخدم
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Phone = model.Phone,
            Address = model.Address,
            ImageUrl = imgname
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
                PayByHour = (int)model.PayByHour!,
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

    [HttpPost("forget-password")]
    public async Task<IActionResult> ForgetPassword([FromBody] SendOtpRequest sendOtpRequest)
    {
        var user = await _userManager.FindByEmailAsync(sendOtpRequest.Email);
        if (user == null)
            return NotFound(new { message = "عنوان البريد الإلكتروني غير متاح" });

        // إنشاء كود OTP عشوائي (مثلاً 6 أرقام)
        var otp = new Random().Next(100000, 999999).ToString();

        // خزّنه في جدول PasswordReset
        var resetEntry = new PasswordReset
        {
            Email = sendOtpRequest.Email,
            OtpCode = otp,
            ExpiryTime = DateTime.UtcNow.AddMinutes(10)
        };

        _context.PasswordResets.Add(resetEntry);
        await _context.SaveChangesAsync();

        // إرسال الإيميل
        await emailService.SendEmailAsync(user.Email!, "تغيير كلمه السر", $"{otp}");

        return Ok("OTP sent to email.");
    }
    
    [HttpPost("verify-otp")]
    public IActionResult VerifyOtp([FromBody] OtpVerifyModel model)
    {
        var otpEntry = _context.PasswordResets
            .FirstOrDefault(p => p.OtpCode == model.OtpCode);

        if (otpEntry == null || otpEntry.ExpiryTime < DateTime.UtcNow)
            return BadRequest(new { message = "رمز تحقق غير صحيح أو منتهي الصلاحيه" });

        return Ok("OTP verified");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
    {
        var otpEntry = _context.PasswordResets
            .FirstOrDefault(p => p.Email == model.Email );

        if (otpEntry == null)
            return BadRequest(new { message = "تحقق من البريد الإلكتروني" });

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return NotFound(new { message = "تحقق من البريد الإلكتروني" });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // حذف OTP بعد الاستخدام
        _context.PasswordResets.Remove(otpEntry);
        await _context.SaveChangesAsync();

        return Ok("تم تغيير كلمه السر بنجاح");
    }

}

