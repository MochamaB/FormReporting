// Data/Configurations/Identity/MenuSectionConfiguration.cs
using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Identity
{
    public class MenuSectionConfiguration : IEntityTypeConfiguration<MenuSection>
    {
        public void Configure(EntityTypeBuilder<MenuSection> builder)
        {
            builder.ToTable("MenuSections");
            builder.HasKey(ms => ms.MenuSectionId);
            
            builder.Property(ms => ms.SectionName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ms => ms.SectionCode)
                .HasMaxLength(10);

            builder.Property(ms => ms.Description)
                .HasMaxLength(500);

            builder.HasMany(ms => ms.Modules)
                .WithOne(m => m.MenuSection)
                .HasForeignKey(m => m.MenuSectionId);
        }
    }
}