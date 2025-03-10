using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ServiceX.Entites;

namespace ServiceX.Persistence.EntitiesConfigurations;

public class TechnicianConfigurations : IEntityTypeConfiguration<Technician>
{
    public void Configure(EntityTypeBuilder<Technician> builder)
    {
        builder.HasKey(t => t.UserId);
        builder.HasOne(t => t.User).WithOne().HasForeignKey<Technician>(t => t.UserId);
    }
}