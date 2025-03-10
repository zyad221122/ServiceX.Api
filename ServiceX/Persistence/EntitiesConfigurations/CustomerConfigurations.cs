using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ServiceX.Entites;

namespace ServiceX.Persistence.EntitiesConfigurations;

public class CustomerConfigurations : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.UserId);
        builder.HasOne(c => c.User).WithOne().HasForeignKey<Customer>(c => c.UserId);
    }
}