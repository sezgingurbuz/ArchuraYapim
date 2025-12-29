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
    public DbSet<Etkinlik> Etkinlikler { get; set; }
    public DbSet<EtkinlikKoltuk> EtkinlikKoltuklari { get; set; }
    public DbSet<AdminUser> AdminUsers { get; set; }
    public DbSet<EtkinlikRapor> EtkinlikRaporlari { get; set; }
    public DbSet<GalleryImage> GalleryImages { get; set; }
    public DbSet<User> Users { get; set; }

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

        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.userName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.passwordHash).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("UTC_TIMESTAMP()");
        });
    }
}
