using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ServiceX.Entites;

public class SupportMessage
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Message { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    public Customer Customer { get; set; }
}
