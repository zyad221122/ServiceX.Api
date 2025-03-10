using System.ComponentModel.DataAnnotations;

namespace ServiceX.Entites;

public class ApplicationUser :  IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Role { get; set; } // "User" or "Technician"
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
}
