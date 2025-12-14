using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class FormTemplateResponseConfiguration : IEntityTypeConfiguration<FormTemplateResponse>
    {
        public void Configure(EntityTypeBuilder<FormTemplateResponse> builder)
        {
            // Primary Key
            builder.HasKey(ftr => ftr.ResponseId);

            // Unique Constraints
            builder.HasIndex(ftr => new { ftr.SubmissionId, ftr.ItemId })
                .IsUnique()
                .HasDatabaseName("UQ_SubmissionItem");

            // Indexes
            builder.HasIndex(ftr => ftr.SubmissionId)
                .HasDatabaseName("IX_TemplateResponses_Submission");

            // Default Values
            builder.Property(ftr => ftr.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(ftr => ftr.ModifiedDate).HasDefaultValueSql("GETDATE()");

            // Scoring Fields - Precision configuration
            builder.Property(ftr => ftr.SelectedScoreValue)
                .HasColumnType("decimal(10,2)");

            builder.Property(ftr => ftr.SelectedScoreWeight)
                .HasColumnType("decimal(10,2)");

            builder.Property(ftr => ftr.WeightedScore)
                .HasColumnType("decimal(10,2)");

            // Relationships
            builder.HasOne(ftr => ftr.Submission)
                .WithMany(fts => fts.Responses)
                .HasForeignKey(ftr => ftr.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ftr => ftr.Item)
                .WithMany(fti => fti.Responses)
                .HasForeignKey(ftr => ftr.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ftr => ftr.SelectedOption)
                .WithMany()
                .HasForeignKey(ftr => ftr.SelectedOptionId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        }
    }
}
