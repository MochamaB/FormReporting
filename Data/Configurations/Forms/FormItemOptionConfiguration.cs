using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class FormItemOptionConfiguration : IEntityTypeConfiguration<FormItemOption>
    {
        public void Configure(EntityTypeBuilder<FormItemOption> builder)
        {
            // Primary Key
            builder.HasKey(fio => fio.OptionId);

            // Unique Constraints
            builder.HasIndex(fio => new { fio.ItemId, fio.OptionValue })
                .IsUnique()
                .HasDatabaseName("UQ_ItemOption");

            // Indexes
            builder.HasIndex(fio => new { fio.ItemId, fio.DisplayOrder, fio.IsActive })
                .HasDatabaseName("IX_ItemOptions_Item");

            builder.HasIndex(fio => fio.ParentOptionId)
                .HasDatabaseName("IX_ItemOptions_Parent");

            // Default Values
            builder.Property(fio => fio.DisplayOrder).HasDefaultValue(0);
            builder.Property(fio => fio.IsDefault).HasDefaultValue(false);
            builder.Property(fio => fio.IsActive).HasDefaultValue(true);

            // Relationships
            builder.HasOne(fio => fio.Item)
                .WithMany(fti => fti.Options)
                .HasForeignKey(fio => fio.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(fio => fio.ParentOption)
                .WithMany(po => po.ChildOptions)
                .HasForeignKey(fio => fio.ParentOptionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
