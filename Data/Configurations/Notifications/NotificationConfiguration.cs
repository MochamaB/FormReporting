using FormReporting.Models.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Notifications
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            // Primary Key
            builder.HasKey(n => n.NotificationId);

            // Relationships
            builder.HasOne(n => n.Template)
                .WithMany(t => t.Notifications)
                .HasForeignKey(n => n.TemplateId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(n => n.Trigger)
                .WithMany()
                .HasForeignKey(n => n.TriggeredBy)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes
            builder.HasIndex(n => new { n.NotificationType, n.CreatedDate })
                .HasDatabaseName("IX_Notifications_Type_Date");

            builder.HasIndex(n => new { n.Priority, n.CreatedDate })
                .HasDatabaseName("IX_Notifications_Priority_Date");

            builder.HasIndex(n => n.ScheduledDate)
                .HasDatabaseName("IX_Notifications_Scheduled")
                .HasFilter("[ScheduledDate] IS NOT NULL AND [IsActive] = 1");

            builder.HasIndex(n => n.ExpiryDate)
                .HasDatabaseName("IX_Notifications_Expiry")
                .HasFilter("[ExpiryDate] IS NOT NULL");

            builder.HasIndex(n => new { n.SourceEntityType, n.SourceEntityId })
                .HasDatabaseName("IX_Notifications_Source");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Notification_Priority",
                "[Priority] IN ('Low', 'Normal', 'High', 'Urgent')"
            ));
        }
    }
}
