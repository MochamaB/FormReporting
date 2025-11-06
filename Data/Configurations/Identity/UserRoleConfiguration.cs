using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Identity
{
    /// <summary>
    /// EF Core configuration for UserRole entity
    /// </summary>
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            // Table name
            builder.ToTable("UserRoles");

            // Primary key
            builder.HasKey(e => e.UserRoleId);

            // Properties
            builder.Property(e => e.UserRoleId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.RoleId)
                .IsRequired();

            builder.Property(e => e.AssignedDate)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Unique constraint: User can only have a role once
            builder.HasIndex(e => new { e.UserId, e.RoleId })
                .IsUnique()
                .HasDatabaseName("UQ_UserRole");

            // Indexes
            builder.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_UserRoles_User");

            builder.HasIndex(e => e.RoleId)
                .HasDatabaseName("IX_UserRoles_Role");

            // Relationships
            builder.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_UserRole_User");

            builder.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_UserRole_Role");

            builder.HasOne(e => e.Assigner)
                .WithMany()
                .HasForeignKey(e => e.AssignedBy)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
