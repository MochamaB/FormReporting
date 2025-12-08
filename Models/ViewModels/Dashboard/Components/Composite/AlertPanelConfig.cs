namespace FormReporting.Models.ViewModels.Dashboard.Components.Composite
{
    /// <summary>
    /// TIER 2 COMPOSITE: Alert panel
    /// Alert/notification panel with dismiss and action buttons
    /// </summary>
    public class AlertPanelConfig
    {
        /// <summary>
        /// Alert identifier
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Alert title/heading
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Alert message/content
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Alert type: primary, success, danger, warning, info, secondary
        /// </summary>
        public string AlertType { get; set; } = "info";

        /// <summary>
        /// Alert variant: Standard, Solid, BorderLeft, Modern
        /// </summary>
        public AlertVariant Variant { get; set; } = AlertVariant.Standard;

        /// <summary>
        /// Icon class (optional)
        /// </summary>
        public string? IconClass { get; set; }

        /// <summary>
        /// Show dismiss button
        /// </summary>
        public bool Dismissible { get; set; } = true;

        /// <summary>
        /// Action buttons
        /// </summary>
        public List<AlertAction>? Actions { get; set; }

        /// <summary>
        /// Additional CSS classes
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Helper: Create info alert
        /// </summary>
        public static AlertPanelConfig Info(string message, string? title = null, bool dismissible = true)
        {
            return new AlertPanelConfig
            {
                Title = title,
                Message = message,
                AlertType = "info",
                IconClass = "ri-information-line",
                Dismissible = dismissible
            };
        }

        /// <summary>
        /// Helper: Create success alert
        /// </summary>
        public static AlertPanelConfig Success(string message, string? title = null, bool dismissible = true)
        {
            return new AlertPanelConfig
            {
                Title = title,
                Message = message,
                AlertType = "success",
                IconClass = "ri-checkbox-circle-line",
                Dismissible = dismissible
            };
        }

        /// <summary>
        /// Helper: Create warning alert
        /// </summary>
        public static AlertPanelConfig Warning(string message, string? title = null, bool dismissible = true)
        {
            return new AlertPanelConfig
            {
                Title = title,
                Message = message,
                AlertType = "warning",
                IconClass = "ri-alert-line",
                Dismissible = dismissible
            };
        }

        /// <summary>
        /// Helper: Create danger/error alert
        /// </summary>
        public static AlertPanelConfig Danger(string message, string? title = null, bool dismissible = true)
        {
            return new AlertPanelConfig
            {
                Title = title,
                Message = message,
                AlertType = "danger",
                IconClass = "ri-error-warning-line",
                Dismissible = dismissible
            };
        }
    }

    /// <summary>
    /// Alert visual variant
    /// </summary>
    public enum AlertVariant
    {
        /// <summary>Standard Bootstrap alert</summary>
        Standard,
        /// <summary>Solid background</summary>
        Solid,
        /// <summary>Border on left side</summary>
        BorderLeft,
        /// <summary>Modern style with icon</summary>
        Modern
    }

    /// <summary>
    /// Alert action button
    /// </summary>
    public class AlertAction
    {
        /// <summary>
        /// Button text
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Button URL
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// JavaScript onclick handler
        /// </summary>
        public string? OnClick { get; set; }

        /// <summary>
        /// Button CSS class
        /// </summary>
        public string ButtonClass { get; set; } = "btn btn-sm btn-primary";
    }
}
