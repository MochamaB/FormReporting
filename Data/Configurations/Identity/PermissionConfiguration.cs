using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Identity
{
    /// <summary>
    /// EF Core configuration for Permission entity
    /// </summary>
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            // Table name
            builder.ToTable("Permissions");

            // Primary key
            builder.HasKey(e => e.PermissionId);

            // Properties
            builder.Property(e => e.PermissionId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.ModuleId)
                .IsRequired();

            builder.Property(e => e.PermissionName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.PermissionCode)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.PermissionType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Check constraint
            builder.HasCheckConstraint(
                "CHK_Permission_Type",
                "PermissionType IN ('View', 'Create', 'Edit', 'Delete', 'Approve', 'Export', 'Manage', 'Custom')");

            // Unique constraints
            builder.HasIndex(e => e.PermissionCode)
                .IsUnique()
                .HasDatabaseName("UQ_Permission_Code");

            builder.HasIndex(e => new { e.ModuleId, e.PermissionCode })
                .IsUnique()
                .HasDatabaseName("UQ_Permission_Module");

            // Indexes
            builder.HasIndex(e => new { e.ModuleId, e.IsActive })
                .HasDatabaseName("IX_Permission_Module");

            builder.HasIndex(e => e.PermissionCode)
                .HasDatabaseName("IX_Permission_Code");

            builder.HasIndex(e => new { e.PermissionType, e.IsActive })
                .HasDatabaseName("IX_Permission_Type");

            builder.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_Permission_Active");

            // Relationships
            builder.HasOne(e => e.Module)
                .WithMany(m => m.Permissions)
                .HasForeignKey(e => e.ModuleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Permission_Module");

            builder.HasMany(e => e.RolePermissions)
                .WithOne(rp => rp.Permission)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
