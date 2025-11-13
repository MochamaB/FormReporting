using System.ComponentModel.DataAnnotations;

namespace FormReporting.Models.ViewModels.Organizational
{
    /// <summary>
    /// View model for creating a new tenant through multi-step wizard
    /// Follows WF-1.10: Tenant Creation Wizard (4 Steps)
    /// </summary>
    public class TenantCreateViewModel
    {
        // ============================================================================
        // STEP 1: BASIC DETAILS
        // ============================================================================

        [Required(ErrorMessage = "Tenant code is required")]
        [StringLength(50, ErrorMessage = "Tenant code cannot exceed 50 characters")]
        [Display(Name = "Tenant Code")]
        public string TenantCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tenant name is required")]
        [StringLength(200, ErrorMessage = "Tenant name cannot exceed 200 characters")]
        [Display(Name = "Tenant Name")]
        public string TenantName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tenant type is required")]
        [Display(Name = "Tenant Type")]
        public string TenantType { get; set; } = "Factory";

        [Display(Name = "Region")]
        public int? RegionId { get; set; }

        [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters")]
        [Display(Name = "Location/Address")]
        public string? Location { get; set; }

        [Display(Name = "Latitude")]
        public decimal? Latitude { get; set; }

        [Display(Name = "Longitude")]
        public decimal? Longitude { get; set; }

        [StringLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [Display(Name = "Contact Phone")]
        public string? ContactPhone { get; set; }

        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Contact Email")]
        public string? ContactEmail { get; set; }

        // ============================================================================
        // STEP 2: DEPARTMENTS
        // ============================================================================

        /// <summary>
        /// List of departments to create with the tenant
        /// Defaults: General, ICT, Finance, Operations
        /// </summary>
        public List<DepartmentCreateModel> Departments { get; set; } = new();

        /// <summary>
        /// Should we create default departments?
        /// </summary>
        public bool CreateDefaultDepartments { get; set; } = true;

        // ============================================================================
        // STEP 3: TENANT GROUPS (OPTIONAL)
        // ============================================================================

        /// <summary>
        /// IDs of TenantGroups to add this tenant to
        /// </summary>
        public List<int> SelectedGroupIds { get; set; } = new();

        // ============================================================================
        // STEP 4: SUMMARY (Read-only, no new properties)
        // ============================================================================

        // ============================================================================
        // VALIDATION HELPERS
        // ============================================================================

        /// <summary>
        /// Validates Step 1 (Basic Details)
        /// </summary>
        public List<string> ValidateStep1()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(TenantCode))
                errors.Add("Tenant code is required");

            if (string.IsNullOrWhiteSpace(TenantName))
                errors.Add("Tenant name is required");

            if (string.IsNullOrWhiteSpace(TenantType))
                errors.Add("Tenant type is required");

            // Business Rule: If Factory → RegionId REQUIRED
            if (TenantType?.ToLower() == "factory" && !RegionId.HasValue)
                errors.Add("Region is required for Factory type tenants");

            // Business Rule: If HeadOffice → RegionId MUST BE NULL
            if (TenantType?.ToLower() == "headoffice" && RegionId.HasValue)
                errors.Add("Head Office cannot be assigned to a region");

            // Validate TenantType enum
            var validTypes = new[] { "factory", "headoffice", "subsidiary" };
            if (!string.IsNullOrWhiteSpace(TenantType) && !validTypes.Contains(TenantType.ToLower()))
                errors.Add("Tenant type must be Factory, HeadOffice, or Subsidiary");

            return errors;
        }

        /// <summary>
        /// Validates Step 2 (Departments)
        /// </summary>
        public List<string> ValidateStep2()
        {
            var errors = new List<string>();

            if (!CreateDefaultDepartments && (Departments == null || !Departments.Any()))
                errors.Add("At least one department must be created");

            if (Departments != null && Departments.Any())
            {
                // Check for duplicate department codes
                var duplicates = Departments
                    .GroupBy(d => d.DepartmentCode?.ToLower())
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicates.Any())
                    errors.Add($"Duplicate department codes: {string.Join(", ", duplicates)}");

                // Validate each department
                foreach (var dept in Departments)
                {
                    if (string.IsNullOrWhiteSpace(dept.DepartmentCode))
                        errors.Add("All departments must have a code");

                    if (string.IsNullOrWhiteSpace(dept.DepartmentName))
                        errors.Add("All departments must have a name");
                }
            }

            return errors;
        }

        /// <summary>
        /// Validates Step 3 (Groups) - Always valid (optional step)
        /// </summary>
        public List<string> ValidateStep3()
        {
            // Groups are optional, no validation needed
            return new List<string>();
        }

        /// <summary>
        /// Initialize default departments for Step 2
        /// Called when wizard loads Step 2
        /// </summary>
        public void InitializeDefaultDepartments()
        {
            if (CreateDefaultDepartments && !Departments.Any())
            {
                Departments = new List<DepartmentCreateModel>
                {
                    new DepartmentCreateModel
                    {
                        DepartmentCode = "GEN",
                        DepartmentName = "General",
                        IsActive = true
                    },
                    new DepartmentCreateModel
                    {
                        DepartmentCode = "ICT",
                        DepartmentName = "ICT Department",
                        IsActive = true
                    },
                    new DepartmentCreateModel
                    {
                        DepartmentCode = "FIN",
                        DepartmentName = "Finance Department",
                        IsActive = true
                    },
                    new DepartmentCreateModel
                    {
                        DepartmentCode = "OPS",
                        DepartmentName = "Operations Department",
                        IsActive = true
                    }
                };
            }
        }
    }

    /// <summary>
    /// Helper model for creating departments during tenant creation
    /// </summary>
    public class DepartmentCreateModel
    {
        [Required]
        [StringLength(50)]
        public string DepartmentCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string DepartmentName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Can this department be removed? (default departments cannot)
        /// </summary>
        public bool IsRemovable { get; set; } = true;
    }
}
