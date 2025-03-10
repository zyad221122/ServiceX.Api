namespace ServiceX.Contracts.Order;

public class OrderRequest
{
    public string? UserId { get; set; }
    public string ProblemDescription { get; set; }

    public DateTime createdOn = DateTime.UtcNow;
}
