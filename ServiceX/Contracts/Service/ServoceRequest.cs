namespace ServiceX.Contracts.Service;

public class ServoceRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string? ImageUrl { get; set; }
    public IFormFile? Cover { get; set; } = default!;
}
