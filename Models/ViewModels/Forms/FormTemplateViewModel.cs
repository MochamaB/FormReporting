using System;

namespace FormReporting.Models.ViewModels.Forms
{
    /// <summary>
    /// View model for displaying form templates in list/grid views
    /// </summary>
    public class FormTemplateViewModel
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateCode { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string TemplateType { get; set; } = string.Empty;
        public string PublishStatus { get; set; } = string.Empty;
        public int Version { get; set; }
        public int SubmissionCount { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime ModifiedDate { get; set; }
        public string? Description { get; set; }

        // ============================================================================
        // CONFIGURATION STATUS - Shows what has been configured for this template
        // ============================================================================
        
        /// <summary>Has sections and fields configured in Form Builder</summary>
        public bool HasFormBuilder { get; set; }
        
        /// <summary>Has user/role assignments configured</summary>
        public bool HasAssignments { get; set; }
        
        /// <summary>Has approval workflow configured</summary>
        public bool HasWorkflow { get; set; }
        
        /// <summary>Has metric mappings configured</summary>
        public bool HasMetrics { get; set; }
        
        /// <summary>Number of sections in the form</summary>
        public int SectionCount { get; set; }
        
        /// <summary>Number of fields in the form</summary>
        public int FieldCount { get; set; }

        /// <summary>
        /// Configuration completion level (0-4)
        /// </summary>
        public int ConfigurationLevel => 
            (HasFormBuilder ? 1 : 0) + 
            (HasAssignments ? 1 : 0) + 
            (HasWorkflow ? 1 : 0) + 
            (HasMetrics ? 1 : 0);

        /// <summary>
        /// Formatted version display (e.g., v1.0, v2.5)
        /// </summary>
        public string VersionFormatted => $"v{Version / 10}.{Version % 10}";

        /// <summary>
        /// Formatted modified date (e.g., "Nov 13, 2025 8:00 PM")
        /// </summary>
        public string ModifiedDateFormatted => ModifiedDate.ToString("MMM d, yyyy h:mm tt");

        /// <summary>
        /// HTML badge for publish status
        /// </summary>
        public string StatusBadge => PublishStatus switch
        {
            "Draft" => "<span class='badge bg-secondary-subtle text-secondary'><i class='ri-draft-line me-1'></i>Draft</span>",
            "Published" => "<span class='badge bg-success-subtle text-success'><i class='ri-checkbox-circle-line me-1'></i>Published</span>",
            "Archived" => "<span class='badge bg-warning-subtle text-warning'><i class='ri-archive-line me-1'></i>Archived</span>",
            "Deprecated" => "<span class='badge bg-danger-subtle text-danger'><i class='ri-error-warning-line me-1'></i>Deprecated</span>",
            _ => "<span class='badge bg-light text-dark'>Unknown</span>"
        };

        /// <summary>
        /// HTML badge for template type
        /// </summary>
        public string TypeBadge => TemplateType switch
        {
            "Monthly" => "<span class='badge bg-primary-subtle text-primary'>Monthly</span>",
            "Quarterly" => "<span class='badge bg-info-subtle text-info'>Quarterly</span>",
            "Annual" => "<span class='badge bg-success-subtle text-success'>Annual</span>",
            "OnDemand" => "<span class='badge bg-warning-subtle text-warning'>On Demand</span>",
            _ => $"<span class='badge bg-light text-dark'>{TemplateType}</span>"
        };
    }
}
