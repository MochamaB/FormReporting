using FormReporting.Models.Entities.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Reporting
{
    public class ReportAccessControlConfiguration : IEntityTypeConfiguration<ReportAccessControl>
    {
        public void Configure(EntityTypeBuilder<ReportAccessControl> builder)
        {
            builder.HasKey(rac => rac.AccessId);

            builder.HasOne(rac => rac.Report)
                .WithMany(r => r.AccessControls)
                .HasForeignKey(rac => rac.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rac => rac.User)
                .WithMany()
                .HasForeignKey(rac => rac.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(rac => rac.Role)
                .WithMany()
                .HasForeignKey(rac => rac.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(rac => rac.Department)
                .WithMany()
                .HasForeignKey(rac => rac.DepartmentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(rac => rac.Grantor)
                .WithMany()
                .HasForeignKey(rac => rac.GrantedBy)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(rac => new { rac.ReportId, rac.IsActive })
                .HasDatabaseName("IX_ReportAccess_Report");

            builder.HasIndex(rac => new { rac.UserId, rac.IsActive })
                .HasDatabaseName("IX_ReportAccess_User")
                .HasFilter("[UserId] IS NOT NULL");

            builder.HasIndex(rac => new { rac.RoleId, rac.IsActive })
                .HasDatabaseName("IX_ReportAccess_Role")
                .HasFilter("[RoleId] IS NOT NULL");

            builder.HasIndex(rac => rac.ExpiryDate)
                .HasDatabaseName("IX_ReportAccess_Expiry")
                .HasFilter("[ExpiryDate] IS NOT NULL");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ReportAccess_Type",
                "[AccessType] IN ('User', 'Role', 'Department', 'Everyone')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ReportAccess_Permission",
                "[PermissionLevel] IN ('View', 'Run', 'Edit', 'Delete', 'Share', 'Admin')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ReportAccess_Target",
                @"([AccessType] = 'User' AND [UserId] IS NOT NULL AND [RoleId] IS NULL AND [DepartmentId] IS NULL) OR
                  ([AccessType] = 'Role' AND [RoleId] IS NOT NULL AND [UserId] IS NULL AND [DepartmentId] IS NULL) OR
                  ([AccessType] = 'Department' AND [DepartmentId] IS NOT NULL AND [UserId] IS NULL AND [RoleId] IS NULL) OR
                  ([AccessType] = 'Everyone' AND [UserId] IS NULL AND [RoleId] IS NULL AND [DepartmentId] IS NULL)"
            ));
        }
    }
}
