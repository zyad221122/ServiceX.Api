using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ServiceX.Entites;

namespace ServiceX.Persistence.EntitiesConfigurations;

public class ReviewConfigurations : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.ReviewId);
        builder.HasOne(r => r.Order).WithMany().HasForeignKey(r => r.OrderId);
        builder.HasOne(r => r.Customer).WithMany(c => c.Reviews).HasForeignKey(r => r.UserId);
    }
}
