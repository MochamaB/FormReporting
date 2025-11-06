using FormReporting.Models.Entities.Organizational;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormReporting.Data.Configurations.Organizational
{
    /// <summary>
    /// EF Core configuration for Department entity
    /// </summary>
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            // Table name
            builder.ToTable("Departments");

            // Primary key
            builder.HasKey(e => e.DepartmentId);

            // Properties
            builder.Property(e => e.DepartmentId)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.TenantId)
                .IsRequired();

            builder.Property(e => e.DepartmentName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.DepartmentCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.ModifiedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint: Department code must be unique within a tenant
            builder.HasIndex(e => new { e.TenantId, e.DepartmentCode })
                .IsUnique()
                .HasDatabaseName("UQ_Department_Tenant_Code");

            // Indexes
            builder.HasIndex(e => e.TenantId)
                .HasDatabaseName("IX_Department_Tenant");

            builder.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_Department_Active");

            // Relationships
            builder.HasOne(e => e.Tenant)
                .WithMany(t => t.Departments)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Department_Tenant");

            builder.HasOne(e => e.ParentDepartment)
                .WithMany(d => d.ChildDepartments)
                .HasForeignKey(e => e.ParentDepartmentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Department_Parent");

            builder.HasMany(e => e.ChildDepartments)
                .WithOne(d => d.ParentDepartment)
                .HasForeignKey(d => d.ParentDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Users)
                .WithOne()
                .HasForeignKey("DepartmentId")
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
