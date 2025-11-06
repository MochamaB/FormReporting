using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Identity
{
    /// <summary>
    /// EF Core configuration for User entity
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Table name
            builder.ToTable("Users");

            // Primary key
            builder.HasKey(e => e.UserId);

            // Properties
            builder.Property(e => e.UserId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UserName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.NormalizedUserName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.NormalizedEmail)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.EmailConfirmed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.PhoneNumber)
                .HasMaxLength(50);

            builder.Property(e => e.PhoneNumberConfirmed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.TwoFactorEnabled)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.LockoutEnabled)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.AccessFailedCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.EmployeeNumber)
                .HasMaxLength(50);

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
            builder.HasIndex(e => e.UserName)
                .IsUnique()
                .HasDatabaseName("UQ_User_UserName");

            builder.HasIndex(e => e.EmployeeNumber)
                .IsUnique()
                .HasDatabaseName("UQ_User_EmployeeNumber")
                .HasFilter("EmployeeNumber IS NOT NULL");

            // Indexes
            builder.HasIndex(e => e.NormalizedEmail)
                .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(e => e.NormalizedUserName)
                .HasDatabaseName("IX_Users_Username");

            builder.HasIndex(e => e.DepartmentId)
                .HasDatabaseName("IX_User_Department");

            // Relationships
            builder.HasOne(e => e.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_User_Department");

            builder.HasMany(e => e.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.TenantAccesses)
                .WithOne(ta => ta.User)
                .HasForeignKey(ta => ta.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.GroupMemberships)
                .WithOne(gm => gm.User)
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore computed property
            builder.Ignore(e => e.FullName);
        }
    }
}
