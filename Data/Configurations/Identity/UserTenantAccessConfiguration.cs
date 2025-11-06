using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Identity
{
    /// <summary>
    /// EF Core configuration for UserTenantAccess entity
    /// </summary>
    public class UserTenantAccessConfiguration : IEntityTypeConfiguration<UserTenantAccess>
    {
        public void Configure(EntityTypeBuilder<UserTenantAccess> builder)
        {
            // Table name
            builder.ToTable("UserTenantAccess");

            // Primary key
            builder.HasKey(e => e.AccessId);

            // Properties
            builder.Property(e => e.AccessId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.TenantId)
                .IsRequired();

            builder.Property(e => e.GrantedBy)
                .IsRequired();

            builder.Property(e => e.GrantedDate)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(e => e.Reason)
                .HasMaxLength(500);

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Unique constraint
            builder.HasIndex(e => new { e.UserId, e.TenantId })
                .IsUnique()
                .HasDatabaseName("UQ_UserTenant");

            // Indexes
            builder.HasIndex(e => new { e.UserId, e.IsActive })
                .HasDatabaseName("IX_UserTenantAccess_User");

            builder.HasIndex(e => new { e.TenantId, e.IsActive })
                .HasDatabaseName("IX_UserTenantAccess_Tenant");

            builder.HasIndex(e => e.ExpiryDate)
                .HasDatabaseName("IX_UserTenantAccess_Expiry")
                .HasFilter("ExpiryDate IS NOT NULL AND IsActive = 1");

            // Relationships
            builder.HasOne(e => e.User)
                .WithMany(u => u.TenantAccesses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_UserAccess_User");

            builder.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_UserAccess_Tenant");

            builder.HasOne(e => e.Grantor)
                .WithMany()
                .HasForeignKey(e => e.GrantedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_UserAccess_GrantedBy");
        }
    }
}
