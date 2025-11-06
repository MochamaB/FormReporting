using FormReporting.Models.Entities.Organizational;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Organizational
{
    /// <summary>
    /// EF Core configuration for Region entity
    /// </summary>
    public class RegionConfiguration : IEntityTypeConfiguration<Region>
    {
        public void Configure(EntityTypeBuilder<Region> builder)
        {
            // Table name
            builder.ToTable("Regions");

            // Primary key
            builder.HasKey(e => e.RegionId);

            // Properties
            builder.Property(e => e.RegionId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.RegionNumber)
                .IsRequired();

            builder.Property(e => e.RegionName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.RegionCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(e => e.ModifiedDate)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Unique constraints
            builder.HasIndex(e => e.RegionNumber)
                .IsUnique()
                .HasDatabaseName("UQ_Region_Number");

            builder.HasIndex(e => e.RegionCode)
                .IsUnique()
                .HasDatabaseName("UQ_Region_Code");

            // Relationships
            builder.HasOne(e => e.RegionalManager)
                .WithMany()
                .HasForeignKey(e => e.RegionalManagerUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(e => e.Tenants)
                .WithOne(t => t.Region)
                .HasForeignKey(t => t.RegionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
