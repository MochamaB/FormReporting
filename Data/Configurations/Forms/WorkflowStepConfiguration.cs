using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
    {
        public void Configure(EntityTypeBuilder<WorkflowStep> builder)
        {
            // Primary Key
            builder.HasKey(ws => ws.StepId);

            // Unique Constraints
            builder.HasIndex(ws => new { ws.WorkflowId, ws.StepOrder })
                .IsUnique()
                .HasDatabaseName("UQ_WorkflowStep_Order");

            // Indexes
            builder.HasIndex(ws => new { ws.WorkflowId, ws.StepOrder })
                .HasDatabaseName("IX_WorkflowStep_Workflow");

            builder.HasIndex(ws => ws.EscalationRoleId)
                .HasDatabaseName("IX_WorkflowStep_Escalation");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_WorkflowStep_Approver",
                "(ApproverRoleId IS NOT NULL AND ApproverUserId IS NULL) OR (ApproverRoleId IS NULL AND ApproverUserId IS NOT NULL)"
            ));

            // Default Values
            builder.Property(ws => ws.IsMandatory).HasDefaultValue(true);
            builder.Property(ws => ws.IsParallel).HasDefaultValue(false);
            builder.Property(ws => ws.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(ws => ws.Workflow)
                .WithMany(wd => wd.Steps)
                .HasForeignKey(ws => ws.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ws => ws.ApproverRole)
                .WithMany()
                .HasForeignKey(ws => ws.ApproverRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ws => ws.ApproverUser)
                .WithMany()
                .HasForeignKey(ws => ws.ApproverUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ws => ws.EscalationRole)
                .WithMany()
                .HasForeignKey(ws => ws.EscalationRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(ws => ws.ProgressRecords)
                .WithOne(swp => swp.Step)
                .HasForeignKey(swp => swp.StepId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
