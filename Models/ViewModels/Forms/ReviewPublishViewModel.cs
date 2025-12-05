using FormReporting.Models.Entities.Forms;

namespace FormReporting.Models.ViewModels.Forms
{
    /// <summary>
    /// ViewModel for Review & Publish step (Step 3 of wizard)
    /// Contains template summary, structure stats, and validation results
    /// </summary>
    public class ReviewPublishViewModel
    {
        // ═══════════════════════════════════════════════════════════
        // TEMPLATE SUMMARY
        // ═══════════════════════════════════════════════════════════
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateCode { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? TemplateType { get; set; }
        public int Version { get; set; }
        public string? Description { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string PublishStatus { get; set; } = "Draft";

        // ═══════════════════════════════════════════════════════════
        // FORM STRUCTURE SUMMARY
        // ═══════════════════════════════════════════════════════════
        public int SectionCount { get; set; }
        public int FieldCount { get; set; }
        public int RequiredFieldCount { get; set; }
        public int OptionalFieldCount => FieldCount - RequiredFieldCount;
        public Dictionary<string, int> FieldTypeSummary { get; set; } = new();
        public List<SectionSummary> Sections { get; set; } = new();

        // ═══════════════════════════════════════════════════════════
        // VALIDATION
        // ═══════════════════════════════════════════════════════════
        public List<ValidationItem> ValidationChecklist { get; set; } = new();
        public bool CanPublish => ValidationChecklist.All(v => v.IsWarning || v.IsPassed);
        public int PassedCount => ValidationChecklist.Count(v => v.IsPassed);
        public int FailedCount => ValidationChecklist.Count(v => !v.IsPassed && !v.IsWarning);
        public int WarningCount => ValidationChecklist.Count(v => v.IsWarning && !v.IsPassed);
    }

    /// <summary>
    /// Section summary for structure display
    /// </summary>
    public class SectionSummary
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int FieldCount { get; set; }
        public int RequiredFieldCount { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Individual validation check result
    /// </summary>
    public class ValidationItem
    {
        public string CheckName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPassed { get; set; }
        public bool IsWarning { get; set; }
        public string? FailureMessage { get; set; }

        // Computed properties for UI
        public string StatusIcon => IsPassed ? "ri-checkbox-circle-fill" : (IsWarning ? "ri-error-warning-fill" : "ri-close-circle-fill");
        public string StatusColor => IsPassed ? "success" : (IsWarning ? "warning" : "danger");
        public string StatusText => IsPassed ? "Passed" : (IsWarning ? "Warning" : "Failed");
    }
}
