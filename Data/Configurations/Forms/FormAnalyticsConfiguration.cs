using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class FormAnalyticsConfiguration : IEntityTypeConfiguration<FormAnalytics>
    {
        public void Configure(EntityTypeBuilder<FormAnalytics> builder)
        {
            // Primary Key
            builder.HasKey(fa => fa.AnalyticId);

            // Indexes
            builder.HasIndex(fa => new { fa.TemplateId, fa.EventDate })
                .IsDescending(false, true)
                .HasDatabaseName("IX_Analytics_Template_Date");

            builder.HasIndex(fa => new { fa.UserId, fa.EventDate })
                .IsDescending(false, true)
                .HasDatabaseName("IX_Analytics_User_Date");

            builder.HasIndex(fa => new { fa.SessionId, fa.EventDate })
                .HasDatabaseName("IX_Analytics_Session");

            builder.HasIndex(fa => new { fa.EventType, fa.EventDate })
                .IsDescending(false, true)
                .HasDatabaseName("IX_Analytics_EventType");

            builder.HasIndex(fa => fa.SubmissionId)
                .HasDatabaseName("IX_Analytics_Submission");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Analytics_EventType",
                "EventType IN ('FormOpened', 'SectionStarted', 'SectionCompleted', 'FieldFilled', 'FormAbandoned', 'FormSubmitted', 'FormSaved')"
            ));

            // Default Values
            builder.Property(fa => fa.EventDate).HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(fa => fa.Template)
                .WithMany(ft => ft.Analytics)
                .HasForeignKey(fa => fa.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(fa => fa.User)
                .WithMany()
                .HasForeignKey(fa => fa.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(fa => fa.Submission)
                .WithMany(fts => fts.Analytics)
                .HasForeignKey(fa => fa.SubmissionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
