using System.ComponentModel.DataAnnotations;

namespace FormReporting.Models.ViewModels.Forms
{
    /// <summary>
    /// ViewModel for creating and editing option templates
    /// </summary>
    public class OptionTemplateEditViewModel
    {
        public int TemplateId { get; set; }

        [Required(ErrorMessage = "Template name is required")]
        [StringLength(100)]
        [Display(Name = "Template Name")]
        public string TemplateName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Template code is required")]
        [StringLength(50)]
        [Display(Name = "Template Code")]
        [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Template code must be uppercase letters, numbers, and underscores only")]
        public string TemplateCode { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Category")]
        public string? Category { get; set; }

        [StringLength(50)]
        [Display(Name = "Sub-Category")]
        public string? SubCategory { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;

        [StringLength(200)]
        [Display(Name = "Applicable Field Types")]
        public string? ApplicableFieldTypes { get; set; }

        [StringLength(500)]
        [Display(Name = "Recommended For")]
        public string? RecommendedFor { get; set; }

        [Display(Name = "Has Scoring")]
        public bool HasScoring { get; set; } = false;

        [StringLength(30)]
        [Display(Name = "Scoring Type")]
        public string? ScoringType { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Template items collection (for dynamic form fields)
        public List<OptionTemplateItemEditViewModel> Items { get; set; } = new List<OptionTemplateItemEditViewModel>();
    }

    /// <summary>
    /// ViewModel for individual template items (options)
    /// </summary>
    public class OptionTemplateItemEditViewModel
    {
        public int TemplateItemId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Option Value")]
        public string OptionValue { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Option Label")]
        public string OptionLabel { get; set; } = string.Empty;

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Score Value")]
        public decimal? ScoreValue { get; set; }

        [Display(Name = "Score Weight")]
        public decimal? ScoreWeight { get; set; } = 1.0m;

        [StringLength(50)]
        [Display(Name = "Icon Class")]
        public string? IconClass { get; set; }

        [StringLength(7)]
        [Display(Name = "Color Hint")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be in hex format (e.g., #28a745)")]
        public string? ColorHint { get; set; }

        [Display(Name = "Is Default")]
        public bool IsDefault { get; set; } = false;
    }
}
