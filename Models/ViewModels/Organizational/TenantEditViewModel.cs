using System.ComponentModel.DataAnnotations;

namespace FormReporting.Models.ViewModels.Organizational
{
    /// <summary>
    /// ViewModel for editing existing tenants
    /// Separate from TenantCreateViewModel to handle existing data and relationships
    /// </summary>
    public class TenantEditViewModel
    {
        // ============================================================================
        // TENANT BASIC DETAILS
        // ============================================================================

        [Required]
        public int TenantId { get; set; }

        [Required(ErrorMessage = "Tenant code is required")]
        [StringLength(10, ErrorMessage = "Tenant code cannot exceed 10 characters")]
        public string TenantCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tenant name is required")]
        [StringLength(100, ErrorMessage = "Tenant name cannot exceed 100 characters")]
        public string TenantName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tenant type is required")]
        public string TenantType { get; set; } = "Factory";

        public int? RegionId { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        [Phone]
        public string? ContactPhone { get; set; }

        [EmailAddress]
        public string? ContactEmail { get; set; }

        public bool IsActive { get; set; } = true;

        // ============================================================================
        // TENANT GROUPS
        // ============================================================================

        /// <summary>
        /// List of currently assigned group IDs
        /// </summary>
        public List<int> SelectedGroupIds { get; set; } = new();

        // ============================================================================
        // METADATA (read-only)
        // ============================================================================

        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public string? ModifiedByName { get; set; }

        // ============================================================================
        // VALIDATION METHODS
        // ============================================================================

        /// <summary>
        /// Validate basic details (used in controller before save)
        /// </summary>
        public List<string> ValidateBasicDetails()
        {
            var errors = new List<string>();

            // Business Rule: If Factory → RegionId REQUIRED
            if (TenantType?.ToLower() == "factory" && !RegionId.HasValue)
            {
                errors.Add("Region is required for Factory type tenants");
            }

            // Business Rule: If HeadOffice → RegionId MUST BE NULL
            if (TenantType?.ToLower() == "headoffice" && RegionId.HasValue)
            {
                errors.Add("Head Office cannot be assigned to a region");
            }

            return errors;
        }
    }
}
