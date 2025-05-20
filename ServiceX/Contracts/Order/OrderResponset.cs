namespace ServiceX.Contracts.Order;

public class OrderResponset
{
    public int Id{ get; set; }
    public string UserName { get; set; }
    public string OrderStatus { get; set; }
    public string problemDescription { get; set; }
    public string TechnicianName { get; set; }
    public string ServiceName { get; set; }
    public bool isCompletedByCustomer { get; set; } 
    public bool isCompletedByTechnician { get; set; }
    public double Period { get; set; }
    public double Price { get; set; }

    //public DateTime createdOn { get; set; }
}
