using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ServiceX.Entites;

namespace ServiceX.Persistence.EntitiesConfigurations;

public class OrderConfigurations : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.OrderId);

        builder.HasOne(o => o.Customer)
               .WithMany(c => c.Orders)
               .HasForeignKey(o => o.CustomerId)
               .OnDelete(DeleteBehavior.Restrict); // منع الحذف التتابعي

        builder.HasOne(o => o.Technician)
               .WithMany(t => t.Orders)
               .HasForeignKey(o => o.TechnicianID)
               .OnDelete(DeleteBehavior.Restrict); // منع الحذف التتابعي
    }
}