namespace FormReporting.Models.ViewModels.Dashboard
{
    /// <summary>
    /// Widget position configuration for fixed/mixed layout modes
    /// </summary>
    public class WidgetPositionViewModel
    {
        /// <summary>
        /// Row position (1-based) for fixed placement
        /// </summary>
        public int? Row { get; set; }

        /// <summary>
        /// Column position (1-based) for fixed placement
        /// </summary>
        public int? Column { get; set; }

        /// <summary>
        /// Number of columns the widget spans (overrides WidgetSize if set)
        /// </summary>
        public int? ColSpan { get; set; }

        /// <summary>
        /// Number of rows the widget spans (for tall widgets)
        /// </summary>
        public int? RowSpan { get; set; }

        /// <summary>
        /// Display order for auto layout (lower numbers first)
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        /// Whether this widget has a fixed position (vs auto-placed)
        /// </summary>
        public bool IsFixed => Row.HasValue && Column.HasValue;
    }
}
