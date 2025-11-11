using FormReporting.Models.Entities.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Media
{
    public class FileAccessLogConfiguration : IEntityTypeConfiguration<FileAccessLog>
    {
        public void Configure(EntityTypeBuilder<FileAccessLog> builder)
        {
            builder.HasKey(fal => fal.AccessLogId);

            builder.HasOne(fal => fal.File)
                .WithMany(f => f.AccessLogs)
                .HasForeignKey(fal => fal.FileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(fal => fal.Accessor)
                .WithMany()
                .HasForeignKey(fal => fal.AccessedBy)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(fal => new { fal.FileId, fal.AccessDate })
                .HasDatabaseName("IX_FileAccess_File")
                .IsDescending(false, true);

            builder.HasIndex(fal => new { fal.AccessedBy, fal.AccessDate })
                .HasDatabaseName("IX_FileAccess_User")
                .IsDescending(false, true);

            builder.HasIndex(fal => fal.AccessDate)
                .HasDatabaseName("IX_FileAccess_Date")
                .IsDescending();

            builder.HasIndex(fal => new { fal.AccessType, fal.AccessDate })
                .HasDatabaseName("IX_FileAccess_Type")
                .IsDescending(false, true);

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_FileAccess_Type",
                "[AccessType] IN ('View', 'Download', 'Delete', 'Update', 'Share', 'Scan')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_FileAccess_Result",
                "[AccessResult] IN ('Success', 'Denied', 'NotFound', 'Error')"
            ));
        }
    }
}
