using FormReporting.Models.Entities.Organizational;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Organizational
{
    /// <summary>
    /// EF Core configuration for TenantGroup entity
    /// </summary>
    public class TenantGroupConfiguration : IEntityTypeConfiguration<TenantGroup>
    {
        public void Configure(EntityTypeBuilder<TenantGroup> builder)
        {
            // Table name
            builder.ToTable("TenantGroups");

            // Primary key
            builder.HasKey(e => e.TenantGroupId);

            // Properties
            builder.Property(e => e.TenantGroupId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.GroupName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.GroupCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.ModifiedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraints
            builder.HasIndex(e => e.GroupName)
                .IsUnique()
                .HasDatabaseName("UQ_TenantGroup_Name");

            builder.HasIndex(e => e.GroupCode)
                .IsUnique()
                .HasDatabaseName("UQ_TenantGroup_Code");

            // Indexes
            builder.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_TenantGroups_Active");

            builder.HasIndex(e => e.GroupCode)
                .HasDatabaseName("IX_TenantGroups_Code");

            // Relationships
            builder.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_TenantGroup_Creator");

            builder.HasMany(e => e.Members)
                .WithOne(m => m.TenantGroup)
                .HasForeignKey(m => m.TenantGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
