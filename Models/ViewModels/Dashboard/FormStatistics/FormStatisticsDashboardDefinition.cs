using FormReporting.Models.ViewModels.Dashboard.Common;

namespace FormReporting.Models.ViewModels.Dashboard.FormStatistics
{
    /// <summary>
    /// Dashboard definition for Form Submission Statistics
    /// Defines section layout, order, and loading behavior (config-driven approach)
    /// </summary>
    public class FormStatisticsDashboardDefinition
    {
        public string DashboardId => "form-statistics";
        public string Title => "Form Submission Statistics";
        
        public List<DashboardSectionConfig> GetSections()
        {
            return new List<DashboardSectionConfig>
            {
                new DashboardSectionConfig
                {
                    SectionId = "quick-stats",
                    SectionType = "StatCards",
                    Title = "Quick Statistics",
                    Component = "_StatCardGroup",
                    Width = "col-12",
                    Order = 1,
                    LoadMethod = "Server",
                    RefreshInterval = null
                },
                
                new DashboardSectionConfig
                {
                    SectionId = "trend-chart",
                    SectionType = "Chart",
                    Title = "Submission Trends",
                    Component = "_TrendChartPartial",
                    Width = "col-lg-8",
                    Order = 2,
                    LoadMethod = "Ajax",
                    AjaxUrl = "/Dashboard/FormStatistics/GetTrendChart",
                    RefreshInterval = null
                },
                
                new DashboardSectionConfig
                {
                    SectionId = "status-chart",
                    SectionType = "Chart",
                    Title = "Status Breakdown",
                    Component = "_StatusChartPartial",
                    Width = "col-lg-4",
                    Order = 3,
                    LoadMethod = "Ajax",
                    AjaxUrl = "/Dashboard/FormStatistics/GetStatusChart",
                    RefreshInterval = null
                },
                
                new DashboardSectionConfig
                {
                    SectionId = "recent-submissions",
                    SectionType = "Table",
                    Title = "Recent Submissions",
                    Component = "_DataTable",
                    Width = "col-12",
                    Order = 4,
                    LoadMethod = "Ajax",
                    AjaxUrl = "/Dashboard/FormStatistics/GetRecentSubmissions",
                    RefreshInterval = null
                },
                
                new DashboardSectionConfig
                {
                    SectionId = "tenant-comparison",
                    SectionType = "Table",
                    Title = "Tenant Comparison",
                    Component = "_DataTable",
                    Width = "col-12",
                    Order = 5,
                    LoadMethod = "Ajax",
                    AjaxUrl = "/Dashboard/FormStatistics/GetTenantComparison",
                    RefreshInterval = null
                }
            };
        }
    }
}
