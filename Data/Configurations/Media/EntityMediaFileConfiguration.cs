using FormReporting.Models.Entities.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Media
{
    public class EntityMediaFileConfiguration : IEntityTypeConfiguration<EntityMediaFile>
    {
        public void Configure(EntityTypeBuilder<EntityMediaFile> builder)
        {
            builder.HasKey(emf => emf.EntityMediaId);

            builder.HasOne(emf => emf.File)
                .WithMany(f => f.EntityMediaFiles)
                .HasForeignKey(emf => emf.FileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(emf => emf.Attacher)
                .WithMany()
                .HasForeignKey(emf => emf.AttachedBy)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(emf => new { emf.EntityType, emf.EntityId, emf.IsActive })
                .HasDatabaseName("IX_EntityMedia_Entity");

            builder.HasIndex(emf => new { emf.FileId, emf.IsActive })
                .HasDatabaseName("IX_EntityMedia_File");

            builder.HasIndex(emf => new { emf.AttachmentType, emf.EntityType })
                .HasDatabaseName("IX_EntityMedia_Type");

            builder.HasIndex(emf => emf.ResponseId)
                .HasDatabaseName("IX_EntityMedia_Response")
                .HasFilter("[ResponseId] IS NOT NULL");

            builder.HasIndex(emf => new { emf.EntityType, emf.EntityId, emf.IsPrimary })
                .HasDatabaseName("IX_EntityMedia_Primary")
                .HasFilter("[IsPrimary] = 1 AND [IsActive] = 1");

            builder.HasIndex(emf => new { emf.EntityType, emf.EntityId })
                .IsUnique()
                .HasDatabaseName("UQ_EntityMedia_OnePrimary")
                .HasFilter("[IsPrimary] = 1 AND [IsActive] = 1");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_EntityMedia_Type",
                "[EntityType] IN ('Expense', 'Budget', 'Ticket', 'FormResponse', 'FormSubmission', 'Hardware', 'Software', 'User', 'Tenant', 'Training', 'Project', 'Maintenance', 'Audit', 'Report', 'Other')"
            ));
        }
    }
}
