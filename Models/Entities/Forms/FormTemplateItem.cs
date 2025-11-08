using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormReporting.Models.Entities.Forms
{
    /// <summary>
    /// Individual form fields/questions within sections with advanced features
    /// </summary>
    [Table("FormTemplateItems")]
    public class FormTemplateItem
    {
        [Key]
        public int ItemId { get; set; }

        [Required]
        public int TemplateId { get; set; }

        [Required]
        public int SectionId { get; set; }

        [Required]
        [StringLength(50)]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? ItemDescription { get; set; }

        public int DisplayOrder { get; set; } = 0;

        [StringLength(50)]
        public string? DataType { get; set; } // Text, Number, Boolean, Date, Dropdown, TextArea, Rating, MultiSelect, FileUpload, Signature

        public bool IsRequired { get; set; } = false;

        [StringLength(500)]
        public string? DefaultValue { get; set; }

        // UI Enhancement Fields
        [StringLength(200)]
        public string? PlaceholderText { get; set; } // Hint text shown in empty field

        [StringLength(1000)]
        public string? HelpText { get; set; } // Explanation text below field

        [StringLength(50)]
        public string? PrefixText { get; set; } // Text before input (e.g., "$", "KES")

        [StringLength(50)]
        public string? SuffixText { get; set; } // Text after input (e.g., "%", "kg")

        // Conditional Logic
        public string? ConditionalLogic { get; set; } // JSON: {"action": "show", "rules": [{"itemId": 45, "operator": "equals", "value": "Yes"}]}

        // Matrix/Grid Layout Support
        [StringLength(30)]
        public string LayoutType { get; set; } = "Single"; // Single, Matrix, Grid, Inline

        public int? MatrixGroupId { get; set; } // Groups items in same matrix

        [StringLength(200)]
        public string? MatrixRowLabel { get; set; } // Row label in matrix layout

        // Field Library Integration
        public int? LibraryFieldId { get; set; } // Link to reusable field definition

        public bool IsLibraryOverride { get; set; } = false; // True if admin customized a library field

        public int Version { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(TemplateId))]
        public virtual FormTemplate Template { get; set; } = null!;

        [ForeignKey(nameof(SectionId))]
        public virtual FormTemplateSection Section { get; set; } = null!;

        [ForeignKey(nameof(LibraryFieldId))]
        public virtual FieldLibrary? LibraryField { get; set; }

        public virtual ICollection<FormItemOption> Options { get; set; } = new List<FormItemOption>();
        public virtual ICollection<FormItemConfiguration> Configurations { get; set; } = new List<FormItemConfiguration>();
        public virtual ICollection<FormItemValidation> Validations { get; set; } = new List<FormItemValidation>();
        public virtual ICollection<FormItemCalculation> Calculations { get; set; } = new List<FormItemCalculation>();
        public virtual ICollection<FormItemMetricMapping> MetricMappings { get; set; } = new List<FormItemMetricMapping>();
        public virtual ICollection<FormTemplateResponse> Responses { get; set; } = new List<FormTemplateResponse>();
        public virtual ICollection<SectionRouting> Routings { get; set; } = new List<SectionRouting>();
    }
}
