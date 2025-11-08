using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class FormTemplateSubmissionConfiguration : IEntityTypeConfiguration<FormTemplateSubmission>
    {
        public void Configure(EntityTypeBuilder<FormTemplateSubmission> builder)
        {
            // Primary Key
            builder.HasKey(fts => fts.SubmissionId);

            // Indexes
            builder.HasIndex(fts => new { fts.ReportingPeriod, fts.TenantId, fts.TemplateId })
                .IsDescending(true, false, false)
                .IncludeProperties(fts => new { fts.Status, fts.SubmittedBy })
                .HasDatabaseName("IX_TemplateSubmissions_TimeSeries");

            builder.HasIndex(fts => new { fts.Status, fts.TenantId })
                .HasDatabaseName("IX_TemplateSubmissions_Status");

            builder.HasIndex(fts => new { fts.TenantId, fts.ReportingPeriod })
                .IsDescending(false, true)
                .HasFilter("Status IN ('Submitted', 'Approved')")
                .HasDatabaseName("IX_TemplateSubmissions_Tenant_Recent");

            builder.HasIndex(fts => new { fts.SubmittedBy, fts.ReportingPeriod })
                .HasDatabaseName("IX_Submission_User_Period");

            // Filtered unique indexes for location-based vs user-based forms
            builder.HasIndex(fts => new { fts.TenantId, fts.TemplateId, fts.ReportingPeriod })
                .IsUnique()
                .HasFilter("TenantId IS NOT NULL AND Status <> 'Draft'")
                .HasDatabaseName("IX_Submission_Location_Unique");

            builder.HasIndex(fts => new { fts.SubmittedBy, fts.TemplateId, fts.ReportingPeriod })
                .IsUnique()
                .HasFilter("TenantId IS NULL AND Status <> 'Draft'")
                .HasDatabaseName("IX_Submission_User_Unique");

            // Default Values
            builder.Property(fts => fts.Status).HasDefaultValue("Draft");
            builder.Property(fts => fts.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(fts => fts.ModifiedDate).HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(fts => fts.Template)
                .WithMany(ft => ft.Submissions)
                .HasForeignKey(fts => fts.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(fts => fts.Tenant)
                .WithMany()
                .HasForeignKey(fts => fts.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(fts => fts.Submitter)
                .WithMany()
                .HasForeignKey(fts => fts.SubmittedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(fts => fts.Reviewer)
                .WithMany()
                .HasForeignKey(fts => fts.ReviewedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(fts => fts.Modifier)
                .WithMany()
                .HasForeignKey(fts => fts.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(fts => fts.Responses)
                .WithOne(ftr => ftr.Submission)
                .HasForeignKey(ftr => ftr.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(fts => fts.WorkflowProgress)
                .WithOne(swp => swp.Submission)
                .HasForeignKey(swp => swp.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(fts => fts.MetricPopulationLogs)
                .WithOne(mpl => mpl.Submission)
                .HasForeignKey(mpl => mpl.SubmissionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(fts => fts.Analytics)
                .WithOne(fa => fa.Submission)
                .HasForeignKey(fa => fa.SubmissionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
