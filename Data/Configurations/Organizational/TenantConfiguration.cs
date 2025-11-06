using FormReporting.Models.Entities.Organizational;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Organizational
{
    /// <summary>
    /// EF Core configuration for Tenant entity
    /// </summary>
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            // Table name
            builder.ToTable("Tenants");

            // Primary key
            builder.HasKey(e => e.TenantId);

            // Properties
            builder.Property(e => e.TenantId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.TenantType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.TenantCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.TenantName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Location)
                .HasMaxLength(500);

            builder.Property(e => e.Latitude)
                .HasColumnType("decimal(10, 7)");

            builder.Property(e => e.Longitude)
                .HasColumnType("decimal(10, 7)");

            builder.Property(e => e.ContactPhone)
                .HasMaxLength(50);

            builder.Property(e => e.ContactEmail)
                .HasMaxLength(200);

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
            builder.HasIndex(e => e.TenantCode)
                .IsUnique()
                .HasDatabaseName("UQ_Tenant_Code");

            // Check constraint: TenantType must be 'HeadOffice', 'Factory', or 'Subsidiary'
            builder.HasCheckConstraint(
                "CHK_Tenant_Type",
                "TenantType IN ('HeadOffice', 'Factory', 'Subsidiary')");

            // Business rule: Factories must have RegionId, HeadOffice and Subsidiaries must not
            builder.HasCheckConstraint(
                "CHK_Tenant_Region",
                "(TenantType = 'Factory' AND RegionId IS NOT NULL) OR (TenantType IN ('HeadOffice', 'Subsidiary') AND RegionId IS NULL)");

            // Indexes for multi-tenant queries
            builder.HasIndex(e => new { e.TenantType, e.IsActive })
                .IncludeProperties(e => e.TenantName)
                .HasDatabaseName("IX_Tenants_Type");

            builder.HasIndex(e => new { e.RegionId, e.IsActive })
                .HasDatabaseName("IX_Tenants_Region")
                .HasFilter("TenantType = 'Factory'");

            builder.HasIndex(e => e.TenantCode)
                .HasDatabaseName("IX_Tenants_Code");

            builder.HasIndex(e => new { e.IsActive, e.TenantType })
                .HasDatabaseName("IX_Tenants_Active");

            // Relationships
            builder.HasOne(e => e.Region)
                .WithMany(r => r.Tenants)
                .HasForeignKey(e => e.RegionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Tenant_Region");

            builder.HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(e => e.ICTSupport)
                .WithMany()
                .HasForeignKey(e => e.ICTSupportUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Tenant_Creator");

            builder.HasOne(e => e.Modifier)
                .WithMany()
                .HasForeignKey(e => e.ModifiedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Tenant_Modifier");

            builder.HasMany(e => e.Departments)
                .WithOne(d => d.Tenant)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.GroupMemberships)
                .WithOne(gm => gm.Tenant)
                .HasForeignKey(gm => gm.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
