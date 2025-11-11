using FormReporting.Models.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Audit
{
    public class UserActivityLogConfiguration : IEntityTypeConfiguration<UserActivityLog>
    {
        public void Configure(EntityTypeBuilder<UserActivityLog> builder)
        {
            builder.HasKey(ual => ual.ActivityId);

            builder.HasOne(ual => ual.User)
                .WithMany()
                .HasForeignKey(ual => ual.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(ual => new { ual.UserId, ual.ActivityDate })
                .HasDatabaseName("IX_Activity_User")
                .IsDescending(false, true);

            builder.HasIndex(ual => ual.ActivityDate)
                .HasDatabaseName("IX_Activity_Date")
                .IsDescending();
        }
    }
}
