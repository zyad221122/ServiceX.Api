using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ServiceX.Entites;

public class Review
{
    public int ReviewId { get; set; }
    public string Comments { get; set; }
    public int RatingValue { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; }
    public string UserId { get; set; }
    public Customer Customer { get; set; }
}
