using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Identity
{
    /// <summary>
    /// EF Core configuration for ScopeLevel entity
    /// </summary>
    public class ScopeLevelConfiguration : IEntityTypeConfiguration<ScopeLevel>
    {
        public void Configure(EntityTypeBuilder<ScopeLevel> builder)
        {
            // Table name
            builder.ToTable("ScopeLevels");

            // Primary key
            builder.HasKey(e => e.ScopeLevelId);

            // Properties
            builder.Property(e => e.ScopeLevelId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.ScopeName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.ScopeCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.Level)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Unique constraints
            builder.HasIndex(e => e.ScopeName)
                .IsUnique()
                .HasDatabaseName("UQ_ScopeLevel_Name");

            builder.HasIndex(e => e.ScopeCode)
                .IsUnique()
                .HasDatabaseName("UQ_ScopeLevel_Code");

            // Index on Level for performance
            builder.HasIndex(e => e.Level)
                .HasDatabaseName("IX_ScopeLevel_Level");

            // Note: Seed data will be handled separately via seeding service
        }
    }
}
