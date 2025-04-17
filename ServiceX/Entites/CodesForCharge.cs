namespace ServiceX.Entites;

public class CodesForCharge
{
    public int Id { get; set; }
    public string Code { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 15);
}
