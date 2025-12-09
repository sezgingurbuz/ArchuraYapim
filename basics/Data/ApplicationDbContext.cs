using Microsoft.EntityFrameworkCore;
using basics.Models;
using basics.Areas.Admin.Models;

namespace basics.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<SeatingPlan> SeatingPlans { get; set; }
    public DbSet<Salon> Salonlar { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SeatingPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SalonAdi).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PlanAdi).HasMaxLength(255);
            entity.Property(e => e.PlanJson).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValue(DateTime.UtcNow);
            entity.Property(e => e.UpdatedAt).HasDefaultValue(DateTime.UtcNow);
        });
    }
}
