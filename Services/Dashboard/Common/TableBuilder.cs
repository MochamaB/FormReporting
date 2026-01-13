using FormReporting.Models.ViewModels.Dashboard.Components.Composite;

namespace FormReporting.Services.Dashboard.Common
{
    /// <summary>
    /// Builder service for creating table configurations
    /// Handles column definitions, data formatting, and table features
    /// </summary>
    public class TableBuilder
    {
        /// <summary>
        /// Build a table configuration with columns and data
        /// </summary>
        public DataTableConfig BuildTable(
            string title, 
            List<TableColumn> columns, 
            List<Dictionary<string, object>> rows, 
            bool paginated = true, 
            int pageSize = 10)
        {
            return new DataTableConfig
            {
                Title = title,
                Columns = columns,
                Rows = rows,
                ShowSearch = true,
                ShowPagination = paginated,
                PageSize = pageSize,
                TotalItems = rows.Count,
                ShowCard = true
            };
        }
        
        /// <summary>
        /// Create a standard table column
        /// </summary>
        public TableColumn CreateColumn(
            string field, 
            string header, 
            string alignment = "start", 
            bool sortable = true,
            bool renderAsBadge = false)
        {
            return new TableColumn
            {
                Field = field,
                Header = header,
                Alignment = alignment,
                Sortable = sortable,
                RenderAsBadge = renderAsBadge
            };
        }
        
        /// <summary>
        /// Convert object list to table row format
        /// </summary>
        public Dictionary<string, object> ConvertToRow(Dictionary<string, object> data)
        {
            return new Dictionary<string, object>(data);
        }
        
        /// <summary>
        /// Format value based on column format type
        /// </summary>
        public string FormatValue(object value, string format)
        {
            if (value == null) return string.Empty;
            
            return format.ToLower() switch
            {
                "date" => value is DateTime dt ? dt.ToString("MMM dd, yyyy") : value.ToString() ?? string.Empty,
                "number" => value is int || value is decimal || value is double 
                    ? string.Format("{0:N0}", value) 
                    : value.ToString() ?? string.Empty,
                "currency" => value is decimal d 
                    ? d.ToString("C2") 
                    : value.ToString() ?? string.Empty,
                "percentage" => value is decimal p 
                    ? $"{p:F1}%" 
                    : value.ToString() ?? string.Empty,
                "badge" => value.ToString() ?? string.Empty,
                "link" => value.ToString() ?? string.Empty,
                _ => value.ToString() ?? string.Empty
            };
        }
    }
}
