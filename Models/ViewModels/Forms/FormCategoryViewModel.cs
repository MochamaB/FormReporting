using System;
using System.ComponentModel.DataAnnotations;

namespace FormReporting.Models.ViewModels.Forms
{
    /// <summary>
    /// View model for displaying and managing form categories
    /// </summary>
    public class FormCategoryViewModel
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category code is required")]
        [StringLength(50, ErrorMessage = "Category code cannot exceed 50 characters")]
        [Display(Name = "Category Code")]
        public string CategoryCode { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Icon")]
        [StringLength(50)]
        public string? IconClass { get; set; }

        [Display(Name = "Color")]
        [StringLength(20)]
        public string? Color { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        // Computed properties
        [Display(Name = "Form Count")]
        public int FormCount { get; set; }

        [Display(Name = "Status")]
        public string StatusBadge => IsActive 
            ? "<span class='badge bg-success'>Active</span>" 
            : "<span class='badge bg-danger'>Inactive</span>";

        [Display(Name = "Icon Preview")]
        public string IconPreview => !string.IsNullOrEmpty(IconClass) 
            ? $"<i class='{IconClass}'></i>" 
            : "<i class='ri-folder-line'></i>";

        [Display(Name = "Color Badge")]
        public string ColorBadge => !string.IsNullOrEmpty(Color) 
            ? $"<span class='badge' style='background-color: {Color}'>{Color}</span>" 
            : "<span class='badge bg-secondary'>None</span>";
    }

    /// <summary>
    /// View model for creating/editing form categories
    /// </summary>
    public class FormCategoryEditViewModel
    {
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category code is required")]
        [StringLength(50, ErrorMessage = "Category code cannot exceed 50 characters")]
        [Display(Name = "Category Code")]
        public string CategoryCode { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Icon Class")]
        [StringLength(50)]
        public string? IconClass { get; set; }

        [Display(Name = "Color (Hex)")]
        [StringLength(20)]
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Please enter a valid hex color (e.g., #FF5733)")]
        public string? Color { get; set; }

        [Display(Name = "Display Order")]
        [Range(0, 9999, ErrorMessage = "Display order must be between 0 and 9999")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// View model for the form categories index page
    /// </summary>
    public class FormCategoriesIndexViewModel
    {
        public List<FormCategoryViewModel> Categories { get; set; } = new();
        
        // Statistics (for future use)
        public int TotalCategories { get; set; }
        public int ActiveCategories { get; set; }
        public int InactiveCategories { get; set; }
        public int TotalForms { get; set; }
    }

    /// <summary>
    /// View model for form category details page with template lists and pagination
    /// </summary>
    public class FormCategoryDetailsViewModel
    {
        // ============================================================================
        // CATEGORY DETAILS (reuse existing FormCategoryViewModel properties)
        // ============================================================================
        
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconClass { get; set; }
        public string? Color { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        
        // ============================================================================
        // TEMPLATE STATISTICS
        // ============================================================================
        
        public int TotalTemplates { get; set; }
        public int PublishedTemplates { get; set; }
        public int DraftTemplates { get; set; }
        public int ArchivedTemplates { get; set; }
        
        // ============================================================================
        // CURRENT TAB DATA (server-side loaded templates)
        // ============================================================================
        
        public string ActiveTab { get; set; } = "published";
        public List<FormTemplateViewModel> Templates { get; set; } = new();
        
        // ============================================================================
        // PAGINATION & FILTERING
        // ============================================================================
        
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalRecords { get; set; } = 0;
        public int PageSize { get; set; } = 10;
        public string? CurrentSearch { get; set; }
        public string? CurrentStatus { get; set; }
        
        // ============================================================================
        // COMPUTED PROPERTIES
        // ============================================================================
        
        /// <summary>
        /// Status badge for the category
        /// </summary>
        public string StatusBadge => IsActive 
            ? "<span class='badge bg-success-subtle text-success'>Active</span>" 
            : "<span class='badge bg-danger-subtle text-danger'>Inactive</span>";
        
        /// <summary>
        /// Icon preview HTML
        /// </summary>
        public string IconPreview => !string.IsNullOrEmpty(IconClass) 
            ? $"<i class='{IconClass}'></i>" 
            : "<i class='ri-folder-line'></i>";
        
        /// <summary>
        /// Summary statistics badges
        /// </summary>
        public string StatisticsBadges => $@"
            <span class='badge bg-primary-subtle text-primary me-1'>{TotalTemplates} Total</span>
            <span class='badge bg-success-subtle text-success me-1'>{PublishedTemplates} Published</span>
            <span class='badge bg-warning-subtle text-warning me-1'>{DraftTemplates} Draft</span>
            <span class='badge bg-secondary-subtle text-secondary'>{ArchivedTemplates} Archived</span>
        ";
    }
}
