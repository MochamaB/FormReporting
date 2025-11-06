using FormReporting.Models.Entities.Organizational;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Organizational
{
    /// <summary>
    /// EF Core configuration for TenantGroupMember entity
    /// </summary>
    public class TenantGroupMemberConfiguration : IEntityTypeConfiguration<TenantGroupMember>
    {
        public void Configure(EntityTypeBuilder<TenantGroupMember> builder)
        {
            // Table name
            builder.ToTable("TenantGroupMembers");

            // Primary key
            builder.HasKey(e => e.GroupMemberId);

            // Properties
            builder.Property(e => e.GroupMemberId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.TenantGroupId)
                .IsRequired();

            builder.Property(e => e.TenantId)
                .IsRequired();

            builder.Property(e => e.AddedBy)
                .IsRequired();

            builder.Property(e => e.AddedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint: A tenant can only be in a group once
            builder.HasIndex(e => new { e.TenantGroupId, e.TenantId })
                .IsUnique()
                .HasDatabaseName("UQ_GroupMember_Group_Tenant");

            // Indexes
            builder.HasIndex(e => e.TenantGroupId)
                .HasDatabaseName("IX_GroupMembers_Group");

            builder.HasIndex(e => e.TenantId)
                .HasDatabaseName("IX_GroupMembers_Tenant");

            // Relationships
            builder.HasOne(e => e.TenantGroup)
                .WithMany(g => g.Members)
                .HasForeignKey(e => e.TenantGroupId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_GroupMember_Group");

            builder.HasOne(e => e.Tenant)
                .WithMany(t => t.GroupMemberships)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_GroupMember_Tenant");

            builder.HasOne(e => e.AddedByUser)
                .WithMany()
                .HasForeignKey(e => e.AddedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_GroupMember_AddedBy");
        }
    }
}
