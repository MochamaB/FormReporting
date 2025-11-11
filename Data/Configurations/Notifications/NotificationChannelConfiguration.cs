using FormReporting.Models.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Notifications
{
    public class NotificationChannelConfiguration : IEntityTypeConfiguration<NotificationChannel>
    {
        public void Configure(EntityTypeBuilder<NotificationChannel> builder)
        {
            // Primary Key
            builder.HasKey(nc => nc.ChannelId);

            // Indexes
            builder.HasIndex(nc => nc.ChannelType)
                .HasDatabaseName("IX_NotificationChannels_Type");

            builder.HasIndex(nc => nc.IsEnabled)
                .HasDatabaseName("IX_NotificationChannels_Enabled");

            builder.HasIndex(nc => nc.Provider)
                .HasDatabaseName("IX_NotificationChannels_Provider");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Channel_Type",
                "[ChannelType] IN ('Email', 'SMS', 'Push', 'InApp', 'Webhook')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Channel_Priority",
                "[Priority] BETWEEN 1 AND 5"
            ));
        }
    }
}
