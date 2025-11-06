using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Identity
{
    /// <summary>
    /// EF Core configuration for Module entity
    /// </summary>
    public class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            // Table name
            builder.ToTable("Modules");

            // Primary key
            builder.HasKey(e => e.ModuleId);

            // Properties
            builder.Property(e => e.ModuleId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.ModuleName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.ModuleCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.Icon)
                .HasMaxLength(50);

            builder.Property(e => e.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraints
            builder.HasIndex(e => e.ModuleName)
                .IsUnique()
                .HasDatabaseName("UQ_Module_Name");

            builder.HasIndex(e => e.ModuleCode)
                .IsUnique()
                .HasDatabaseName("UQ_Module_Code");

            // Indexes
            builder.HasIndex(e => new { e.IsActive, e.DisplayOrder })
                .HasDatabaseName("IX_Modules_Active");

            builder.HasIndex(e => e.ModuleCode)
                .HasDatabaseName("IX_Modules_Code");

            // Relationships
            builder.HasMany(e => e.Permissions)
                .WithOne(p => p.Module)
                .HasForeignKey(p => p.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.MenuItems)
                .WithOne(mi => mi.Module)
                .HasForeignKey(mi => mi.ModuleId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
