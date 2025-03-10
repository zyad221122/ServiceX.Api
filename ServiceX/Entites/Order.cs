using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ServiceX.Entites;

public class Order
{
    public int OrderId { get; set; }
    public string Status { get; set; }
    public DateTime RequestDate { get; set; }
    public string UserId { get; set; }
    public Customer Customer { get; set; }
    public string ProblemDescription { get; set; }
    public string TechnicianID { get; set; }
    public Technician Technician { get; set; }
    public int ServiceId { get; set; }
    public Service Service { get; set; }

    public DateTime createdOn = DateTime.UtcNow;
}
