using FormReporting.Models.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Notifications
{
    public class UserNotificationPreferenceConfiguration : IEntityTypeConfiguration<UserNotificationPreference>
    {
        public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
        {
            // Primary Key
            builder.HasKey(unp => unp.PreferenceId);

            // Relationships
            builder.HasOne(unp => unp.User)
                .WithMany()
                .HasForeignKey(unp => unp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(unp => unp.Channel)
                .WithMany(c => c.UserPreferences)
                .HasForeignKey(unp => unp.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(unp => new { unp.UserId, unp.ChannelId, unp.NotificationType })
                .IsUnique()
                .HasDatabaseName("UQ_UserPreference");

            builder.HasIndex(unp => new { unp.UserId, unp.IsEnabled })
                .HasDatabaseName("IX_Preferences_User");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Preference_Frequency",
                "[Frequency] IN ('Immediate', 'Hourly', 'Daily', 'Weekly', 'Never')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Preference_Priority",
                "[MinimumPriority] IN ('Low', 'Normal', 'High', 'Urgent')"
            ));
        }
    }
}
