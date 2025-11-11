using FormReporting.Models.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Notifications
{
    public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
    {
        public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
        {
            // Primary Key
            builder.HasKey(nt => nt.TemplateId);

            // Relationships
            builder.HasOne(nt => nt.Creator)
                .WithMany()
                .HasForeignKey(nt => nt.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(nt => nt.Modifier)
                .WithMany()
                .HasForeignKey(nt => nt.ModifiedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes
            builder.HasIndex(nt => nt.TemplateCode)
                .IsUnique()
                .HasDatabaseName("UQ_Template_Code");

            builder.HasIndex(nt => new { nt.Category, nt.IsActive })
                .HasDatabaseName("IX_Templates_Category");

            builder.HasIndex(nt => nt.IsActive)
                .HasDatabaseName("IX_Templates_Active");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Template_Priority",
                "[DefaultPriority] IN ('Low', 'Normal', 'High', 'Urgent')"
            ));
        }
    }
}
