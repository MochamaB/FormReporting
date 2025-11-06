using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Identity
{
    /// <summary>
    /// EF Core configuration for UserGroupMember entity
    /// </summary>
    public class UserGroupMemberConfiguration : IEntityTypeConfiguration<UserGroupMember>
    {
        public void Configure(EntityTypeBuilder<UserGroupMember> builder)
        {
            // Table name
            builder.ToTable("UserGroupMembers");

            // Primary key
            builder.HasKey(e => e.UserGroupMemberId);

            // Properties
            builder.Property(e => e.UserGroupMemberId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UserGroupId)
                .IsRequired();

            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.AddedBy)
                .IsRequired();

            builder.Property(e => e.AddedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint
            builder.HasIndex(e => new { e.UserGroupId, e.UserId })
                .IsUnique()
                .HasDatabaseName("UQ_UserGroupMember_Group_User");

            // Indexes
            builder.HasIndex(e => e.UserGroupId)
                .HasDatabaseName("IX_UserGroupMember_Group");

            builder.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_UserGroupMember_User");

            // Relationships
            builder.HasOne(e => e.UserGroup)
                .WithMany(g => g.Members)
                .HasForeignKey(e => e.UserGroupId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UserGroupMember_Group");

            builder.HasOne(e => e.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UserGroupMember_User");

            builder.HasOne(e => e.AddedByUser)
                .WithMany()
                .HasForeignKey(e => e.AddedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_UserGroupMember_AddedBy");
        }
    }
}
