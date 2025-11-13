using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Identity
{
    /// <summary>
    /// EF Core configuration for Role entity
    /// </summary>
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            // Table name
            builder.ToTable("Roles");

            // Primary key
            builder.HasKey(e => e.RoleId);

            // Properties
            builder.Property(e => e.RoleId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.RoleName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.RoleCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.ScopeLevelId)
                .IsRequired();

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Unique constraints
            builder.HasIndex(e => e.RoleName)
                .IsUnique()
                .HasDatabaseName("UQ_Role_Name");

            builder.HasIndex(e => e.RoleCode)
                .IsUnique()
                .HasDatabaseName("UQ_Role_Code");

            // Relationships
            // Foreign key to ScopeLevel
            builder.HasOne(e => e.ScopeLevel)
                .WithMany(sl => sl.Roles)
                .HasForeignKey(e => e.ScopeLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.RoleMenuItems)
                .WithOne(rmi => rmi.Role)
                .HasForeignKey(rmi => rmi.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
