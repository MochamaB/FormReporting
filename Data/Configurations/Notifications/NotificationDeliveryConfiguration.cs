using FormReporting.Models.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Notifications
{
    public class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
    {
        public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
        {
            // Primary Key
            builder.HasKey(nd => nd.DeliveryId);

            // Relationships
            builder.HasOne(nd => nd.Notification)
                .WithMany(n => n.Deliveries)
                .HasForeignKey(nd => nd.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(nd => nd.RecipientUser)
                .WithMany()
                .HasForeignKey(nd => nd.RecipientUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(nd => nd.Channel)
                .WithMany(c => c.Deliveries)
                .HasForeignKey(nd => nd.ChannelId)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes
            builder.HasIndex(nd => new { nd.NotificationId, nd.ChannelId })
                .HasDatabaseName("IX_Delivery_Notification_Channel");

            builder.HasIndex(nd => new { nd.Status, nd.NextRetryDate })
                .HasDatabaseName("IX_Delivery_Retry")
                .HasFilter("[Status] = 'Pending' AND [NextRetryDate] IS NOT NULL");

            builder.HasIndex(nd => new { nd.ChannelId, nd.Status, nd.CreatedDate })
                .HasDatabaseName("IX_Delivery_Channel_Status");

            builder.HasIndex(nd => nd.ExternalMessageId)
                .HasDatabaseName("IX_Delivery_ExternalId")
                .HasFilter("[ExternalMessageId] IS NOT NULL");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Delivery_Status",
                "[Status] IN ('Pending', 'Sent', 'Delivered', 'Failed', 'Bounced', 'Cancelled')"
            ));
        }
    }
}
