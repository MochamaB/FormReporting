using FormReporting.Models.Common;
using FormReporting.Models.ViewModels.Dashboard;

namespace FormReporting.Services.Dashboard
{
    /// <summary>
    /// Interface for widget data providers
    /// Each provider supplies data for specific widget types within a domain (Forms, Hardware, etc.)
    /// </summary>
    public interface IWidgetDataProvider
    {
        /// <summary>
        /// Unique identifier for this provider (e.g., "form", "hardware", "ticket")
        /// </summary>
        string ProviderKey { get; }

        /// <summary>
        /// Gets the widget keys this provider can handle
        /// </summary>
        IEnumerable<string> SupportedWidgets { get; }

        /// <summary>
        /// Checks if this provider can handle the specified widget
        /// </summary>
        /// <param name="widgetKey">The widget key to check</param>
        /// <returns>True if this provider handles the widget</returns>
        bool CanHandle(string widgetKey);

        /// <summary>
        /// Gets data for a specific widget
        /// </summary>
        /// <param name="widgetKey">The widget key</param>
        /// <param name="filters">Optional filters to apply</param>
        /// <param name="contextType">Optional context type for entity filtering</param>
        /// <param name="contextId">Optional context entity ID</param>
        /// <returns>Widget data object (cast to appropriate type based on widget)</returns>
        Task<object?> GetWidgetDataAsync(
            string widgetKey,
            DashboardFilterViewModel? filters = null,
            ContextType contextType = ContextType.None,
            int? contextId = null);

        /// <summary>
        /// Gets data for multiple widgets in a single call (for efficiency)
        /// </summary>
        /// <param name="widgetKeys">The widget keys to fetch</param>
        /// <param name="filters">Optional filters to apply</param>
        /// <param name="contextType">Optional context type for entity filtering</param>
        /// <param name="contextId">Optional context entity ID</param>
        /// <returns>Dictionary of widget key to data object</returns>
        Task<Dictionary<string, object?>> GetMultipleWidgetDataAsync(
            IEnumerable<string> widgetKeys,
            DashboardFilterViewModel? filters = null,
            ContextType contextType = ContextType.None,
            int? contextId = null);
    }

    /// <summary>
    /// Base class for widget data providers with common functionality
    /// </summary>
    public abstract class WidgetDataProviderBase : IWidgetDataProvider
    {
        /// <inheritdoc />
        public abstract string ProviderKey { get; }

        /// <inheritdoc />
        public abstract IEnumerable<string> SupportedWidgets { get; }

        /// <inheritdoc />
        public virtual bool CanHandle(string widgetKey)
        {
            return SupportedWidgets.Contains(widgetKey, StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public abstract Task<object?> GetWidgetDataAsync(
            string widgetKey,
            DashboardFilterViewModel? filters = null,
            ContextType contextType = ContextType.None,
            int? contextId = null);

        /// <inheritdoc />
        public virtual async Task<Dictionary<string, object?>> GetMultipleWidgetDataAsync(
            IEnumerable<string> widgetKeys,
            DashboardFilterViewModel? filters = null,
            ContextType contextType = ContextType.None,
            int? contextId = null)
        {
            var result = new Dictionary<string, object?>();

            // Default implementation fetches each widget individually
            // Providers can override for more efficient batch fetching
            foreach (var key in widgetKeys.Where(CanHandle))
            {
                try
                {
                    result[key] = await GetWidgetDataAsync(key, filters, contextType, contextId);
                }
                catch (Exception)
                {
                    result[key] = null;
                }
            }

            return result;
        }
    }
}
