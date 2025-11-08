using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class FormItemCalculationConfiguration : IEntityTypeConfiguration<FormItemCalculation>
    {
        public void Configure(EntityTypeBuilder<FormItemCalculation> builder)
        {
            // Primary Key
            builder.HasKey(fic => fic.CalculationId);

            // Indexes
            builder.HasIndex(fic => new { fic.TargetItemId, fic.IsActive })
                .HasDatabaseName("IX_ItemCalculations_Target");

            // Default Values
            builder.Property(fic => fic.IsActive).HasDefaultValue(true);
            builder.Property(fic => fic.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(fic => fic.TargetItem)
                .WithMany(fti => fti.Calculations)
                .HasForeignKey(fic => fic.TargetItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
