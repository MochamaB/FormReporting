namespace FormReporting.Models.ViewModels.Forms
{
    public class OnTimeStatisticsViewModel
    {
        public int TotalSubmissions { get; set; }
        public int OnTimeCount { get; set; }
        public int LateCount { get; set; }
        public decimal OnTimePercentage { get; set; }
        public decimal LatePercentage { get; set; }
    }

    public class SubmissionTrendDataPoint
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class TemplateStatisticsDashboardViewModel
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public int TotalSubmissions { get; set; }
        public Dictionary<string, int> StatusBreakdown { get; set; } = new Dictionary<string, int>();
        public OnTimeStatisticsViewModel OnTimeStatistics { get; set; } = new OnTimeStatisticsViewModel();
        public double? AverageCompletionTimeHours { get; set; }
        public List<SubmissionTrendDataPoint> TrendData { get; set; } = new List<SubmissionTrendDataPoint>();
        public List<SubmissionSummaryViewModel> RecentSubmissions { get; set; } = new List<SubmissionSummaryViewModel>();
    }

    public class TenantSubmissionStatistics
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public int TotalSubmissions { get; set; }
        public int SubmittedCount { get; set; }
        public int DraftCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
    }

    public class UserSubmissionStatistics
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TotalSubmissions { get; set; }
        public int SubmittedCount { get; set; }
        public int DraftCount { get; set; }
        public int ApprovedCount { get; set; }
    }

    public class SubmissionSummaryViewModel
    {
        public int SubmissionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = string.Empty;
        public DateTime? SubmittedDate { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public DateTime ReportingPeriod { get; set; }
    }
}
