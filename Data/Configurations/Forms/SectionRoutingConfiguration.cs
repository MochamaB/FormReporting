using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class SectionRoutingConfiguration : IEntityTypeConfiguration<SectionRouting>
    {
        public void Configure(EntityTypeBuilder<SectionRouting> builder)
        {
            // Primary Key
            builder.HasKey(sr => sr.RoutingId);

            // Indexes
            builder.HasIndex(sr => new { sr.SourceSectionId, sr.IsActive })
                .HasDatabaseName("IX_SectionRouting_Source");

            builder.HasIndex(sr => new { sr.SourceItemId, sr.IsActive })
                .HasDatabaseName("IX_SectionRouting_Item");

            builder.HasIndex(sr => sr.TargetSectionId)
                .HasDatabaseName("IX_SectionRouting_Target");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Routing_Condition",
                "ConditionType IN ('equals', 'not_equals', 'contains', 'greater_than', 'less_than', 'is_empty')"
            ));

            // Default Values
            builder.Property(sr => sr.IsActive).HasDefaultValue(true);

            // Relationships
            builder.HasOne(sr => sr.SourceSection)
                .WithMany(fts => fts.SourceRoutings)
                .HasForeignKey(sr => sr.SourceSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.SourceItem)
                .WithMany(fti => fti.Routings)
                .HasForeignKey(sr => sr.SourceItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.TargetSection)
                .WithMany(fts => fts.TargetRoutings)
                .HasForeignKey(sr => sr.TargetSectionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
