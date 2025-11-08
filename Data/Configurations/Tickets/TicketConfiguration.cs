using FormReporting.Models.Entities.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Tickets
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            // Primary Key
            builder.HasKey(t => t.TicketId);

            // Unique Constraints
            builder.HasIndex(t => t.TicketNumber).IsUnique();

            // Indexes
            builder.HasIndex(t => new { t.Status, t.TenantId })
                .HasDatabaseName("IX_Tickets_Status");

            builder.HasIndex(t => new { t.TenantId, t.Status })
                .HasDatabaseName("IX_Tickets_Tenant");

            builder.HasIndex(t => new { t.Priority, t.Status })
                .HasDatabaseName("IX_Tickets_Priority");

            builder.HasIndex(t => new { t.SLADueDate, t.Status })
                .HasFilter("Status <> 'Resolved' AND Status <> 'Closed'")
                .HasDatabaseName("IX_Tickets_SLA");

            builder.HasIndex(t => new { t.ExternalSystem, t.ExternalTicketId })
                .HasFilter("IsExternal = 1")
                .HasDatabaseName("IX_Tickets_External");

            builder.HasIndex(t => t.ExternalTicketId)
                .HasFilter("ExternalTicketId IS NOT NULL")
                .HasDatabaseName("IX_Tickets_ExternalId");

            builder.HasIndex(t => new { t.LastSyncDate, t.SyncStatus })
                .HasFilter("IsExternal = 1")
                .HasDatabaseName("IX_Tickets_Sync");

            builder.HasIndex(t => new { t.SyncStatus, t.LastSyncDate })
                .HasFilter("SyncStatus = 'Failed'")
                .HasDatabaseName("IX_Tickets_SyncFailed");

            builder.HasIndex(t => t.RelatedHardwareId)
                .HasFilter("RelatedHardwareId IS NOT NULL")
                .HasDatabaseName("IX_Tickets_Hardware");

            builder.HasIndex(t => t.RelatedSoftwareId)
                .HasFilter("RelatedSoftwareId IS NOT NULL")
                .HasDatabaseName("IX_Tickets_Software");

            builder.HasIndex(t => new { t.AssignedTo, t.Status })
                .HasFilter("AssignedTo IS NOT NULL")
                .HasDatabaseName("IX_Tickets_Assigned");

            // Check Constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Ticket_Priority",
                "Priority IN ('Low', 'Medium', 'High', 'Critical')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Ticket_Status",
                "Status IN ('Open', 'InProgress', 'Escalated', 'Resolved', 'Closed', 'Cancelled')"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Ticket_ExternalSystem",
                "ExternalSystem IN ('Internal', 'Jira', 'ServiceNow', 'Zendesk', 'Freshdesk', 'BMC', 'ManageEngine', 'Other') OR ExternalSystem IS NULL"
            ));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Ticket_SyncStatus",
                "SyncStatus IN ('Synced', 'Pending', 'Failed', 'NotApplicable')"
            ));

            // Default Values
            builder.Property(t => t.Priority).HasDefaultValue("Medium");
            builder.Property(t => t.Status).HasDefaultValue("Open");
            builder.Property(t => t.ReportedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(t => t.IsExternal).HasDefaultValue(false);
            builder.Property(t => t.SyncStatus).HasDefaultValue("Synced");
            builder.Property(t => t.IsSLABreached).HasDefaultValue(false);
            builder.Property(t => t.CreatedDate).HasDefaultValueSql("GETDATE()");
            builder.Property(t => t.ModifiedDate).HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(t => t.Tenant)
                .WithMany()
                .HasForeignKey(t => t.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Category)
                .WithMany(tc => tc.Tickets)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Reporter)
                .WithMany()
                .HasForeignKey(t => t.ReportedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.AssignedUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedTo)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.EscalatedUser)
                .WithMany()
                .HasForeignKey(t => t.EscalatedTo)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Resolver)
                .WithMany()
                .HasForeignKey(t => t.ResolvedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.RelatedHardware)
                .WithMany()
                .HasForeignKey(t => t.RelatedHardwareId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.RelatedSoftware)
                .WithMany()
                .HasForeignKey(t => t.RelatedSoftwareId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.Comments)
                .WithOne(tc => tc.Ticket)
                .HasForeignKey(tc => tc.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
