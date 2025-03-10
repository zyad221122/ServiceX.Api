using System.ComponentModel.DataAnnotations;

namespace ServiceX.Contracts;

public class LoginModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}