using FormReporting.Models.ViewModels.Components;
using System.Text.Json;

namespace FormReporting.Extensions
{
    /// <summary>
    /// Extension methods for building editable tables from configuration objects
    /// Handles all transformation logic - Views only provide data, Extensions build the structure
    /// Follows three-layer pattern: Config → Extension → ViewModel
    /// </summary>
    public static class EditableTableExtensions
    {
        /// <summary>
        /// Transforms EditableTableConfig into EditableTableViewModel
        /// Validates, sorts columns, applies defaults, transforms to render-ready ViewModels
        /// ALL logic happens here - partials receive fully prepared data with ZERO logic needed
        /// </summary>
        public static EditableTableViewModel BuildEditableTable(this EditableTableConfig config)
        {
            // 1. Validate
            if (config.Columns == null || !config.Columns.Any())
                throw new ArgumentException("Editable table must have at least one column");

            if (string.IsNullOrEmpty(config.TableId))
                config.TableId = $"editable-table-{Guid.NewGuid():N}";

            // 2. Sort columns by display order
            var sortedColumns = config.Columns.OrderBy(c => c.DisplayOrder).ToList();

            // 3. Transform Config Columns → ViewModel Columns
            var viewModelColumns = new List<EditableTableColumnViewModel>();

            foreach (var configColumn in sortedColumns)
            {
                var viewModelColumn = new EditableTableColumnViewModel
                {
                    PropertyName = configColumn.PropertyName,
                    Header = configColumn.Header,
                    ColumnTypeString = configColumn.ColumnType.ToString(),
                    Width = configColumn.Width,
                    IsRequired = configColumn.IsRequired,
                    Placeholder = configColumn.Placeholder,
                    DefaultValue = configColumn.DefaultValue,
                    SelectOptions = configColumn.SelectOptions,
                    InputCssClass = configColumn.InputCssClass ?? "form-control form-control-sm",
                    Pattern = configColumn.Pattern,
                    Min = configColumn.Min,
                    Max = configColumn.Max,
                    Step = configColumn.Step,
                    MaxLength = configColumn.MaxLength,
                    Sortable = configColumn.Sortable,
                    HelpText = configColumn.HelpText,
                    ConditionalDisplay = configColumn.ConditionalDisplay,
                    InputTypeAttribute = GetInputType(configColumn.ColumnType)
                };

                viewModelColumns.Add(viewModelColumn);
            }

            // 4. Create configuration JSON for JavaScript
            var jsConfig = new
            {
                tableId = config.TableId,
                itemIndexPropertyName = config.ItemIndexPropertyName,
                columns = viewModelColumns.Select(c => new
                {
                    propertyName = c.PropertyName,
                    columnType = c.ColumnTypeString,
                    isRequired = c.IsRequired,
                    defaultValue = c.DefaultValue,
                    min = c.Min,
                    max = c.Max,
                    step = c.Step,
                    maxLength = c.MaxLength,
                    pattern = c.Pattern,
                    selectOptions = c.SelectOptions,
                    conditionalDisplay = c.ConditionalDisplay
                }),
                allowReorder = config.AllowReorder,
                allowMultiSelect = config.AllowMultiSelect,
                allowSorting = config.AllowSorting,
                showRowNumbers = config.ShowRowNumbers,
                initialRowCount = config.InitialRowCount
            };

            var configJson = JsonSerializer.Serialize(jsConfig, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            // 5. Return fully prepared ViewModel
            return new EditableTableViewModel
            {
                TableId = config.TableId,
                ItemIndexPropertyName = config.ItemIndexPropertyName,
                Columns = viewModelColumns,
                Buttons = config.Buttons,
                AllowReorder = config.AllowReorder,
                ShowRowNumbers = config.ShowRowNumbers,
                AllowMultiSelect = config.AllowMultiSelect,
                AllowSorting = config.AllowSorting,
                EmptyMessage = config.EmptyMessage,
                InitialRowCount = config.InitialRowCount,
                TableCssClass = config.TableCssClass,
                WrapInCard = config.WrapInCard,
                CardTitle = config.CardTitle,
                ConfigJson = configJson
            };
        }

        /// <summary>
        /// Get HTML input type attribute based on column type
        /// </summary>
        private static string GetInputType(EditableColumnType columnType)
        {
            return columnType switch
            {
                EditableColumnType.Text => "text",
                EditableColumnType.Number => "number",
                EditableColumnType.Decimal => "number",
                EditableColumnType.Date => "date",
                EditableColumnType.Email => "email",
                EditableColumnType.Url => "url",
                EditableColumnType.Color => "color",
                EditableColumnType.Hidden => "hidden",
                _ => "text"
            };
        }

        // ========== Fluent API Methods ==========

        /// <summary>
        /// Enable multi-select for the table
        /// </summary>
        public static EditableTableConfig WithMultiSelect(this EditableTableConfig config)
        {
            config.AllowMultiSelect = true;
            config.Buttons.ShowDeleteSelectedButton = true;
            return config;
        }

        /// <summary>
        /// Enable sorting for the table
        /// </summary>
        public static EditableTableConfig WithSorting(this EditableTableConfig config)
        {
            config.AllowSorting = true;
            return config;
        }

        /// <summary>
        /// Enable drag-and-drop reordering
        /// </summary>
        public static EditableTableConfig WithReordering(this EditableTableConfig config)
        {
            config.AllowReorder = true;
            return config;
        }

        /// <summary>
        /// Wrap table in a card
        /// </summary>
        public static EditableTableConfig InCard(this EditableTableConfig config, string title)
        {
            config.WrapInCard = true;
            config.CardTitle = title;
            return config;
        }

        /// <summary>
        /// Set empty message
        /// </summary>
        public static EditableTableConfig WithEmptyMessage(this EditableTableConfig config, string message)
        {
            config.EmptyMessage = message;
            return config;
        }
    }
}
