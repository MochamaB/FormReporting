namespace FormReporting.Models.ViewModels.Forms
{
    public class SubmissionScoreBreakdownViewModel
    {
        public int SubmissionId { get; set; }
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public DateTime? SubmittedDate { get; set; }
        public decimal? OverallScore { get; set; }
        public List<SectionScoreViewModel> SectionScores { get; set; } = new List<SectionScoreViewModel>();
        public List<FieldScoreViewModel> FieldScores { get; set; } = new List<FieldScoreViewModel>();
    }

    public class SectionScoreViewModel
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public decimal? Score { get; set; }
        public decimal Weight { get; set; }
    }

    public class FieldScoreViewModel
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public decimal? Score { get; set; }
        public decimal Weight { get; set; }
    }

    public class FieldPerformanceViewModel
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public int ResponseCount { get; set; }
        public decimal? AverageScore { get; set; }
        public decimal? MinScore { get; set; }
        public decimal? MaxScore { get; set; }
        public decimal Weight { get; set; }
    }
}
