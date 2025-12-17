using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class SubmissionWorkflowProgressConfiguration : IEntityTypeConfiguration<SubmissionWorkflowProgress>
    {
        public void Configure(EntityTypeBuilder<SubmissionWorkflowProgress> builder)
        {
            // Primary Key
            builder.HasKey(swp => swp.ProgressId);

            // Unique Constraints
            builder.HasIndex(swp => new { swp.SubmissionId, swp.StepId })
                .IsUnique()
                .HasDatabaseName("UQ_Progress_Submission_Step");

            // Indexes
            builder.HasIndex(swp => new { swp.SubmissionId, swp.StepOrder })
                .HasDatabaseName("IX_Progress_Submission_Order");

            builder.HasIndex(swp => swp.Status)
                .HasDatabaseName("IX_Progress_Status");

            builder.HasIndex(swp => swp.DueDate)
                .HasFilter("Status = 'Pending'")
                .HasDatabaseName("IX_Progress_DueDate");

            builder.HasIndex(swp => swp.DelegatedTo)
                .HasFilter("DelegatedTo IS NOT NULL")
                .HasDatabaseName("IX_Progress_Delegated");

            // Indexes for new fields
            builder.HasIndex(swp => swp.ActionId)
                .HasDatabaseName("IX_Progress_Action");

            builder.HasIndex(swp => swp.TargetType)
                .HasDatabaseName("IX_Progress_TargetType");

            builder.HasIndex(swp => swp.AssignedTo)
                .HasDatabaseName("IX_Progress_AssignedTo");

            builder.HasIndex(swp => new { swp.AssignedTo, swp.Status })
                .HasDatabaseName("IX_Progress_AssignedTo_Status");

            // Default Values
            builder.Property(swp => swp.Status).HasDefaultValue("Pending");
            builder.Property(swp => swp.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(swp => swp.Submission)
                .WithMany(fts => fts.WorkflowProgress)
                .HasForeignKey(swp => swp.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(swp => swp.Step)
                .WithMany(ws => ws.ProgressRecords)
                .HasForeignKey(swp => swp.StepId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(swp => swp.Reviewer)
                .WithMany()
                .HasForeignKey(swp => swp.ReviewedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(swp => swp.DelegatedToUser)
                .WithMany()
                .HasForeignKey(swp => swp.DelegatedTo)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(swp => swp.DelegatedByUser)
                .WithMany()
                .HasForeignKey(swp => swp.DelegatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // New relationships for enhanced workflow
            builder.HasOne(swp => swp.Action)
                .WithMany()
                .HasForeignKey(swp => swp.ActionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(swp => swp.AssignedToUser)
                .WithMany()
                .HasForeignKey(swp => swp.AssignedTo)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
