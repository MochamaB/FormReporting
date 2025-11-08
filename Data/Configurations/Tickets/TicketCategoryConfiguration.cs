using FormReporting.Models.Entities.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Tickets
{
    public class TicketCategoryConfiguration : IEntityTypeConfiguration<TicketCategory>
    {
        public void Configure(EntityTypeBuilder<TicketCategory> builder)
        {
            // Primary Key
            builder.HasKey(tc => tc.CategoryId);

            // Unique Constraints
            builder.HasIndex(tc => tc.CategoryCode).IsUnique();

            // Indexes
            builder.HasIndex(tc => tc.ParentCategoryId)
                .HasFilter("ParentCategoryId IS NOT NULL")
                .HasDatabaseName("IX_TicketCategories_Parent");

            builder.HasIndex(tc => new { tc.IsActive, tc.DisplayOrder })
                .HasDatabaseName("IX_TicketCategories_Active");

            // Default Values
            builder.Property(tc => tc.DisplayOrder).HasDefaultValue(0);
            builder.Property(tc => tc.IsActive).HasDefaultValue(true);

            // Relationships
            builder.HasOne(tc => tc.ParentCategory)
                .WithMany(pc => pc.ChildCategories)
                .HasForeignKey(tc => tc.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(tc => tc.Tickets)
                .WithOne(t => t.Category)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
