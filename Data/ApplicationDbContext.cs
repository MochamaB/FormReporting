using FormReporting.Data.Configurations.Identity;
using FormReporting.Data.Configurations.Organizational;
using FormReporting.Models.Entities.Identity;
using FormReporting.Models.Entities.Organizational;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Data
{
    /// <summary>
    /// Application database context for KTDA ICT Reporting System
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ============================================================================
        // SECTION 1: ORGANIZATIONAL STRUCTURE
        // ============================================================================

        /// <summary>
        /// Regions for grouping factories
        /// </summary>
        public DbSet<Region> Regions { get; set; } = null!;

        /// <summary>
        /// Tenants (HeadOffice, Factories, Subsidiaries)
        /// </summary>
        public DbSet<Tenant> Tenants { get; set; } = null!;

        /// <summary>
        /// Custom tenant groupings
        /// </summary>
        public DbSet<TenantGroup> TenantGroups { get; set; } = null!;

        /// <summary>
        /// Tenant group memberships
        /// </summary>
        public DbSet<TenantGroupMember> TenantGroupMembers { get; set; } = null!;

        /// <summary>
        /// Organizational departments within tenants
        /// </summary>
        public DbSet<Department> Departments { get; set; } = null!;

        // ============================================================================
        // SECTION 2: IDENTITY & ACCESS MANAGEMENT
        // ============================================================================

        /// <summary>
        /// Roles with hierarchical levels
        /// </summary>
        public DbSet<Role> Roles { get; set; } = null!;

        /// <summary>
        /// User accounts (ASP.NET Identity compatible)
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;

        /// <summary>
        /// User-role assignments
        /// </summary>
        public DbSet<UserRole> UserRoles { get; set; } = null!;

        /// <summary>
        /// Application modules
        /// </summary>
        public DbSet<Module> Modules { get; set; } = null!;

        /// <summary>
        /// Granular permissions
        /// </summary>
        public DbSet<Permission> Permissions { get; set; } = null!;

        /// <summary>
        /// Role-permission assignments
        /// </summary>
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;

        /// <summary>
        /// Dynamic sidebar menu items
        /// </summary>
        public DbSet<MenuItem> MenuItems { get; set; } = null!;

        /// <summary>
        /// Role-based menu visibility
        /// </summary>
        public DbSet<RoleMenuItem> RoleMenuItems { get; set; } = null!;

        /// <summary>
        /// Explicit tenant access exceptions
        /// </summary>
        public DbSet<UserTenantAccess> UserTenantAccesses { get; set; } = null!;

        /// <summary>
        /// User groups (training, projects, committees)
        /// </summary>
        public DbSet<UserGroup> UserGroups { get; set; } = null!;

        /// <summary>
        /// User group memberships
        /// </summary>
        public DbSet<UserGroupMember> UserGroupMembers { get; set; } = null!;

        // ============================================================================
        // TODO: Additional sections will be added as we scaffold them
        // - Section 3: Metrics & KPI Tracking
        // - Section 4: Form Templates & Submissions
        // - Section 5: Software Management
        // - Section 6: Hardware Inventory
        // - Section 7: Support Tickets
        // - Section 8: Financial Tracking
        // - Section 9: Reporting & Analytics
        // - Section 10: Notifications
        // - Section 11: Media Management
        // - Section 12: Audit & Logging
        // ============================================================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply Section 1: Organizational Structure configurations
            modelBuilder.ApplyConfiguration(new RegionConfiguration());
            modelBuilder.ApplyConfiguration(new TenantConfiguration());
            modelBuilder.ApplyConfiguration(new TenantGroupConfiguration());
            modelBuilder.ApplyConfiguration(new TenantGroupMemberConfiguration());
            modelBuilder.ApplyConfiguration(new DepartmentConfiguration());

            // Apply Section 2: Identity & Access Management configurations
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
            modelBuilder.ApplyConfiguration(new ModuleConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionConfiguration());
            modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
            modelBuilder.ApplyConfiguration(new MenuItemConfiguration());
            modelBuilder.ApplyConfiguration(new RoleMenuItemConfiguration());
            modelBuilder.ApplyConfiguration(new UserTenantAccessConfiguration());
            modelBuilder.ApplyConfiguration(new UserGroupConfiguration());
            modelBuilder.ApplyConfiguration(new UserGroupMemberConfiguration());

            // TODO: Apply additional configurations as we scaffold more sections
        }

        /// <summary>
        /// Override SaveChanges to automatically update ModifiedDate
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically update ModifiedDate
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Automatically update ModifiedDate for modified entities
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is Models.Common.BaseEntity baseEntity)
                {
                    baseEntity.ModifiedDate = DateTime.UtcNow;
                }
            }
        }
    }
}
