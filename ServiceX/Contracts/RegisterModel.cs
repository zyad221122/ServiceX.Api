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

    public string? ImageUrl { get; set; }

    [Required]
    public string Phone { get; set; }
    
    public int? ServiceId { get; set; }
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; }
    
    [Required]
    public string Role { get; set; }  // New: Admin, Customer, Technician
    
    public string? Address { get; set; }

    public IFormFile? Cover { get; set; } = default!;
}