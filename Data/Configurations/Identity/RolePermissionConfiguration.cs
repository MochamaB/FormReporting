using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Identity
{
    /// <summary>
    /// EF Core configuration for RolePermission entity
    /// </summary>
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            // Table name
            builder.ToTable("RolePermissions");

            // Primary key
            builder.HasKey(e => e.RolePermissionId);

            // Properties
            builder.Property(e => e.RolePermissionId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.RoleId)
                .IsRequired();

            builder.Property(e => e.PermissionId)
                .IsRequired();

            builder.Property(e => e.IsGranted)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.AssignedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint
            builder.HasIndex(e => new { e.RoleId, e.PermissionId })
                .IsUnique()
                .HasDatabaseName("UQ_RolePermission");

            // Indexes
            builder.HasIndex(e => new { e.RoleId, e.IsGranted })
                .HasDatabaseName("IX_RolePermission_Role");

            builder.HasIndex(e => e.PermissionId)
                .HasDatabaseName("IX_RolePermission_Permission");

            // Relationships
            builder.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RolePermission_Role");

            builder.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RolePermission_Permission");

            builder.HasOne(e => e.Assigner)
                .WithMany()
                .HasForeignKey(e => e.AssignedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_RolePermission_AssignedBy");
        }
    }
}
