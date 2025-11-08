using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
    {
        public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
        {
            // Primary Key
            builder.HasKey(wd => wd.WorkflowId);

            // Indexes
            builder.HasIndex(wd => wd.IsActive)
                .HasDatabaseName("IX_Workflow_Active");

            // Default Values
            builder.Property(wd => wd.IsActive).HasDefaultValue(true);
            builder.Property(wd => wd.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(wd => wd.ModifiedDate).HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(wd => wd.Creator)
                .WithMany()
                .HasForeignKey(wd => wd.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(wd => wd.Steps)
                .WithOne(ws => ws.Workflow)
                .HasForeignKey(ws => ws.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(wd => wd.FormTemplates)
                .WithOne(ft => ft.Workflow)
                .HasForeignKey(ft => ft.WorkflowId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
