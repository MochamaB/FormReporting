using FormReporting.Models.Entities.Hardware;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Hardware
{
    public class HardwareItemConfiguration : IEntityTypeConfiguration<HardwareItem>
    {
        public void Configure(EntityTypeBuilder<HardwareItem> builder)
        {
            // Primary Key
            builder.HasKey(hi => hi.HardwareItemId);

            // Unique Constraints
            builder.HasIndex(hi => hi.ItemCode).IsUnique();

            // Default Values
            builder.Property(hi => hi.IsActive).HasDefaultValue(true);
            builder.Property(hi => hi.CreatedDate).HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(hi => hi.Category)
                .WithMany(hc => hc.HardwareItems)
                .HasForeignKey(hi => hi.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(hi => hi.TenantHardware)
                .WithOne(th => th.HardwareItem)
                .HasForeignKey(th => th.HardwareItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
