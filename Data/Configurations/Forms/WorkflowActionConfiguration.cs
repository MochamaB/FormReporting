using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class WorkflowActionConfiguration : IEntityTypeConfiguration<WorkflowAction>
    {
        public void Configure(EntityTypeBuilder<WorkflowAction> builder)
        {
            // Primary Key
            builder.HasKey(wa => wa.ActionId);

            // Unique Constraints
            builder.HasIndex(wa => wa.ActionCode)
                .IsUnique()
                .HasDatabaseName("UQ_WorkflowAction_Code");

            // Indexes
            builder.HasIndex(wa => wa.IsActive)
                .HasDatabaseName("IX_WorkflowAction_Active");

            builder.HasIndex(wa => wa.DisplayOrder)
                .HasDatabaseName("IX_WorkflowAction_DisplayOrder");

            // Default Values
            builder.Property(wa => wa.RequiresSignature).HasDefaultValue(false);
            builder.Property(wa => wa.RequiresComment).HasDefaultValue(false);
            builder.Property(wa => wa.AllowDelegate).HasDefaultValue(true);
            builder.Property(wa => wa.IsActive).HasDefaultValue(true);
            builder.Property(wa => wa.DisplayOrder).HasDefaultValue(0);

            // Relationships
            builder.HasMany(wa => wa.WorkflowSteps)
                .WithOne(ws => ws.Action)
                .HasForeignKey(ws => ws.ActionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
