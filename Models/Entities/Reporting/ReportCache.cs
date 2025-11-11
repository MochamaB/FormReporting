using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Reporting
{
    [Table("ReportCache")]
    public class ReportCache
    {
        [Key]
        public long CacheId { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        [StringLength(64)]
        public string ParameterHash { get; set; } = string.Empty;

        public string? Parameters { get; set; }

        [Required]
        public string ResultData { get; set; } = string.Empty;

        [Required]
        public int RowCount { get; set; }

        public int? ColumnCount { get; set; }

        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        public int? GeneratedBy { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public int HitCount { get; set; } = 0;

        public DateTime? LastAccessDate { get; set; }

        public int? GenerationTimeMs { get; set; }

        [ForeignKey(nameof(ReportId))]
        public virtual ReportDefinition Report { get; set; } = null!;

        [ForeignKey(nameof(GeneratedBy))]
        public virtual User? Generator { get; set; }
    }
}
