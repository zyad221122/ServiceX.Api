using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServiceX.Entites;
using System.Reflection;

namespace ServiceX.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
                                : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
{

    public DbSet<Customer> Customers { get; set; }
    public DbSet<PasswordReset> PasswordResets { get; set; }
    public DbSet<Technician> Technicians { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<CodesForCharge> CodesForCharges { get; set; }
    public DbSet<SupportMessage> SupportMessages { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>()
            .HasOne(c => c.User)
            .WithOne()
            .HasForeignKey<Customer>(c => c.UserId);

        modelBuilder.Entity<Technician>()
            .HasOne(t => t.User)
            .WithOne()
            .HasForeignKey<Technician>(t => t.UserId);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
