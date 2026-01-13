namespace FormReporting.Models.ViewModels.Dashboard.Common
{
    /// <summary>
    /// Configuration for dashboard section layout and loading behavior
    /// Used in config-driven dashboard rendering
    /// </summary>
    public class DashboardSectionConfig
    {
        public string SectionId { get; set; } = string.Empty;
        public string SectionType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Component { get; set; } = string.Empty;
        public string Width { get; set; } = "col-12";
        public int Order { get; set; } = 0;
        public string LoadMethod { get; set; } = "Server";
        public int? RefreshInterval { get; set; }
        public string AjaxUrl { get; set; } = string.Empty;
    }
}
