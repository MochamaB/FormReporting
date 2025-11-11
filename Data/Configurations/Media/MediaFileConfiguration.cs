using FormReporting.Models.Entities.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Media
{
    public class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
    {
        public void Configure(EntityTypeBuilder<MediaFile> builder)
        {
            builder.HasKey(mf => mf.FileId);

            builder.HasOne(mf => mf.Uploader)
                .WithMany()
                .HasForeignKey(mf => mf.UploadedBy)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(mf => mf.Deleter)
                .WithMany()
                .HasForeignKey(mf => mf.DeletedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            builder.HasIndex(mf => mf.FileHash)
                .HasDatabaseName("IX_MediaFiles_Hash")
                .HasFilter("[FileHash] IS NOT NULL AND [IsDeleted] = 0");

            builder.HasIndex(mf => new { mf.UploadedBy, mf.UploadedDate })
                .HasDatabaseName("IX_MediaFiles_Uploader")
                .IsDescending(false, true);

            builder.HasIndex(mf => new { mf.MimeType, mf.IsDeleted })
                .HasDatabaseName("IX_MediaFiles_Type")
                .HasFilter("[IsDeleted] = 0");

            builder.HasIndex(mf => new { mf.StorageProvider, mf.IsDeleted })
                .HasDatabaseName("IX_MediaFiles_Storage");

            builder.HasIndex(mf => mf.ExpiryDate)
                .HasDatabaseName("IX_MediaFiles_Expiry")
                .HasFilter("[ExpiryDate] IS NOT NULL AND [IsDeleted] = 0");

            builder.HasIndex(mf => mf.IsImage)
                .HasDatabaseName("IX_MediaFiles_Images")
                .HasFilter("[IsImage] = 1 AND [IsDeleted] = 0");

            builder.HasIndex(mf => new { mf.IsVirusSafe, mf.VirusScanDate })
                .HasDatabaseName("IX_MediaFiles_VirusScan")
                .HasFilter("[IsDeleted] = 0");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_MediaFile_Provider",
                "[StorageProvider] IN ('Local', 'Azure', 'AWS', 'OneDrive', 'SharePoint', 'GoogleDrive')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_MediaFile_Access",
                "[AccessLevel] IN ('Public', 'Private', 'Internal', 'Confidential', 'Restricted')"
            ));
        }
    }
}
