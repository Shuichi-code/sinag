using Microsoft.EntityFrameworkCore;
using Sinag.Api.Models;

namespace Sinag.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<EquipmentPrice> EquipmentPrices => Set<EquipmentPrice>();
    public DbSet<ComponentSpec> ComponentSpecs => Set<ComponentSpec>();
    public DbSet<DlpcRate> DlpcRates => Set<DlpcRate>();
    public DbSet<IrradianceData> IrradianceCache => Set<IrradianceData>();
    public DbSet<SavedEstimate> SavedEstimates => Set<SavedEstimate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EquipmentPrice>(entity =>
        {
            entity.HasIndex(e => new { e.Category, e.Tier, e.EffectiveDate }).IsUnique();
            entity.Property(e => e.MinPricePhp).HasPrecision(10, 2);
            entity.Property(e => e.MaxPricePhp).HasPrecision(10, 2);
            entity.Property(e => e.DavaoDiscountPct).HasPrecision(4, 2);
        });

        modelBuilder.Entity<ComponentSpec>(entity =>
        {
            entity.HasIndex(e => new { e.Category, e.Tier, e.EffectiveDate }).IsUnique();
            entity.Property(e => e.WattageOrCapacity).HasPrecision(10, 2);
        });

        modelBuilder.Entity<DlpcRate>(entity =>
        {
            entity.Property(e => e.RatePerKwh).HasPrecision(8, 4);
        });

        modelBuilder.Entity<IrradianceData>(entity =>
        {
            entity.HasIndex(e => new { e.Latitude, e.Longitude, e.Month }).IsUnique();
            entity.Property(e => e.Latitude).HasPrecision(8, 5);
            entity.Property(e => e.Longitude).HasPrecision(8, 5);
            entity.Property(e => e.GhiKwhM2Day).HasPrecision(5, 2);
            entity.Property(e => e.PeakSunHours).HasPrecision(4, 2);
        });

        modelBuilder.Entity<SavedEstimate>(entity =>
        {
            entity.Property(e => e.SystemSizeKwp).HasPrecision(4, 1);
            if (Database.IsNpgsql())
                entity.Property(e => e.EstimateJson).HasColumnType("jsonb");
        });

        SeedData.Seed(modelBuilder);
    }
}
