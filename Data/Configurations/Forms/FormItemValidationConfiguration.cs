using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class FormItemValidationConfiguration : IEntityTypeConfiguration<FormItemValidation>
    {
        public void Configure(EntityTypeBuilder<FormItemValidation> builder)
        {
            // Primary Key
            builder.HasKey(fiv => fiv.ItemValidationId);

            // Indexes
            builder.HasIndex(fiv => new { fiv.ItemId, fiv.ValidationOrder, fiv.IsActive })
                .HasDatabaseName("IX_ItemValidations_Item");

            builder.HasIndex(fiv => new { fiv.ValidationType, fiv.IsActive })
                .HasDatabaseName("IX_ItemValidations_Type");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ValidationType",
                "ValidationType IN ('Required', 'Email', 'Phone', 'URL', 'Range', 'MinLength', 'MaxLength', 'Pattern', 'Custom', 'CrossField', 'Date', 'Number', 'Integer', 'Decimal')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ValidationSeverity",
                "Severity IN ('Error', 'Warning', 'Info')"
            ));

            // Default Values
            builder.Property(fiv => fiv.ValidationOrder).HasDefaultValue(0);
            builder.Property(fiv => fiv.Severity).HasDefaultValue("Error");
            builder.Property(fiv => fiv.IsActive).HasDefaultValue(true);
            builder.Property(fiv => fiv.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(fiv => fiv.Item)
                .WithMany(fti => fti.Validations)
                .HasForeignKey(fiv => fiv.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
