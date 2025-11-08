using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Forms
{
    public class FormItemConfigurationConfiguration : IEntityTypeConfiguration<FormItemConfiguration>
    {
        public void Configure(EntityTypeBuilder<FormItemConfiguration> builder)
        {
            // Primary Key
            builder.HasKey(fic => fic.ConfigId);

            // Unique Constraints
            builder.HasIndex(fic => new { fic.ItemId, fic.ConfigKey })
                .IsUnique()
                .HasDatabaseName("UQ_ItemConfig");

            // Indexes
            builder.HasIndex(fic => fic.ItemId)
                .HasDatabaseName("IX_ItemConfig_Item");

            builder.HasIndex(fic => fic.ConfigKey)
                .HasDatabaseName("IX_ItemConfig_Key");

            // Relationships
            builder.HasOne(fic => fic.Item)
                .WithMany(fti => fti.Configurations)
                .HasForeignKey(fic => fic.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
