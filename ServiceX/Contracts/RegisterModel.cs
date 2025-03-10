using System.ComponentModel.DataAnnotations;

namespace ServiceX.Contracts;

public class RegisterModel
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Phone { get; set; }
    public int? ServiceId { get; set; } // المفتاح الأجنبي للخدمة
    [Required]
    [MinLength(6)]
    public string Password { get; set; }
    [Required]
    public string Role { get; set; }  // New: Admin, Customer, Technician
    public string? Address { get; set; }
}