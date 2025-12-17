using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class FormTemplateAssignmentConfiguration : IEntityTypeConfiguration<FormTemplateAssignment>
    {
        public void Configure(EntityTypeBuilder<FormTemplateAssignment> builder)
        {
            // Primary Key
            builder.HasKey(fta => fta.AssignmentId);

            // Indexes
            builder.HasIndex(fta => new { fta.TemplateId, fta.Status })
                .HasDatabaseName("IX_TemplateAssignments_Template");

            builder.HasIndex(fta => new { fta.AssignmentType, fta.Status })
                .HasDatabaseName("IX_TemplateAssignments_Type");

            builder.HasIndex(fta => new { fta.TenantType, fta.Status })
                .HasDatabaseName("IX_TemplateAssignments_TenantType");

            builder.HasIndex(fta => new { fta.TenantGroupId, fta.Status })
                .HasDatabaseName("IX_TemplateAssignments_TenantGroup");

            builder.HasIndex(fta => new { fta.TenantId, fta.Status })
                .HasDatabaseName("IX_TemplateAssignments_Tenant");

            builder.HasIndex(fta => fta.RoleId)
                .HasDatabaseName("IX_TemplateAssignment_Role");

            // Indexes for access fields
            builder.HasIndex(fta => fta.Status)
                .HasDatabaseName("IX_TemplateAssignment_Status");

            builder.HasIndex(fta => new { fta.EffectiveFrom, fta.EffectiveUntil })
                .HasDatabaseName("IX_TemplateAssignment_EffectivePeriod");

            builder.HasIndex(fta => new { fta.TemplateId, fta.Status, fta.EffectiveFrom })
                .HasDatabaseName("IX_TemplateAssignment_Template_Status_Effective");

            builder.HasIndex(fta => fta.AllowAnonymous)
                .HasFilter("AllowAnonymous = 1")
                .HasDatabaseName("IX_TemplateAssignment_Anonymous");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_TemplateAssignment_Type",
                "AssignmentType IN ('All', 'TenantType', 'TenantGroup', 'SpecificTenant', 'Role', 'Department', 'UserGroup', 'SpecificUser')"
            ));

            // Complex check constraint for assignment target validation
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_TemplateAssignment_Target",
                @"(AssignmentType = 'All' AND TenantType IS NULL AND TenantGroupId IS NULL AND TenantId IS NULL AND RoleId IS NULL AND DepartmentId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR
                  (AssignmentType = 'TenantType' AND TenantType IS NOT NULL AND TenantGroupId IS NULL AND TenantId IS NULL AND RoleId IS NULL AND DepartmentId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR
                  (AssignmentType = 'TenantGroup' AND TenantGroupId IS NOT NULL AND TenantType IS NULL AND TenantId IS NULL AND RoleId IS NULL AND DepartmentId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR
                  (AssignmentType = 'SpecificTenant' AND TenantId IS NOT NULL AND TenantType IS NULL AND TenantGroupId IS NULL AND RoleId IS NULL AND DepartmentId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR
                  (AssignmentType = 'Role' AND RoleId IS NOT NULL AND TenantType IS NULL AND TenantGroupId IS NULL AND TenantId IS NULL AND DepartmentId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR
                  (AssignmentType = 'Department' AND DepartmentId IS NOT NULL AND TenantType IS NULL AND TenantGroupId IS NULL AND TenantId IS NULL AND RoleId IS NULL AND UserGroupId IS NULL AND UserId IS NULL) OR
                  (AssignmentType = 'UserGroup' AND UserGroupId IS NOT NULL AND TenantType IS NULL AND TenantGroupId IS NULL AND TenantId IS NULL AND RoleId IS NULL AND DepartmentId IS NULL AND UserId IS NULL) OR
                  (AssignmentType = 'SpecificUser' AND UserId IS NOT NULL AND TenantType IS NULL AND TenantGroupId IS NULL AND TenantId IS NULL AND RoleId IS NULL AND DepartmentId IS NULL AND UserGroupId IS NULL)"
            ));

            // Check constraint for status values
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_TemplateAssignment_Status",
                "Status IN ('Active', 'Suspended', 'Revoked')"
            ));

            // Default Values
            builder.Property(fta => fta.AssignedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(fta => fta.EffectiveFrom).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(fta => fta.Status).HasDefaultValue("Active");
            builder.Property(fta => fta.AllowAnonymous).HasDefaultValue(false);

            // Relationships
            builder.HasOne(fta => fta.Template)
                .WithMany(ft => ft.Assignments)
                .HasForeignKey(fta => fta.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(fta => fta.TenantGroup)
                .WithMany()
                .HasForeignKey(fta => fta.TenantGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(fta => fta.Tenant)
                .WithMany()
                .HasForeignKey(fta => fta.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(fta => fta.Role)
                .WithMany()
                .HasForeignKey(fta => fta.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(fta => fta.Department)
                .WithMany()
                .HasForeignKey(fta => fta.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(fta => fta.UserGroup)
                .WithMany()
                .HasForeignKey(fta => fta.UserGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(fta => fta.User)
                .WithMany()
                .HasForeignKey(fta => fta.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(fta => fta.AssignedByUser)
                .WithMany()
                .HasForeignKey(fta => fta.AssignedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(fta => fta.CancelledByUser)
                .WithMany()
                .HasForeignKey(fta => fta.CancelledBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
