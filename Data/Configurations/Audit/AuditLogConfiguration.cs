using FormReporting.Models.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Audit
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(al => al.AuditId);

            builder.HasOne(al => al.Changer)
                .WithMany()
                .HasForeignKey(al => al.ChangedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            builder.HasIndex(al => new { al.TableName, al.RecordId, al.ChangedDate })
                .HasDatabaseName("IX_AuditLogs_Table")
                .IsDescending(false, false, true);

            builder.HasIndex(al => new { al.ChangedBy, al.ChangedDate })
                .HasDatabaseName("IX_AuditLogs_User")
                .IsDescending(false, true);

            builder.HasIndex(al => al.ChangedDate)
                .HasDatabaseName("IX_AuditLogs_Date")
                .IsDescending();
        }
    }
}
