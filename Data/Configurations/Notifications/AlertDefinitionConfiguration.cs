using FormReporting.Models.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Notifications
{
    public class AlertDefinitionConfiguration : IEntityTypeConfiguration<AlertDefinition>
    {
        public void Configure(EntityTypeBuilder<AlertDefinition> builder)
        {
            // Primary Key
            builder.HasKey(ad => ad.AlertId);

            // Relationships
            builder.HasOne(ad => ad.Template)
                .WithMany(t => t.AlertDefinitions)
                .HasForeignKey(ad => ad.TemplateId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(ad => ad.Creator)
                .WithMany()
                .HasForeignKey(ad => ad.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(ad => ad.Modifier)
                .WithMany()
                .HasForeignKey(ad => ad.ModifiedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes
            builder.HasIndex(ad => ad.AlertCode)
                .IsUnique()
                .HasDatabaseName("UQ_Alert_Code");

            builder.HasIndex(ad => new { ad.AlertType, ad.IsActive })
                .HasDatabaseName("IX_Alerts_Type");

            builder.HasIndex(ad => ad.IsActive)
                .HasDatabaseName("IX_Alerts_Active");

            builder.HasIndex(ad => ad.LastCheckDate)
                .HasDatabaseName("IX_Alerts_LastCheck")
                .HasFilter("[IsActive] = 1");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Alert_Severity",
                "[Severity] IN ('Info', 'Warning', 'Error', 'Critical')"
            ));
        }
    }
}
