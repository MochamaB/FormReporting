using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Identity
{
    /// <summary>
    /// EF Core configuration for UserGroup entity
    /// </summary>
    public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
    {
        public void Configure(EntityTypeBuilder<UserGroup> builder)
        {
            // Table name
            builder.ToTable("UserGroups");

            // Primary key
            builder.HasKey(e => e.UserGroupId);

            // Properties
            builder.Property(e => e.UserGroupId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.GroupName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.GroupCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.GroupType)
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

            // Unique constraint
            builder.HasIndex(e => e.GroupCode)
                .IsUnique()
                .HasDatabaseName("UQ_UserGroup_Code");

            // Indexes
            builder.HasIndex(e => e.TenantId)
                .HasDatabaseName("IX_UserGroup_Tenant");

            builder.HasIndex(e => e.GroupCode)
                .HasDatabaseName("IX_UserGroup_Code");

            builder.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_UserGroup_Active");

            // Relationships
            builder.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_UserGroup_Tenant");

            builder.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_UserGroup_Creator");

            builder.HasMany(e => e.Members)
                .WithOne(m => m.UserGroup)
                .HasForeignKey(m => m.UserGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
