using FormReporting.Data.Configurations.Identity;
using FormReporting.Data.Configurations.Organizational;
using FormReporting.Data.Configurations.Metrics;
using FormReporting.Data.Configurations.Forms;
using FormReporting.Data.Configurations.Software;
using FormReporting.Data.Configurations.Hardware;
using FormReporting.Data.Configurations.Tickets;
using FormReporting.Data.Configurations.Financial;
using FormReporting.Models.Entities.Identity;
using FormReporting.Models.Entities.Organizational;
using FormReporting.Models.Entities.Metrics;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.Entities.Software;
using FormReporting.Models.Entities.Hardware;
using FormReporting.Models.Entities.Tickets;
using FormReporting.Models.Entities.Financial;
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
        // SECTION 3: METRICS & KPI TRACKING
        // ============================================================================

        /// <summary>
        /// Metric definitions with KPI thresholds
        /// </summary>
        public DbSet<MetricDefinition> MetricDefinitions { get; set; } = null!;

        /// <summary>
        /// Time-series metric data for tenants
        /// </summary>
        public DbSet<TenantMetric> TenantMetrics { get; set; } = null!;

        /// <summary>
        /// System metric logs from automated jobs
        /// </summary>
        public DbSet<SystemMetricLog> SystemMetricLogs { get; set; } = null!;

        // ============================================================================
        // SECTION 4: FORM TEMPLATES & SUBMISSIONS
        // ============================================================================

        /// <summary>
        /// Form template categories
        /// </summary>
        public DbSet<FormCategory> FormCategories { get; set; } = null!;

        /// <summary>
        /// Reusable field library
        /// </summary>
        public DbSet<FieldLibrary> FieldLibraries { get; set; } = null!;

        /// <summary>
        /// Form templates
        /// </summary>
        public DbSet<FormTemplate> FormTemplates { get; set; } = null!;

        /// <summary>
        /// Form template sections
        /// </summary>
        public DbSet<FormTemplateSection> FormTemplateSections { get; set; } = null!;

        /// <summary>
        /// Form template items (fields/questions)
        /// </summary>
        public DbSet<FormTemplateItem> FormTemplateItems { get; set; } = null!;

        /// <summary>
        /// Form item options (dropdown/radio/checkbox)
        /// </summary>
        public DbSet<FormItemOption> FormItemOptions { get; set; } = null!;

        /// <summary>
        /// Form item configurations
        /// </summary>
        public DbSet<FormItemConfiguration> FormItemConfigurations { get; set; } = null!;

        /// <summary>
        /// Form item validations
        /// </summary>
        public DbSet<FormItemValidation> FormItemValidations { get; set; } = null!;

        /// <summary>
        /// Form item calculations
        /// </summary>
        public DbSet<FormItemCalculation> FormItemCalculations { get; set; } = null!;

        /// <summary>
        /// Form item to metric mappings
        /// </summary>
        public DbSet<FormItemMetricMapping> FormItemMetricMappings { get; set; } = null!;

        /// <summary>
        /// Metric population logs
        /// </summary>
        public DbSet<MetricPopulationLog> MetricPopulationLogs { get; set; } = null!;

        /// <summary>
        /// Form template submissions
        /// </summary>
        public DbSet<FormTemplateSubmission> FormTemplateSubmissions { get; set; } = null!;

        /// <summary>
        /// Form template responses
        /// </summary>
        public DbSet<FormTemplateResponse> FormTemplateResponses { get; set; } = null!;

        /// <summary>
        /// Submission workflow progress
        /// </summary>
        public DbSet<SubmissionWorkflowProgress> SubmissionWorkflowProgresses { get; set; } = null!;

        /// <summary>
        /// Workflow definitions
        /// </summary>
        public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; } = null!;

        /// <summary>
        /// Workflow steps
        /// </summary>
        public DbSet<WorkflowStep> WorkflowSteps { get; set; } = null!;

        /// <summary>
        /// Section routing (skip logic)
        /// </summary>
        public DbSet<SectionRouting> SectionRoutings { get; set; } = null!;

        /// <summary>
        /// Form analytics
        /// </summary>
        public DbSet<FormAnalytics> FormAnalytics { get; set; } = null!;

        /// <summary>
        /// Form template assignments
        /// </summary>
        public DbSet<FormTemplateAssignment> FormTemplateAssignments { get; set; } = null!;

        // ============================================================================
        // SECTION 5: SOFTWARE MANAGEMENT
        // ============================================================================

        /// <summary>
        /// Software products catalog
        /// </summary>
        public DbSet<SoftwareProduct> SoftwareProducts { get; set; } = null!;

        /// <summary>
        /// Software version registry
        /// </summary>
        public DbSet<SoftwareVersion> SoftwareVersions { get; set; } = null!;

        /// <summary>
        /// Software licenses
        /// </summary>
        public DbSet<SoftwareLicense> SoftwareLicenses { get; set; } = null!;

        /// <summary>
        /// Tenant software installations
        /// </summary>
        public DbSet<TenantSoftwareInstallation> TenantSoftwareInstallations { get; set; } = null!;

        /// <summary>
        /// Software installation history
        /// </summary>
        public DbSet<SoftwareInstallationHistory> SoftwareInstallationHistories { get; set; } = null!;

        // ============================================================================
        // SECTION 6: HARDWARE INVENTORY
        // ============================================================================

        /// <summary>
        /// Hardware categories
        /// </summary>
        public DbSet<HardwareCategory> HardwareCategories { get; set; } = null!;

        /// <summary>
        /// Hardware items master list
        /// </summary>
        public DbSet<HardwareItem> HardwareItems { get; set; } = null!;

        /// <summary>
        /// Tenant hardware inventory
        /// </summary>
        public DbSet<TenantHardware> TenantHardwares { get; set; } = null!;

        /// <summary>
        /// Hardware maintenance logs
        /// </summary>
        public DbSet<HardwareMaintenanceLog> HardwareMaintenanceLogs { get; set; } = null!;

        // ============================================================================
        // SECTION 7: SUPPORT TICKETS
        // ============================================================================

        /// <summary>
        /// Ticket categories
        /// </summary>
        public DbSet<TicketCategory> TicketCategories { get; set; } = null!;

        /// <summary>
        /// Support tickets
        /// </summary>
        public DbSet<Ticket> Tickets { get; set; } = null!;

        /// <summary>
        /// Ticket comments
        /// </summary>
        public DbSet<TicketComment> TicketComments { get; set; } = null!;

        // ============================================================================
        // SECTION 8: FINANCIAL TRACKING
        // ============================================================================

        /// <summary>
        /// Budget categories
        /// </summary>
        public DbSet<BudgetCategory> BudgetCategories { get; set; } = null!;

        /// <summary>
        /// Tenant budgets
        /// </summary>
        public DbSet<TenantBudget> TenantBudgets { get; set; } = null!;

        /// <summary>
        /// Tenant expenses
        /// </summary>
        public DbSet<TenantExpense> TenantExpenses { get; set; } = null!;

        // ============================================================================
        // TODO: Additional sections will be added as we scaffold them
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

            // Apply Section 3: Metrics & KPI Tracking configurations
            modelBuilder.ApplyConfiguration(new MetricDefinitionConfiguration());
            modelBuilder.ApplyConfiguration(new TenantMetricConfiguration());
            modelBuilder.ApplyConfiguration(new SystemMetricLogConfiguration());

            // Apply Section 4: Form Templates & Submissions configurations
            modelBuilder.ApplyConfiguration(new FormCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new FieldLibraryConfiguration());
            modelBuilder.ApplyConfiguration(new FormTemplateConfiguration());
            modelBuilder.ApplyConfiguration(new FormTemplateSectionConfiguration());
            modelBuilder.ApplyConfiguration(new FormTemplateItemConfiguration());
            modelBuilder.ApplyConfiguration(new FormItemOptionConfiguration());
            modelBuilder.ApplyConfiguration(new FormItemConfigurationConfiguration());
            modelBuilder.ApplyConfiguration(new FormItemValidationConfiguration());
            modelBuilder.ApplyConfiguration(new FormItemCalculationConfiguration());
            modelBuilder.ApplyConfiguration(new FormItemMetricMappingConfiguration());
            modelBuilder.ApplyConfiguration(new MetricPopulationLogConfiguration());
            modelBuilder.ApplyConfiguration(new FormTemplateSubmissionConfiguration());
            modelBuilder.ApplyConfiguration(new FormTemplateResponseConfiguration());
            modelBuilder.ApplyConfiguration(new SubmissionWorkflowProgressConfiguration());
            modelBuilder.ApplyConfiguration(new WorkflowDefinitionConfiguration());
            modelBuilder.ApplyConfiguration(new WorkflowStepConfiguration());
            modelBuilder.ApplyConfiguration(new SectionRoutingConfiguration());
            modelBuilder.ApplyConfiguration(new FormAnalyticsConfiguration());
            modelBuilder.ApplyConfiguration(new FormTemplateAssignmentConfiguration());

            // Apply Section 5: Software Management configurations
            modelBuilder.ApplyConfiguration(new SoftwareProductConfiguration());
            modelBuilder.ApplyConfiguration(new SoftwareVersionConfiguration());
            modelBuilder.ApplyConfiguration(new SoftwareLicenseConfiguration());
            modelBuilder.ApplyConfiguration(new TenantSoftwareInstallationConfiguration());
            modelBuilder.ApplyConfiguration(new SoftwareInstallationHistoryConfiguration());

            // Apply Section 6: Hardware Inventory configurations
            modelBuilder.ApplyConfiguration(new HardwareCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new HardwareItemConfiguration());
            modelBuilder.ApplyConfiguration(new TenantHardwareConfiguration());
            modelBuilder.ApplyConfiguration(new HardwareMaintenanceLogConfiguration());

            // Apply Section 7: Support Tickets configurations
            modelBuilder.ApplyConfiguration(new TicketCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new TicketConfiguration());
            modelBuilder.ApplyConfiguration(new TicketCommentConfiguration());

            // Apply Section 8: Financial Tracking configurations
            modelBuilder.ApplyConfiguration(new BudgetCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new TenantBudgetConfiguration());
            modelBuilder.ApplyConfiguration(new TenantExpenseConfiguration());

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
