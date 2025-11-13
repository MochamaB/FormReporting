namespace FormReporting.Models.ViewModels.Organizational
{
    /// <summary>
    /// View model for displaying tenant information in lists and grids
    /// </summary>
    public class TenantViewModel
    {
        public int TenantId { get; set; }
        public string TenantCode { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string TenantType { get; set; } = string.Empty;
        public string? RegionName { get; set; }
        public string? Location { get; set; }
        public int DepartmentCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        // Computed properties
        public string StatusBadge => IsActive
            ? "<span class='badge bg-success-subtle text-success'>Active</span>"
            : "<span class='badge bg-danger-subtle text-danger'>Inactive</span>";

        public string CreatedDateFormatted => CreatedDate.ToString("MMM dd, yyyy");
        public string ModifiedDateFormatted => ModifiedDate.ToString("MMM dd, yyyy");

        public string TenantTypeBadge => TenantType.ToLower() switch
        {
            "headoffice" => "<span class='badge bg-primary-subtle text-primary'><i class='ri-building-4-line me-1'></i>Head Office</span>",
            "factory" => "<span class='badge bg-success-subtle text-success'><i class='ri-factory-line me-1'></i>Factory</span>",
            "subsidiary" => "<span class='badge bg-info-subtle text-info'><i class='ri-building-2-line me-1'></i>Subsidiary</span>",
            _ => $"<span class='badge bg-secondary-subtle text-secondary'>{TenantType}</span>"
        };
    }
}
