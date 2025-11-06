using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Identity
{
    /// <summary>
    /// EF Core configuration for MenuItem entity
    /// </summary>
    public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
    {
        public void Configure(EntityTypeBuilder<MenuItem> builder)
        {
            // Table name
            builder.ToTable("MenuItems");

            // Primary key
            builder.HasKey(e => e.MenuItemId);

            // Properties
            builder.Property(e => e.MenuItemId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.MenuTitle)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.MenuCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Icon)
                .HasMaxLength(50);

            builder.Property(e => e.Route)
                .HasMaxLength(200);

            builder.Property(e => e.Controller)
                .HasMaxLength(100);

            builder.Property(e => e.Action)
                .HasMaxLength(100);

            builder.Property(e => e.Area)
                .HasMaxLength(100);

            builder.Property(e => e.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(e => e.Level)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(e => e.IsVisible)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.RequiresAuth)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.ModifiedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint
            builder.HasIndex(e => e.MenuCode)
                .IsUnique()
                .HasDatabaseName("UQ_MenuItem_Code");

            // Indexes
            builder.HasIndex(e => new { e.ParentMenuItemId, e.DisplayOrder })
                .HasDatabaseName("IX_MenuItem_Parent");

            builder.HasIndex(e => e.ModuleId)
                .HasDatabaseName("IX_MenuItem_Module");

            builder.HasIndex(e => new { e.IsActive, e.IsVisible })
                .HasDatabaseName("IX_MenuItem_Active");

            builder.HasIndex(e => e.DisplayOrder)
                .HasDatabaseName("IX_MenuItem_Order");

            // Relationships
            builder.HasOne(e => e.ParentMenuItem)
                .WithMany(mi => mi.ChildMenuItems)
                .HasForeignKey(e => e.ParentMenuItemId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_MenuItem_Parent");

            builder.HasOne(e => e.Module)
                .WithMany(m => m.MenuItems)
                .HasForeignKey(e => e.ModuleId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_MenuItem_Module");

            builder.HasMany(e => e.ChildMenuItems)
                .WithOne(mi => mi.ParentMenuItem)
                .HasForeignKey(mi => mi.ParentMenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.RoleMenuItems)
                .WithOne(rmi => rmi.MenuItem)
                .HasForeignKey(rmi => rmi.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
