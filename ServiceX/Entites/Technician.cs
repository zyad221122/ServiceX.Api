using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ServiceX.Entites;

public class Technician
{
    public int Id { get; set; } // المفتاح الأساسي
    public string UserId { get; set; } // المفتاح الأجنبي للمستخدم
    public ApplicationUser User { get; set; } // العلاقة مع `ApplicationUser`
    public string? Address { get; set; } // السماح بالقيم الفارغة
    public int ServiceId { get; set; } // المفتاح الأجنبي للخدمة
    public Service? Service { get; set; } // العلاقة مع `Service`
    [JsonIgnore]
    public ICollection<Order>? Orders { get; set; } // تهيئة القائمة
}
