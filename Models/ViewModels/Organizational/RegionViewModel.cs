namespace FormReporting.Models.ViewModels.Organizational
{
    /// <summary>
    /// ViewModel for displaying a single region in lists/tables
    /// </summary>
    public class RegionViewModel
    {
        public int RegionId { get; set; }
        public string RegionNumber { get; set; } = string.Empty;
        public string RegionCode { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public int TenantCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        
        // Computed properties
        public string StatusBadge => IsActive 
            ? "<span class='badge bg-success-subtle text-success'>Active</span>" 
            : "<span class='badge bg-danger-subtle text-danger'>Inactive</span>";
            
        public string CreatedDateFormatted => CreatedDate.ToString("MMM dd, yyyy");
        public string ModifiedDateFormatted => ModifiedDate?.ToString("MMM dd, yyyy") ?? "—";
    }
}
