using FormReporting.Models.ViewModels.Dashboard.Components.Composite;

namespace FormReporting.Services.Dashboard.Common
{
    /// <summary>
    /// Interface for component builder services
    /// Defines contract for building dashboard UI components
    /// </summary>
    public interface IComponentBuilder
    {
        StatCardConfig BuildStatCard(
            string title, 
            string value, 
            string icon, 
            string iconColor, 
            string trendValue = "", 
            string trendDirection = "neutral", 
            string subText = "", 
            string linkUrl = "", 
            string linkText = "");
    }
}
