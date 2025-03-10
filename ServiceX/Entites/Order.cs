using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ServiceX.Entites;

public class Order
{
    public int OrderId { get; set; }
    public string Status { get; set; }
    public string ProblemDescription { get; set; }
    public DateTime RequestDate { get; set; }
    public string CustomerId { get; set; }
    [JsonIgnore]
    public Customer Customer { get; set; }
    public string TechnicianID { get; set; }
    [JsonIgnore]
    public Technician Technician { get; set; }
    public DateTime createdOn = DateTime.UtcNow;
}
