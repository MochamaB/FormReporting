using FormReporting.Models.Common;
using FormReporting.Models.ViewModels.Dashboard;

namespace FormReporting.Services.Dashboard
{
    /// <summary>
    /// Registry containing all dashboard configurations
    /// Dashboards are defined here and looked up by key
    /// </summary>
    public static class DashboardRegistry
    {
        /// <summary>
        /// All registered dashboard configurations
        /// </summary>
        private static readonly Dictionary<string, DashboardConfig> _dashboards = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Static constructor to register all dashboards
        /// </summary>
        static DashboardRegistry()
        {
            // Phase 1: Test dashboard for verifying grid layout and widget loading states
            RegisterTestDashboard();

            // Phase 2 will add: RegisterFormStatisticsDashboard();
            // Phase 3 will add: RegisterFormScoringDashboard();
        }

        /// <summary>
        /// Test dashboard for Phase 1 verification only
        /// Verifies: grid layout, widget sizing, loading states
        /// </summary>
        private static void RegisterTestDashboard()
        {
            Register(new DashboardConfig
            {
                Key = "test-dashboard",
                Title = "Test Dashboard",
                Description = "Phase 1 verification - grid layout and widget loading states",
                Icon = "ri-test-tube-line",
                ContextType = ContextType.None,
                HasContextSelector = false,
                HasFilterBar = false,
                Layout = new DashboardLayoutConfig
                {
                    Columns = 12,
                    Mode = LayoutMode.Auto,
                    RowGap = "1rem",
                    ColumnGap = "1rem"
                },
                Widgets = new List<WidgetConfig>
                {
                    // Row 1: 4 StatCards (Small = 3 cols each)
                    new WidgetConfig { Key = "test-stat-1", Type = WidgetType.StatCard, Title = "Stat Card 1", Size = WidgetSize.Small, Order = 1 },
                    new WidgetConfig { Key = "test-stat-2", Type = WidgetType.StatCard, Title = "Stat Card 2", Size = WidgetSize.Small, Order = 2 },
                    new WidgetConfig { Key = "test-stat-3", Type = WidgetType.StatCard, Title = "Stat Card 3", Size = WidgetSize.Small, Order = 3 },
                    new WidgetConfig { Key = "test-stat-4", Type = WidgetType.StatCard, Title = "Stat Card 4", Size = WidgetSize.Small, Order = 4 },

                    // Row 2: Chart + Table (Large = 6 cols each)
                    new WidgetConfig { Key = "test-chart", Type = WidgetType.BarChart, Title = "Bar Chart", Size = WidgetSize.Large, Order = 5 },
                    new WidgetConfig { Key = "test-table", Type = WidgetType.DataTable, Title = "Data Table", Size = WidgetSize.Large, Order = 6 },

                    // Row 3: Full width
                    new WidgetConfig { Key = "test-full", Type = WidgetType.DataTable, Title = "Full Width Table", Size = WidgetSize.Full, Order = 7 }
                }
            });
        }

        /// <summary>
        /// Gets a dashboard configuration by key
        /// </summary>
        public static DashboardConfig? GetDashboard(string key)
        {
            return _dashboards.TryGetValue(key, out var config) ? config : null;
        }

        /// <summary>
        /// Gets all registered dashboards
        /// </summary>
        public static IEnumerable<DashboardConfig> GetAllDashboards()
        {
            return _dashboards.Values;
        }

        /// <summary>
        /// Checks if a dashboard exists
        /// </summary>
        public static bool Exists(string key)
        {
            return _dashboards.ContainsKey(key);
        }

        /// <summary>
        /// Registers a dashboard configuration
        /// </summary>
        public static void Register(DashboardConfig config)
        {
            _dashboards[config.Key] = config;
        }

        // ============================================================================
        // DASHBOARD REGISTRATION METHODS
        // Add new dashboard registrations here as they are implemented
        // ============================================================================

        /// <summary>
        /// Placeholder for Form Statistics Dashboard (Phase 2)
        /// </summary>
        private static void RegisterFormStatisticsDashboard()
        {
            Register(new DashboardConfig
            {
                Key = "form-statistics",
                Title = "Form Statistics",
                Description = "Overview of form submissions and completion rates",
                Icon = "ri-bar-chart-box-line",
                ContextType = ContextType.FormTemplate,
                HasContextSelector = true,
                HasFilterBar = true,
                Layout = new DashboardLayoutConfig
                {
                    Columns = 12,
                    Mode = LayoutMode.Auto,
                    RowGap = "1rem",
                    ColumnGap = "1rem"
                },
                Widgets = new List<WidgetConfig>
                {
                    // Widgets will be added in Phase 2
                }
            });
        }

        /// <summary>
        /// Placeholder for Form Scoring Dashboard (Phase 3)
        /// </summary>
        private static void RegisterFormScoringDashboard()
        {
            Register(new DashboardConfig
            {
                Key = "form-scoring",
                Title = "Form Scoring",
                Description = "Form scores, trends, and performance analysis",
                Icon = "ri-line-chart-line",
                ContextType = ContextType.FormTemplate,
                HasContextSelector = true,
                HasFilterBar = true,
                Layout = new DashboardLayoutConfig
                {
                    Columns = 12,
                    Mode = LayoutMode.Auto
                },
                Widgets = new List<WidgetConfig>
                {
                    // Widgets will be added in Phase 3
                }
            });
        }
    }

    /// <summary>
    /// Dashboard configuration (registry entry)
    /// </summary>
    public class DashboardConfig
    {
        /// <summary>
        /// Unique dashboard key
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Display title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Optional description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Icon class
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Context type for filtering
        /// </summary>
        public ContextType ContextType { get; set; } = ContextType.None;

        /// <summary>
        /// Whether to show context selector dropdown
        /// </summary>
        public bool HasContextSelector { get; set; }

        /// <summary>
        /// Whether to show filter bar
        /// </summary>
        public bool HasFilterBar { get; set; }

        /// <summary>
        /// Layout configuration
        /// </summary>
        public DashboardLayoutConfig Layout { get; set; } = new();

        /// <summary>
        /// Widget configurations for this dashboard
        /// </summary>
        public List<WidgetConfig> Widgets { get; set; } = new();

        /// <summary>
        /// Converts to view model (widgets in loading state)
        /// </summary>
        public DashboardViewModel ToViewModel()
        {
            return new DashboardViewModel
            {
                Key = Key,
                Title = Title,
                Description = Description,
                Icon = Icon,
                ContextType = ContextType,
                HasContextSelector = HasContextSelector,
                HasFilterBar = HasFilterBar,
                Layout = Layout.ToViewModel(),
                Widgets = Widgets.Select(w => w.ToViewModel()).ToList()
            };
        }
    }

    /// <summary>
    /// Layout configuration for registry
    /// </summary>
    public class DashboardLayoutConfig
    {
        public int Columns { get; set; } = 12;
        public LayoutMode Mode { get; set; } = LayoutMode.Auto;
        public string RowGap { get; set; } = "1rem";
        public string ColumnGap { get; set; } = "1rem";
        public string MinRowHeight { get; set; } = "100px";
        public bool UseCssGrid { get; set; } = false;

        public DashboardLayoutViewModel ToViewModel()
        {
            return new DashboardLayoutViewModel
            {
                Columns = Columns,
                Mode = Mode,
                RowGap = RowGap,
                ColumnGap = ColumnGap,
                MinRowHeight = MinRowHeight,
                UseCssGrid = UseCssGrid
            };
        }
    }

    /// <summary>
    /// Widget configuration for registry
    /// </summary>
    public class WidgetConfig
    {
        /// <summary>
        /// Unique widget key (e.g., "total-submissions")
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Widget type
        /// </summary>
        public WidgetType Type { get; set; }

        /// <summary>
        /// Display title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Optional subtitle
        /// </summary>
        public string? Subtitle { get; set; }

        /// <summary>
        /// Widget size in grid
        /// </summary>
        public WidgetSize Size { get; set; } = WidgetSize.Medium;

        /// <summary>
        /// Display order
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Position configuration for fixed layouts
        /// </summary>
        public WidgetPositionConfig? Position { get; set; }

        /// <summary>
        /// Provider key that supplies data for this widget
        /// </summary>
        public string ProviderKey { get; set; } = string.Empty;

        /// <summary>
        /// Optional drill-down URL template
        /// Use {contextId} for context entity ID placeholder
        /// </summary>
        public string? DrillDownUrl { get; set; }

        /// <summary>
        /// Whether widget can be refreshed
        /// </summary>
        public bool CanRefresh { get; set; } = true;

        /// <summary>
        /// CSS class for styling
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Converts to view model (loading state)
        /// </summary>
        public WidgetViewModel ToViewModel()
        {
            return new WidgetViewModel
            {
                Key = Key,
                Type = Type,
                Title = Title,
                Subtitle = Subtitle,
                Size = Size,
                Order = Order,
                Position = Position?.ToViewModel(),
                Status = WidgetStatus.Loading,
                CanRefresh = CanRefresh,
                DrillDownUrl = DrillDownUrl,
                CssClass = CssClass
            };
        }
    }

    /// <summary>
    /// Widget position configuration for registry
    /// </summary>
    public class WidgetPositionConfig
    {
        public int? Row { get; set; }
        public int? Column { get; set; }
        public int? ColSpan { get; set; }
        public int? RowSpan { get; set; }
        public int Order { get; set; }

        public WidgetPositionViewModel ToViewModel()
        {
            return new WidgetPositionViewModel
            {
                Row = Row,
                Column = Column,
                ColSpan = ColSpan,
                RowSpan = RowSpan,
                Order = Order
            };
        }
    }
}
