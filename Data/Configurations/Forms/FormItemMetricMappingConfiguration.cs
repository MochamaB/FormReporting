using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class FormItemMetricMappingConfiguration : IEntityTypeConfiguration<FormItemMetricMapping>
    {
        public void Configure(EntityTypeBuilder<FormItemMetricMapping> builder)
        {
            // Primary Key
            builder.HasKey(fimm => fimm.MappingId);

            // Unique Constraints
            builder.HasIndex(fimm => new { fimm.ItemId, fimm.MetricId })
                .IsUnique()
                .HasDatabaseName("UQ_ItemMetricMap");

            // Indexes
            builder.HasIndex(fimm => new { fimm.ItemId, fimm.IsActive })
                .HasDatabaseName("IX_ItemMetricMap_Item");

            builder.HasIndex(fimm => new { fimm.MetricId, fimm.IsActive })
                .HasDatabaseName("IX_ItemMetricMap_Metric");

            builder.HasIndex(fimm => new { fimm.MappingType, fimm.IsActive })
                .HasDatabaseName("IX_ItemMetricMap_Type");

            // Default Values
            builder.Property(fimm => fimm.IsActive).HasDefaultValue(true);
            builder.Property(fimm => fimm.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(fimm => fimm.Item)
                .WithMany(fti => fti.MetricMappings)
                .HasForeignKey(fimm => fimm.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(fimm => fimm.Metric)
                .WithMany(md => md.FormItemMetricMappings)
                .HasForeignKey(fimm => fimm.MetricId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(fimm => fimm.PopulationLogs)
                .WithOne(mpl => mpl.Mapping)
                .HasForeignKey(mpl => mpl.MappingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
