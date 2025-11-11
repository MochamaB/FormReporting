using FormReporting.Models.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Notifications
{
    public class AlertHistoryConfiguration : IEntityTypeConfiguration<AlertHistory>
    {
        public void Configure(EntityTypeBuilder<AlertHistory> builder)
        {
            // Primary Key
            builder.HasKey(ah => ah.HistoryId);

            // Relationships
            builder.HasOne(ah => ah.Alert)
                .WithMany(a => a.AlertHistories)
                .HasForeignKey(ah => ah.AlertId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ah => ah.Notification)
                .WithMany()
                .HasForeignKey(ah => ah.NotificationId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(ah => ah.Acknowledger)
                .WithMany()
                .HasForeignKey(ah => ah.AcknowledgedBy)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(ah => ah.Resolver)
                .WithMany()
                .HasForeignKey(ah => ah.ResolvedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes
            builder.HasIndex(ah => new { ah.AlertId, ah.TriggeredDate })
                .HasDatabaseName("IX_AlertHistory_Alert_Date")
                .IsDescending(false, true);

            builder.HasIndex(ah => new { ah.Status, ah.TriggeredDate })
                .HasDatabaseName("IX_AlertHistory_Status");

            builder.HasIndex(ah => ah.TriggeredDate)
                .HasDatabaseName("IX_AlertHistory_Date")
                .IsDescending();

            builder.HasIndex(ah => ah.IsEscalated)
                .HasDatabaseName("IX_AlertHistory_Escalated")
                .HasFilter("[IsEscalated] = 1");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_AlertHistory_Status",
                "[Status] IN ('Triggered', 'Acknowledged', 'Resolved', 'AutoResolved', 'Cancelled')"
            ));
        }
    }
}
