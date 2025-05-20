namespace ServiceX.Contracts.ReviewContrctFileName;

public class ReviewRequest
{
    public int OrderId { get; set; }
    public int RatingValue { get; set; }
    public string Comments { get; set; }
}
