using FormReporting.Models.ViewModels.Dashboard.Components.Composite;
using System.Text.Json;

namespace FormReporting.Services.Dashboard.Common
{
    /// <summary>
    /// Builder service for creating ApexCharts configurations
    /// Supports line, bar, pie, donut, and area charts
    /// </summary>
    public class ChartBuilder
    {
        /// <summary>
        /// Build a line chart configuration
        /// </summary>
        public ChartCardConfig BuildLineChart(
            string title, 
            List<string> labels, 
            List<ChartDataset> datasets, 
            int height = 350)
        {
            var series = datasets.Select(ds => new { name = ds.Name, data = ds.Data }).ToArray();
            
            return new ChartCardConfig
            {
                Title = title,
                ChartType = "line",
                CategoriesJson = JsonSerializer.Serialize(labels),
                SeriesJson = JsonSerializer.Serialize(series),
                Height = height,
                ShowHeader = true,
                ShowToolbar = true,
                Colors = GetDefaultColors()
            };
        }
        
        /// <summary>
        /// Build a bar chart configuration
        /// </summary>
        public ChartCardConfig BuildBarChart(
            string title, 
            List<string> labels, 
            List<ChartDataset> datasets, 
            int height = 350)
        {
            var series = datasets.Select(ds => new { name = ds.Name, data = ds.Data }).ToArray();
            
            return new ChartCardConfig
            {
                Title = title,
                ChartType = "bar",
                CategoriesJson = JsonSerializer.Serialize(labels),
                SeriesJson = JsonSerializer.Serialize(series),
                Height = height,
                ShowHeader = true,
                ShowToolbar = true,
                Colors = GetDefaultColors()
            };
        }
        
        /// <summary>
        /// Build a pie chart configuration
        /// </summary>
        public ChartCardConfig BuildPieChart(
            string title, 
            List<string> labels, 
            List<decimal> data, 
            int height = 350)
        {
            return new ChartCardConfig
            {
                Title = title,
                ChartType = "pie",
                CategoriesJson = JsonSerializer.Serialize(labels),
                SeriesJson = JsonSerializer.Serialize(data),
                Height = height,
                ShowHeader = true,
                ShowToolbar = false,
                Colors = GetDefaultColors()
            };
        }
        
        /// <summary>
        /// Build a donut chart configuration
        /// </summary>
        public ChartCardConfig BuildDonutChart(
            string title, 
            List<string> labels, 
            List<decimal> data, 
            int height = 350)
        {
            return new ChartCardConfig
            {
                Title = title,
                ChartType = "donut",
                CategoriesJson = JsonSerializer.Serialize(labels),
                SeriesJson = JsonSerializer.Serialize(data),
                Height = height,
                ShowHeader = true,
                ShowToolbar = false,
                Colors = GetDefaultColors()
            };
        }
        
        /// <summary>
        /// Get default chart colors (primary, success, warning, danger, info)
        /// </summary>
        public string[] GetDefaultColors()
        {
            return new string[]
            {
                "#405189", // primary blue
                "#0ab39c", // success green
                "#f7b84b", // warning orange
                "#f06548", // danger red
                "#299cdb"  // info cyan
            };
        }
    }
    
    /// <summary>
    /// Chart dataset for building charts
    /// </summary>
    public class ChartDataset
    {
        public string Name { get; set; } = string.Empty;
        public List<decimal> Data { get; set; } = new List<decimal>();
        public string? Color { get; set; }
    }
}
