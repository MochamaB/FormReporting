using FormReporting.Models.Entities.Forms;
using FormReporting.Models.Entities.Organizational;

namespace FormReporting.Models.ViewModels.Forms
{
    /// <summary>
    /// ViewModel for the Start Submission page
    /// Contains template details and options for tenant/period selection
    /// </summary>
    public class StartSubmissionViewModel
    {
        // ========== TEMPLATE INFORMATION ==========
        
        /// <summary>
        /// Template ID
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// Template name
        /// </summary>
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// Template description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Template code (e.g., "MFR-001")
        /// </summary>
        public string TemplateCode { get; set; } = string.Empty;

        /// <summary>
        /// Template type (Daily, Weekly, Monthly, Quarterly, Annual, OnDemand)
        /// </summary>
        public string TemplateType { get; set; } = string.Empty;

        /// <summary>
        /// Template version
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Date the template was published
        /// </summary>
        public DateTime? PublishedDate { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Category icon class
        /// </summary>
        public string CategoryIcon { get; set; } = "ri-file-list-3-line";

        /// <summary>
        /// Whether approval is required
        /// </summary>
        public bool RequiresApproval { get; set; }

        /// <summary>
        /// Number of sections in the form
        /// </summary>
        public int SectionCount { get; set; }

        /// <summary>
        /// Number of fields in the form
        /// </summary>
        public int FieldCount { get; set; }

        /// <summary>
        /// Estimated time to complete (in minutes)
        /// </summary>
        public int EstimatedMinutes { get; set; }

        // ========== TENANT SELECTION ==========

        /// <summary>
        /// Whether to show tenant selector (only if user has multiple tenants)
        /// </summary>
        public bool ShowTenantSelector { get; set; }

        /// <summary>
        /// List of tenants user can submit for
        /// </summary>
        public List<TenantOption> AvailableTenants { get; set; } = new();

        /// <summary>
        /// Selected tenant ID
        /// </summary>
        public int? SelectedTenantId { get; set; }

        // ========== PERIOD SELECTION ==========

        /// <summary>
        /// Suggested/default reporting period based on template type and last submission
        /// </summary>
        public DateTime SuggestedPeriod { get; set; }

        /// <summary>
        /// List of available periods for selection
        /// </summary>
        public List<PeriodOption> AvailablePeriods { get; set; } = new();

        /// <summary>
        /// Selected period value (for form submission)
        /// </summary>
        public string? SelectedPeriod { get; set; }

        /// <summary>
        /// For OnDemand: Period start date
        /// </summary>
        public DateTime? PeriodStart { get; set; }

        /// <summary>
        /// For OnDemand: Period end date
        /// </summary>
        public DateTime? PeriodEnd { get; set; }

        // ========== DRAFT DETECTION ==========

        /// <summary>
        /// Whether an existing draft was found
        /// </summary>
        public bool HasExistingDraft { get; set; }

        /// <summary>
        /// Existing draft submission ID (if found)
        /// </summary>
        public int? ExistingDraftId { get; set; }

        /// <summary>
        /// Last saved date of existing draft
        /// </summary>
        public DateTime? DraftLastSaved { get; set; }

        /// <summary>
        /// Current section of existing draft
        /// </summary>
        public int DraftCurrentSection { get; set; }
    }

    /// <summary>
    /// Tenant option for dropdown
    /// </summary>
    public class TenantOption
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string TenantCode { get; set; } = string.Empty;
        public string TenantType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Period option for dropdown
    /// </summary>
    public class PeriodOption
    {
        /// <summary>
        /// Value to submit (e.g., "2025-12-01")
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Display label (e.g., "December 2025")
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Whether this is the suggested/default period
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Whether a draft exists for this period
        /// </summary>
        public bool HasDraft { get; set; }
    }
}
