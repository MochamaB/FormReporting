using FormReporting.Models.Entities.Forms;
using FormReporting.Models.ViewModels.Components;
using FormReporting.Models.Common;

namespace FormReporting.Models.ViewModels.Forms
{
    /// <summary>
    /// ViewModel for the Template Details page (post-publish configuration)
    /// Contains all data needed for the tabbed interface
    /// </summary>
    public class TemplateDetailsViewModel
    {
        // ============================================================================
        // TEMPLATE INFO
        // ============================================================================
        
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TemplateType { get; set; } = string.Empty;
        public string PublishStatus { get; set; } = string.Empty;
        public SubmissionMode SubmissionMode { get; set; } = SubmissionMode.Individual;
        public string? CategoryName { get; set; }
        public int? CategoryId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string? CreatorName { get; set; }
        public int Version { get; set; }

        // ============================================================================
        // STATISTICS (for Overview tab)
        // ============================================================================
        
        public TemplateStatistics Statistics { get; set; } = new();

        // ============================================================================
        // STRUCTURE (for Structure tab)
        // ============================================================================
        
        public List<TemplateSectionSummary> Sections { get; set; } = new();
        public int TotalFields { get; set; }

        // ============================================================================
        // ASSIGNMENTS (for Assignments tab)
        // ============================================================================
        
        public int AssignmentCount { get; set; }
        public int ActiveAssignmentCount { get; set; }
        public int OverdueAssignmentCount { get; set; }

        // ============================================================================
        // WORKFLOW (for Workflow tab)
        // ============================================================================
        
        public int? WorkflowId { get; set; }
        public string? WorkflowName { get; set; }
        public int WorkflowStepCount { get; set; }

        // ============================================================================
        // METRICS (for Metrics tab)
        // ============================================================================
        
        public int MetricMappingCount { get; set; }
        public int FieldMappingCount { get; set; }
        public int SectionMappingCount { get; set; }
        public int TemplateKpiCount { get; set; }
        public int TotalMappableFields { get; set; }
        public int TotalMappableSections { get; set; }
        public DateTime? MetricsLastUpdated { get; set; }
        public List<MetricMappingSummary> ConfiguredMetrics { get; set; } = new();

        // ============================================================================
        // SUBMISSION RULES (for Submission Rules tab)
        // ============================================================================
        
        public int SubmissionRuleCount { get; set; }
        public int ActiveSubmissionRuleCount { get; set; }

        // ============================================================================
        // SUBMISSIONS (for Submissions tab)
        // ============================================================================
        
        public int SubmissionCount { get; set; }
        public int PendingSubmissionCount { get; set; }
        public int CompletedSubmissionCount { get; set; }

        // ============================================================================
        // ACTIVE TAB
        // ============================================================================
        
        /// <summary>
        /// Currently active tab (overview, structure, assignments, workflow, metrics, submissions)
        /// </summary>
        public string ActiveTab { get; set; } = "overview";

        // ============================================================================
        // COMPUTED PROPERTIES
        // ============================================================================
        
        public bool IsPublished => PublishStatus == "Published";
        public bool IsDraft => PublishStatus == "Draft";
        public bool HasWorkflow => WorkflowId.HasValue;
        public bool HasAssignments => AssignmentCount > 0;
        public bool HasMetrics => MetricMappingCount > 0;
        public bool HasSubmissions => SubmissionCount > 0;

        public string StatusBadgeClass => PublishStatus switch
        {
            "Published" => "bg-success",
            "Draft" => "bg-warning",
            "Archived" => "bg-secondary",
            _ => "bg-info"
        };

        public string TypeBadgeClass => TemplateType switch
        {
            "Standard" => "bg-primary",
            "Recurring" => "bg-info",
            "OneTime" => "bg-secondary",
            _ => "bg-light text-dark"
        };
    }

    /// <summary>
    /// Template statistics for Overview tab
    /// </summary>
    public class TemplateStatistics
    {
        public int TotalSubmissions { get; set; }
        public int PendingSubmissions { get; set; }
        public int CompletedSubmissions { get; set; }
        public int OverdueSubmissions { get; set; }
        public decimal CompletionRate { get; set; }
        public int TotalAssignments { get; set; }
        public int ActiveAssignments { get; set; }
        public int OverdueAssignments { get; set; }
        public decimal AssignmentComplianceRate { get; set; }
        public DateTime? LastSubmissionDate { get; set; }
        public DateTime? NextDueDate { get; set; }
    }

    /// <summary>
    /// Section summary for Structure tab
    /// </summary>
    public class TemplateSectionSummary
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int SectionOrder { get; set; }
        public int FieldCount { get; set; }
        public int RequiredFieldCount { get; set; }
        public List<TemplateFieldSummary> Fields { get; set; } = new();
    }

    /// <summary>
    /// Field summary for Structure tab
    /// </summary>
    public class TemplateFieldSummary
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public int ItemOrder { get; set; }
        public bool HasMetricMapping { get; set; }
    }

    /// <summary>
    /// Summary of a configured metric mapping for display in the Metrics panel
    /// </summary>
    public class MetricMappingSummary
    {
        public int MappingId { get; set; }
        public string MappingName { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty; // Field, Section, Template
        public string LevelBadgeClass => Level switch
        {
            "Field" => "bg-primary-subtle text-primary",
            "Section" => "bg-info-subtle text-info",
            "Template" => "bg-success-subtle text-success",
            _ => "bg-secondary-subtle text-secondary"
        };
        public string MappingType { get; set; } = string.Empty; // Direct, Sum, Average, Weighted, etc.
        public string MetricName { get; set; } = string.Empty;
        public string? MetricCode { get; set; }
        public bool IsActive { get; set; } = true;
        public string? PreviewValue { get; set; }
        public DateTime? LastCalculated { get; set; }
    }
}
