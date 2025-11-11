using FormReporting.Models.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Notifications
{
    public class NotificationRecipientConfiguration : IEntityTypeConfiguration<NotificationRecipient>
    {
        public void Configure(EntityTypeBuilder<NotificationRecipient> builder)
        {
            // Primary Key
            builder.HasKey(nr => nr.RecipientId);

            // Relationships
            builder.HasOne(nr => nr.Notification)
                .WithMany(n => n.Recipients)
                .HasForeignKey(nr => nr.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(nr => nr.User)
                .WithMany()
                .HasForeignKey(nr => nr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(nr => new { nr.NotificationId, nr.UserId })
                .IsUnique()
                .HasDatabaseName("UQ_NotificationRecipient");

            builder.HasIndex(nr => new { nr.UserId, nr.IsRead, nr.CreatedDate })
                .HasDatabaseName("IX_Recipients_User_Unread");

            builder.HasIndex(nr => new { nr.UserId, nr.CreatedDate })
                .HasDatabaseName("IX_Recipients_User_Date")
                .IsDescending(false, true);
        }
    }
}
