namespace FormReporting.Models.ViewModels.Forms
{
    /// <summary>
    /// ViewModel for submission detail view (offcanvas panel)
    /// Contains full submission information and all responses
    /// </summary>
    public class SubmissionDetailViewModel
    {
        // ========================================================================
        // SUBMISSION INFO
        // ========================================================================
        
        /// <summary>
        /// Submission ID
        /// </summary>
        public int SubmissionId { get; set; }
        
        /// <summary>
        /// Sequential submission number
        /// </summary>
        public int SubmissionNumber { get; set; }
        
        /// <summary>
        /// Template ID
        /// </summary>
        public int TemplateId { get; set; }
        
        /// <summary>
        /// Template name
        /// </summary>
        public string TemplateName { get; set; } = string.Empty;
        
        // ========================================================================
        // SUBMITTER INFO
        // ========================================================================
        
        /// <summary>
        /// Submitter's display name
        /// </summary>
        public string SubmitterName { get; set; } = string.Empty;
        
        /// <summary>
        /// Submitter's email
        /// </summary>
        public string SubmitterEmail { get; set; } = string.Empty;
        
        /// <summary>
        /// Submitter's user ID
        /// </summary>
        public int SubmittedBy { get; set; }
        
        // ========================================================================
        // CONTEXT INFO
        // ========================================================================
        
        /// <summary>
        /// Tenant/location name (if applicable)
        /// </summary>
        public string? TenantName { get; set; }
        
        /// <summary>
        /// Tenant ID
        /// </summary>
        public int? TenantId { get; set; }
        
        /// <summary>
        /// Reporting period formatted for display (e.g., "November 2025")
        /// </summary>
        public string ReportingPeriod { get; set; } = string.Empty;
        
        /// <summary>
        /// Reporting year
        /// </summary>
        public int ReportingYear { get; set; }
        
        /// <summary>
        /// Reporting month
        /// </summary>
        public int ReportingMonth { get; set; }
        
        // ========================================================================
        // STATUS & DATES
        // ========================================================================
        
        /// <summary>
        /// Submission status
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// CSS class for status badge
        /// </summary>
        public string StatusBadgeClass { get; set; } = "bg-secondary";
        
        /// <summary>
        /// When the form was submitted
        /// </summary>
        public DateTime? SubmittedDate { get; set; }
        
        /// <summary>
        /// Formatted submission date
        /// </summary>
        public string FormattedSubmittedDate { get; set; } = string.Empty;
        
        /// <summary>
        /// When the submission was created
        /// </summary>
        public DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// When the submission was last modified
        /// </summary>
        public DateTime ModifiedDate { get; set; }
        
        // ========================================================================
        // APPROVAL INFO
        // ========================================================================
        
        /// <summary>
        /// Reviewer's name (if reviewed)
        /// </summary>
        public string? ReviewerName { get; set; }
        
        /// <summary>
        /// When the submission was reviewed
        /// </summary>
        public DateTime? ReviewedDate { get; set; }
        
        /// <summary>
        /// Formatted review date
        /// </summary>
        public string? FormattedReviewedDate { get; set; }
        
        /// <summary>
        /// Approval/rejection comments
        /// </summary>
        public string? ApprovalComments { get; set; }
        
        // ========================================================================
        // COMPLETION INFO
        // ========================================================================
        
        /// <summary>
        /// Number of fields answered
        /// </summary>
        public int AnsweredCount { get; set; }
        
        /// <summary>
        /// Total number of fields
        /// </summary>
        public int TotalFields { get; set; }
        
        /// <summary>
        /// Completion percentage
        /// </summary>
        public decimal CompletionPercentage { get; set; }
        
        // ========================================================================
        // RESPONSES
        // ========================================================================
        
        /// <summary>
        /// Responses grouped by section
        /// </summary>
        public List<SectionResponseGroup> Sections { get; set; } = new();
        
        // ========================================================================
        // ADDITIONAL INFO
        // ========================================================================
        
        /// <summary>
        /// Number of comments on this submission
        /// </summary>
        public int CommentsCount { get; set; }
        
        /// <summary>
        /// Whether the submission has workflow progress entries
        /// </summary>
        public bool HasWorkflowProgress { get; set; }
        
        /// <summary>
        /// Current workflow step name (if in approval)
        /// </summary>
        public string? CurrentWorkflowStep { get; set; }
    }
    
    /// <summary>
    /// Group of responses for a form section
    /// </summary>
    public class SectionResponseGroup
    {
        /// <summary>
        /// Section ID
        /// </summary>
        public int SectionId { get; set; }
        
        /// <summary>
        /// Section name
        /// </summary>
        public string SectionName { get; set; } = string.Empty;
        
        /// <summary>
        /// Section description
        /// </summary>
        public string? SectionDescription { get; set; }
        
        /// <summary>
        /// Section icon class
        /// </summary>
        public string IconClass { get; set; } = "ri-list-check";
        
        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }
        
        /// <summary>
        /// Responses in this section
        /// </summary>
        public List<ResponseDetailViewModel> Responses { get; set; } = new();
    }
    
    /// <summary>
    /// Detailed view of a single response
    /// </summary>
    public class ResponseDetailViewModel
    {
        /// <summary>
        /// Response ID
        /// </summary>
        public long ResponseId { get; set; }
        
        /// <summary>
        /// Item/Field ID
        /// </summary>
        public int ItemId { get; set; }
        
        /// <summary>
        /// Field name/question
        /// </summary>
        public string FieldName { get; set; } = string.Empty;
        
        /// <summary>
        /// Field description/help text
        /// </summary>
        public string? FieldDescription { get; set; }
        
        /// <summary>
        /// Data type (Text, Number, Rating, Dropdown, etc.)
        /// </summary>
        public string DataType { get; set; } = "Text";
        
        /// <summary>
        /// Whether the field is required
        /// </summary>
        public bool IsRequired { get; set; }
        
        /// <summary>
        /// Display order within section
        /// </summary>
        public int DisplayOrder { get; set; }
        
        /// <summary>
        /// Formatted display value
        /// </summary>
        public string DisplayValue { get; set; } = string.Empty;
        
        /// <summary>
        /// Raw text value
        /// </summary>
        public string? TextValue { get; set; }
        
        /// <summary>
        /// Numeric value
        /// </summary>
        public decimal? NumericValue { get; set; }
        
        /// <summary>
        /// Date value
        /// </summary>
        public DateTime? DateValue { get; set; }
        
        /// <summary>
        /// Boolean value
        /// </summary>
        public bool? BooleanValue { get; set; }
        
        /// <summary>
        /// Selected option label (for dropdowns/radios)
        /// </summary>
        public string? SelectedOptionLabel { get; set; }
        
        /// <summary>
        /// Remarks/notes for this response
        /// </summary>
        public string? Remarks { get; set; }
        
        /// <summary>
        /// Whether this field has a response
        /// </summary>
        public bool HasValue { get; set; }
        
        // ========================================================================
        // RATING/SCORE FIELDS
        // ========================================================================
        
        /// <summary>
        /// Rating/score value
        /// </summary>
        public decimal? RatingValue { get; set; }
        
        /// <summary>
        /// Maximum rating value
        /// </summary>
        public decimal? RatingMax { get; set; }
        
        /// <summary>
        /// Score weight
        /// </summary>
        public decimal? ScoreWeight { get; set; }
        
        /// <summary>
        /// Weighted score
        /// </summary>
        public decimal? WeightedScore { get; set; }
        
        // ========================================================================
        // FILE FIELDS
        // ========================================================================
        
        /// <summary>
        /// File URLs for file upload fields
        /// </summary>
        public List<FileAttachment> Files { get; set; } = new();
    }
    
    /// <summary>
    /// File attachment information
    /// </summary>
    public class FileAttachment
    {
        /// <summary>
        /// File URL
        /// </summary>
        public string Url { get; set; } = string.Empty;
        
        /// <summary>
        /// File name
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        
        /// <summary>
        /// File type/extension
        /// </summary>
        public string FileType { get; set; } = string.Empty;
        
        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSize { get; set; }
        
        /// <summary>
        /// Whether this is an image file
        /// </summary>
        public bool IsImage { get; set; }
        
        /// <summary>
        /// Thumbnail URL for images
        /// </summary>
        public string? ThumbnailUrl { get; set; }
    }
}
