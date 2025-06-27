namespace ServiceX.Contracts.Order;

public class OrderRequest
{
    public string? UserId { get; set; }
    public string ProblemDescription { get; set; }
    public string Address { get; set; }
    public string? Phone { get; set; }
    public DateOnly date { get; set; }
    public TimeOnly time { get; set; }
    public string? ImageUrl { get; set; }
    public IFormFile? Cover { get; set; } = default!;

    public DateTime createdOn = DateTime.UtcNow;
}
