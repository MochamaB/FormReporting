using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Identity
{
    /// <summary>
    /// EF Core configuration for RoleMenuItem entity
    /// </summary>
    public class RoleMenuItemConfiguration : IEntityTypeConfiguration<RoleMenuItem>
    {
        public void Configure(EntityTypeBuilder<RoleMenuItem> builder)
        {
            // Table name
            builder.ToTable("RoleMenuItems");

            // Primary key
            builder.HasKey(e => e.RoleMenuItemId);

            // Properties
            builder.Property(e => e.RoleMenuItemId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.RoleId)
                .IsRequired();

            builder.Property(e => e.MenuItemId)
                .IsRequired();

            builder.Property(e => e.IsVisible)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.AssignedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint
            builder.HasIndex(e => new { e.RoleId, e.MenuItemId })
                .IsUnique()
                .HasDatabaseName("UQ_RoleMenuItem");

            // Indexes
            builder.HasIndex(e => new { e.RoleId, e.IsVisible })
                .HasDatabaseName("IX_RoleMenuItem_Role");

            builder.HasIndex(e => e.MenuItemId)
                .HasDatabaseName("IX_RoleMenuItem_MenuItem");

            // Relationships
            builder.HasOne(e => e.Role)
                .WithMany(r => r.RoleMenuItems)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RoleMenuItem_Role");

            builder.HasOne(e => e.MenuItem)
                .WithMany(mi => mi.RoleMenuItems)
                .HasForeignKey(e => e.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RoleMenuItem_MenuItem");
        }
    }
}
