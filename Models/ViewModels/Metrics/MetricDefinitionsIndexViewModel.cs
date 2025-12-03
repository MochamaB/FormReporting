namespace FormReporting.Models.ViewModels.Metrics
{
    /// <summary>
    /// ViewModel for Metric Definitions Index page
    /// Contains statistics and list of metrics
    /// </summary>
    public class MetricDefinitionsIndexViewModel
    {
        // Statistics for stat cards
        public int TotalMetrics { get; set; }
        public int KpiMetrics { get; set; }
        public int ActiveMetrics { get; set; }
        public int UserInputMetrics { get; set; }

        // List of metrics for datatable
        public IEnumerable<MetricDefinitionItemViewModel> Metrics { get; set; } = new List<MetricDefinitionItemViewModel>();

        // Filter options
        public List<string> Categories { get; set; } = new List<string>();
        public List<string> SourceTypes { get; set; } = new List<string>();
    }

    /// <summary>
    /// Individual metric data for table rows
    /// </summary>
    public class MetricDefinitionItemViewModel
    {
        public int MetricId { get; set; }
        public string MetricCode { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SourceType { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public bool IsKPI { get; set; }
        public bool IsActive { get; set; }
        public decimal? ThresholdGreen { get; set; }
        public decimal? ThresholdYellow { get; set; }
        public decimal? ThresholdRed { get; set; }
        public DateTime CreatedDate { get; set; }

        // Computed properties for rendering
        public string StatusBadge => IsActive
            ? "<span class=\"badge bg-success-subtle text-success\">Active</span>"
            : "<span class=\"badge bg-secondary-subtle text-secondary\">Inactive</span>";

        public string KpiBadge => IsKPI
            ? "<span class=\"badge bg-warning-subtle text-warning\"><i class=\"ri-star-fill\"></i> KPI</span>"
            : "<span class=\"badge bg-light text-muted\">Standard</span>";

        public string CategoryBadge => Category switch
        {
            "Hardware" => $"<span class=\"badge bg-primary-subtle text-primary\"><i class=\"ri-computer-line\"></i> {Category}</span>",
            "Software" => $"<span class=\"badge bg-info-subtle text-info\"><i class=\"ri-apps-line\"></i> {Category}</span>",
            "Network" => $"<span class=\"badge bg-success-subtle text-success\"><i class=\"ri-global-line\"></i> {Category}</span>",
            "Security" => $"<span class=\"badge bg-danger-subtle text-danger\"><i class=\"ri-shield-check-line\"></i> {Category}</span>",
            "Compliance" => $"<span class=\"badge bg-warning-subtle text-warning\"><i class=\"ri-file-shield-line\"></i> {Category}</span>",
            "Infrastructure" => $"<span class=\"badge bg-secondary-subtle text-secondary\"><i class=\"ri-server-line\"></i> {Category}</span>",
            _ => $"<span class=\"badge bg-light text-muted\">{Category}</span>"
        };

        public string SourceTypeBadge => SourceType switch
        {
            "UserInput" => "<span class=\"badge bg-primary-subtle text-primary\">User Input</span>",
            "SystemCalculated" => "<span class=\"badge bg-info-subtle text-info\">Calculated</span>",
            "ExternalSystem" => "<span class=\"badge bg-success-subtle text-success\">External</span>",
            "ComplianceTracking" => "<span class=\"badge bg-warning-subtle text-warning\">Compliance</span>",
            "AutomatedCheck" => "<span class=\"badge bg-secondary-subtle text-secondary\">Automated</span>",
            _ => $"<span class=\"badge bg-light text-muted\">{SourceType}</span>"
        };

        public string DataTypeBadge => DataType switch
        {
            "Integer" or "Decimal" => $"<code class=\"text-primary\">{DataType}</code>",
            "Percentage" => "<code class=\"text-success\">Percentage</code>",
            "Boolean" => "<code class=\"text-warning\">Boolean</code>",
            "Text" => "<code class=\"text-info\">Text</code>",
            _ => $"<code>{DataType}</code>"
        };

        public string ThresholdDisplay
        {
            get
            {
                if (!ThresholdGreen.HasValue && !ThresholdYellow.HasValue && !ThresholdRed.HasValue)
                    return "<span class=\"text-muted\">—</span>";

                return $"<small><span class=\"text-success\">G: {ThresholdGreen?.ToString() ?? "—"}</span> | " +
                       $"<span class=\"text-warning\">Y: {ThresholdYellow?.ToString() ?? "—"}</span> | " +
                       $"<span class=\"text-danger\">R: {ThresholdRed?.ToString() ?? "—"}</span></small>";
            }
        }
    }
}
