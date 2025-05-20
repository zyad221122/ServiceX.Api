using System.ComponentModel.DataAnnotations;

namespace ServiceX.Entites;

public class Service
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? ImageUrl { get; set; } = string.Empty;
    //public string? ImageUrl { get; set; } = string.Empty;
    //public string? TopServiceImage { get; set; } = string.Empty;
    public string Description { get; set; }
    public ICollection<Technician>? Technicians { get; set; }
}
