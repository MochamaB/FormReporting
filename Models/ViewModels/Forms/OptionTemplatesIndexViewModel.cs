namespace FormReporting.Models.ViewModels.Forms
{
    /// <summary>
    /// ViewModel for Option Templates Index page
    /// Contains list of templates and filtering information
    /// </summary>
    public class OptionTemplatesIndexViewModel
    {
        // List of templates for datatable
        public IEnumerable<OptionTemplateViewModel> Templates { get; set; } = new List<OptionTemplateViewModel>();

        // Available categories for filtering
        public List<string> Categories { get; set; } = new List<string>();
    }

    /// <summary>
    /// Individual template data for table rows
    /// </summary>
    public class OptionTemplateViewModel
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateCode { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public string? Description { get; set; }
        public int ItemCount { get; set; }
        public int UsageCount { get; set; }
        public bool HasScoring { get; set; }
        public string? ScoringType { get; set; }
        public bool IsSystemTemplate { get; set; }
        public bool IsActive { get; set; }
        public string? TenantName { get; set; }
        public DateTime CreatedDate { get; set; }

        // Computed properties for rendering
        public string StatusBadge => IsActive
            ? "<span class=\"badge bg-success-subtle text-success\"><i class=\"ri-checkbox-circle-line me-1\"></i>Active</span>"
            : "<span class=\"badge bg-danger-subtle text-danger\"><i class=\"ri-close-circle-line me-1\"></i>Inactive</span>";

        public string CategoryBadge
        {
            get
            {
                if (string.IsNullOrEmpty(Category))
                    return "<span class=\"badge bg-secondary-subtle text-secondary\">Uncategorized</span>";

                var colorClass = Category switch
                {
                    "Rating" => "bg-primary-subtle text-primary",
                    "Agreement" => "bg-info-subtle text-info",
                    "Binary" => "bg-warning-subtle text-warning",
                    "Frequency" => "bg-success-subtle text-success",
                    "Custom" => "bg-purple-subtle text-purple",
                    _ => "bg-secondary-subtle text-secondary"
                };

                return $"<span class=\"badge {colorClass}\">{Category}</span>";
            }
        }

        public string TemplateTypeBadge => IsSystemTemplate
            ? "<span class=\"badge bg-primary-subtle text-primary\"><i class=\"ri-building-line me-1\"></i>System</span>"
            : "<span class=\"badge bg-info-subtle text-info\"><i class=\"ri-user-line me-1\"></i>Custom</span>";

        public string ScoringBadge => HasScoring
            ? $"<span class=\"badge bg-success-subtle text-success\"><i class=\"ri-star-line me-1\"></i>{ScoringType}</span>"
            : "<span class=\"badge bg-secondary-subtle text-secondary\">No Scoring</span>";
    }
}
