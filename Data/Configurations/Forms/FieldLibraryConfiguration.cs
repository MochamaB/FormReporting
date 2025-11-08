using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class FieldLibraryConfiguration : IEntityTypeConfiguration<FieldLibrary>
    {
        public void Configure(EntityTypeBuilder<FieldLibrary> builder)
        {
            // Primary Key
            builder.HasKey(fl => fl.LibraryFieldId);

            // Unique Constraints
            builder.HasIndex(fl => fl.FieldCode).IsUnique();

            // Indexes
            builder.HasIndex(fl => new { fl.Category, fl.IsActive })
                .HasDatabaseName("IX_FieldLibrary_Category");

            builder.HasIndex(fl => new { fl.FieldType, fl.IsActive })
                .HasDatabaseName("IX_FieldLibrary_Type");

            builder.HasIndex(fl => fl.FieldCode)
                .HasDatabaseName("IX_FieldLibrary_Code");

            // Default Values
            builder.Property(fl => fl.IsActive).HasDefaultValue(true);
            builder.Property(fl => fl.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(fl => fl.ModifiedDate).HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(fl => fl.Creator)
                .WithMany()
                .HasForeignKey(fl => fl.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(fl => fl.FormTemplateItems)
                .WithOne(fti => fti.LibraryField)
                .HasForeignKey(fti => fti.LibraryFieldId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
