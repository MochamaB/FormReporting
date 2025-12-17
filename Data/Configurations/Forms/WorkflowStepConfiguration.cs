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

            // Indexes for new fields
            builder.HasIndex(ws => ws.ActionId)
                .HasDatabaseName("IX_WorkflowStep_Action");

            builder.HasIndex(ws => ws.TargetType)
                .HasDatabaseName("IX_WorkflowStep_TargetType");

            builder.HasIndex(ws => ws.AssigneeType)
                .HasDatabaseName("IX_WorkflowStep_AssigneeType");

            // Default Values
            builder.Property(ws => ws.IsMandatory).HasDefaultValue(true);
            builder.Property(ws => ws.IsParallel).HasDefaultValue(false);
            builder.Property(ws => ws.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(ws => ws.TargetType).HasDefaultValue("Submission");
            builder.Property(ws => ws.AssigneeType).HasDefaultValue("Role");

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

            // New relationships for enhanced workflow
            builder.HasOne(ws => ws.Action)
                .WithMany(wa => wa.WorkflowSteps)
                .HasForeignKey(ws => ws.ActionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ws => ws.AssigneeDepartment)
                .WithMany()
                .HasForeignKey(ws => ws.AssigneeDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ws => ws.AssigneeField)
                .WithMany()
                .HasForeignKey(ws => ws.AssigneeFieldId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(ws => ws.ProgressRecords)
                .WithOne(swp => swp.Step)
                .HasForeignKey(swp => swp.StepId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
