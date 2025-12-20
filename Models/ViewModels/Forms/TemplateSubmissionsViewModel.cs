using FormReporting.Models.Entities.Forms;

namespace FormReporting.Models.ViewModels.Forms
{
    /// <summary>
    /// ViewModel for the Template Submissions page
    /// Displays all submissions for a specific template, filtered by user scope
    /// </summary>
    public class TemplateSubmissionsViewModel
    {
        // ========================================================================
        // TEMPLATE INFO
        // ========================================================================
        
        /// <summary>
        /// Template ID
        /// </summary>
        public int TemplateId { get; set; }
        
        /// <summary>
        /// Template name for display
        /// </summary>
        public string TemplateName { get; set; } = string.Empty;
        
        /// <summary>
        /// Template code for URLs
        /// </summary>
        public string TemplateCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Template description
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Category name
        /// </summary>
        public string CategoryName { get; set; } = "Uncategorized";
        
        /// <summary>
        /// Category icon class
        /// </summary>
        public string CategoryIcon { get; set; } = "ri-file-list-3-line";
        
        /// <summary>
        /// Template version
        /// </summary>
        public string Version { get; set; } = "1.0";
        
        /// <summary>
        /// Template type (Survey, Checklist, Assessment, etc.)
        /// </summary>
        public string TemplateType { get; set; } = string.Empty;
        
        /// <summary>
        /// When the template was published
        /// </summary>
        public DateTime? PublishedDate { get; set; }
        
        /// <summary>
        /// Whether the template requires approval
        /// </summary>
        public bool RequiresApproval { get; set; }
        
        // ========================================================================
        // SCOPE INFO
        // ========================================================================
        
        /// <summary>
        /// User's scope code (GLOBAL, REGIONAL, TENANT, etc.)
        /// </summary>
        public string ScopeCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Human-readable scope description
        /// e.g., "Viewing: Central Region Submissions"
        /// </summary>
        public string ScopeDisplayText { get; set; } = string.Empty;
        
        /// <summary>
        /// Total submissions visible to user (within scope)
        /// </summary>
        public int TotalSubmissions { get; set; }
        
        // ========================================================================
        // FORM FIELDS (for table columns)
        // ========================================================================
        
        /// <summary>
        /// Form fields to display as columns in the table
        /// Limited to first N key fields
        /// </summary>
        public List<FormFieldColumnInfo> FieldColumns { get; set; } = new();
        
        // ========================================================================
        // SUBMISSIONS LIST
        // ========================================================================
        
        /// <summary>
        /// List of submissions for the current page
        /// </summary>
        public List<SubmissionRowViewModel> Submissions { get; set; } = new();
        
        // ========================================================================
        // SUMMARY STATS
        // ========================================================================
        
        /// <summary>
        /// Aggregated statistics for the Summary tab
        /// </summary>
        public SubmissionSummaryStats Summary { get; set; } = new();
        
        // ========================================================================
        // FILTERS
        // ========================================================================
        
        /// <summary>
        /// Current filter state
        /// </summary>
        public SubmissionFilters Filters { get; set; } = new();
        
        /// <summary>
        /// Available tenants for filtering (within user's scope)
        /// </summary>
        public List<TenantFilterOption> AvailableTenants { get; set; } = new();
        
        /// <summary>
        /// Available submitters for filtering (users who have submitted this template)
        /// </summary>
        public List<SubmitterFilterOption> AvailableSubmitters { get; set; } = new();
        
        // ========================================================================
        // PAGINATION
        // ========================================================================
        
        /// <summary>
        /// Current page number (1-indexed)
        /// </summary>
        public int CurrentPage { get; set; } = 1;
        
        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; } = 1;
        
        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; } = 10;
        
        /// <summary>
        /// Total number of records (for display)
        /// </summary>
        public int TotalRecords { get; set; }
        
        /// <summary>
        /// Current active tab (responses or summary)
        /// </summary>
        public string ActiveTab { get; set; } = "responses";
    }
    
    /// <summary>
    /// Information about a form field for table column display
    /// </summary>
    public class FormFieldColumnInfo
    {
        /// <summary>
        /// Field/Item ID
        /// </summary>
        public int ItemId { get; set; }
        
        /// <summary>
        /// Section ID this field belongs to
        /// </summary>
        public int SectionId { get; set; }
        
        /// <summary>
        /// Section name for grouping header
        /// </summary>
        public string SectionName { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether this is the first field in its section
        /// </summary>
        public bool IsFirstInSection { get; set; }
        
        /// <summary>
        /// Field name for column header
        /// </summary>
        public string FieldName { get; set; } = string.Empty;
        
        /// <summary>
        /// Short name for column header (truncated if needed)
        /// </summary>
        public string ShortName { get; set; } = string.Empty;
        
        /// <summary>
        /// Data type for formatting (Text, Number, Rating, Dropdown, etc.)
        /// </summary>
        public string DataType { get; set; } = "Text";
        
        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }
        
        /// <summary>
        /// Options for selection-based fields (Dropdown, Radio, MultiSelect, etc.)
        /// </summary>
        public List<FieldOptionInfo> Options { get; set; } = new List<FieldOptionInfo>();
    }
    
    /// <summary>
    /// Simplified option info for display lookup
    /// </summary>
    public class FieldOptionInfo
    {
        public int OptionId { get; set; }
        public string OptionValue { get; set; } = string.Empty;
        public string OptionLabel { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// ViewModel for a single submission row in the table
    /// </summary>
    public class SubmissionRowViewModel
    {
        /// <summary>
        /// Submission ID
        /// </summary>
        public int SubmissionId { get; set; }
        
        /// <summary>
        /// Sequential submission number for display
        /// </summary>
        public int SubmissionNumber { get; set; }
        
        /// <summary>
        /// When the submission was submitted (or last saved for drafts)
        /// </summary>
        public DateTime SubmissionDate { get; set; }
        
        /// <summary>
        /// Formatted date for display (relative for recent, absolute for older)
        /// </summary>
        public string FormattedDate { get; set; } = string.Empty;
        
        /// <summary>
        /// Submitter's display name
        /// </summary>
        public string SubmitterName { get; set; } = string.Empty;
        
        /// <summary>
        /// Submitter's email
        /// </summary>
        public string SubmitterEmail { get; set; } = string.Empty;
        
        /// <summary>
        /// Tenant/location name (if applicable)
        /// </summary>
        public string? TenantName { get; set; }
        
        /// <summary>
        /// Tenant ID (for filtering)
        /// </summary>
        public int? TenantId { get; set; }
        
        /// <summary>
        /// Reporting period formatted for display
        /// </summary>
        public string ReportingPeriod { get; set; } = string.Empty;
        
        /// <summary>
        /// Submission status (Draft, Submitted, InApproval, Approved, Rejected)
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// CSS class for status badge
        /// </summary>
        public string StatusBadgeClass { get; set; } = "bg-secondary";
        
        /// <summary>
        /// Number of fields answered
        /// </summary>
        public int AnsweredCount { get; set; }
        
        /// <summary>
        /// Total number of fields in the form
        /// </summary>
        public int TotalFields { get; set; }
        
        /// <summary>
        /// Completion percentage (0-100)
        /// </summary>
        public decimal CompletionPercentage { get; set; }
        
        /// <summary>
        /// Whether the submission is flagged/starred
        /// </summary>
        public bool IsFlagged { get; set; }
        
        /// <summary>
        /// Inline response previews for table columns
        /// </summary>
        public List<ResponsePreview> ResponsePreviews { get; set; } = new();
    }
    
    /// <summary>
    /// Preview of a response for inline table display
    /// </summary>
    public class ResponsePreview
    {
        /// <summary>
        /// Item/Field ID
        /// </summary>
        public int ItemId { get; set; }
        
        /// <summary>
        /// Formatted display value (truncated/formatted based on type)
        /// </summary>
        public string DisplayValue { get; set; } = string.Empty;
        
        /// <summary>
        /// Data type for rendering (affects how value is displayed)
        /// </summary>
        public string DataType { get; set; } = "Text";
        
        /// <summary>
        /// Numeric value for progress bars (e.g., rating score)
        /// </summary>
        public decimal? NumericValue { get; set; }
        
        /// <summary>
        /// Maximum value for progress bars (e.g., max rating)
        /// </summary>
        public decimal? MaxValue { get; set; }
        
        /// <summary>
        /// Whether this field has a response
        /// </summary>
        public bool HasValue { get; set; }
    }
    
    /// <summary>
    /// Aggregated statistics for submissions
    /// </summary>
    public class SubmissionSummaryStats
    {
        /// <summary>
        /// Total number of submissions
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// Number of draft submissions
        /// </summary>
        public int DraftCount { get; set; }
        
        /// <summary>
        /// Number of submitted (pending review) submissions
        /// </summary>
        public int SubmittedCount { get; set; }
        
        /// <summary>
        /// Number of submissions in approval workflow
        /// </summary>
        public int InApprovalCount { get; set; }
        
        /// <summary>
        /// Number of approved submissions
        /// </summary>
        public int ApprovedCount { get; set; }
        
        /// <summary>
        /// Number of rejected submissions
        /// </summary>
        public int RejectedCount { get; set; }
        
        /// <summary>
        /// Average completion percentage across all submissions
        /// </summary>
        public decimal AverageCompletionRate { get; set; }
        
        /// <summary>
        /// Percentage of submissions that are approved
        /// </summary>
        public decimal ApprovalRate { get; set; }
    }
    
    /// <summary>
    /// Current filter state for submissions
    /// </summary>
    public class SubmissionFilters
    {
        /// <summary>
        /// Filter by status
        /// </summary>
        public string? Status { get; set; }
        
        /// <summary>
        /// Filter by tenant ID
        /// </summary>
        public int? TenantId { get; set; }
        
        /// <summary>
        /// Filter by period (thisMonth, lastMonth, thisQuarter, thisYear, custom)
        /// </summary>
        public string? Period { get; set; }
        
        /// <summary>
        /// Custom date range start
        /// </summary>
        public DateTime? DateFrom { get; set; }
        
        /// <summary>
        /// Custom date range end
        /// </summary>
        public DateTime? DateTo { get; set; }
        
        /// <summary>
        /// Search text (submitter name/email, response text)
        /// </summary>
        public string? Search { get; set; }
        
        /// <summary>
        /// Filter by submitter user ID
        /// </summary>
        public int? SubmitterId { get; set; }
    }
    
    /// <summary>
    /// Tenant option for filter dropdown
    /// </summary>
    public class TenantFilterOption
    {
        /// <summary>
        /// Tenant ID
        /// </summary>
        public int TenantId { get; set; }
        
        /// <summary>
        /// Tenant name for display
        /// </summary>
        public string TenantName { get; set; } = string.Empty;
        
        /// <summary>
        /// Number of submissions from this tenant
        /// </summary>
        public int SubmissionCount { get; set; }
    }
    
    /// <summary>
    /// Submitter option for filter dropdown
    /// </summary>
    public class SubmitterFilterOption
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// User's full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// User's email
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Number of submissions by this user
        /// </summary>
        public int SubmissionCount { get; set; }
    }
}
