using FormReporting.Models.Entities.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Tickets
{
    public class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
    {
        public void Configure(EntityTypeBuilder<TicketComment> builder)
        {
            // Primary Key
            builder.HasKey(tc => tc.CommentId);

            // Indexes
            builder.HasIndex(tc => new { tc.TicketId, tc.CreatedDate })
                .HasDatabaseName("IX_Comments_Ticket");

            // Default Values
            builder.Property(tc => tc.IsInternal).HasDefaultValue(false);
            builder.Property(tc => tc.CreatedDate).HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(tc => tc.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(tc => tc.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tc => tc.User)
                .WithMany()
                .HasForeignKey(tc => tc.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
