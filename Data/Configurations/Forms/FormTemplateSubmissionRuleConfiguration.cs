using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FormReporting.Models.Entities.Forms;

namespace FormReporting.Data.Configurations.Forms
{
    public class FormTemplateSubmissionRuleConfiguration : IEntityTypeConfiguration<FormTemplateSubmissionRule>
    {
        public void Configure(EntityTypeBuilder<FormTemplateSubmissionRule> builder)
        {
            builder.ToTable("FormTemplateSubmissionRules");

            builder.HasKey(r => r.SubmissionRuleId);

            // Required fields
            builder.Property(r => r.RuleName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.Description)
                .HasMaxLength(500);

            builder.Property(r => r.Frequency)
                .HasMaxLength(20);

            builder.Property(r => r.ReminderDaysBefore)
                .HasMaxLength(50);

            builder.Property(r => r.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            // Indexes
            builder.HasIndex(r => r.TemplateId);
            builder.HasIndex(r => r.Status);
            builder.HasIndex(r => new { r.TemplateId, r.Status });

            // Relationships
            builder.HasOne(r => r.Template)
                .WithMany(t => t.SubmissionRules)
                .HasForeignKey(r => r.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.CreatedByUser)
                .WithMany()
                .HasForeignKey(r => r.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.ModifiedByUser)
                .WithMany()
                .HasForeignKey(r => r.ModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
