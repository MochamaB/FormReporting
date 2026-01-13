# Dashboard Architecture Design

## 🎯 Core Architecture

### Dashboard Engine System
```
Dashboard Engine → Widget Registry → Data Adapters → Security Layer
```

**Key Components:**
- **DashboardEngine**: Central orchestrator
- **WidgetRegistry**: Plugin system for widgets
- **DataAdapter**: Pluggable data sources
- **SecurityFilter**: Automatic scope-based filtering

## 📁 Complete Folder Structure

```
Services/Dashboard/
├── Core/
│   ├── IDashboardEngine.cs
│   ├── DashboardEngine.cs
│   └── DashboardRegistry.cs
├── Filters/
│   ├── IFilterManager.cs
│   ├── FilterManager.cs
│   ├── IFilterStateStore.cs
│   └── FilterStateStore.cs
├── Widgets/
│   ├── IWidgetRegistry.cs
│   ├── WidgetRegistry.cs
│   ├── IWidgetRenderer.cs
│   ├── WidgetRenderer.cs
│   └── Widgets/
│       ├── ChartWidget.cs
│       ├── TableWidget.cs
│       ├── StatCardWidget.cs
│       └── FilterWidget.cs
├── Data/
│   ├── IDataAdapter.cs
│   ├── SqlDataAdapter.cs
│   ├── ApiDataAdapter.cs
│   ├── ClaimsDataAdapter.cs
│   └── AggregateDataAdapter.cs
├── Layouts/
│   ├── ILayoutManager.cs
│   ├── LayoutManager.cs
│   ├── GridLayoutManager.cs
│   └── TabLayoutManager.cs
├── Statistics/
│   ├── IFormSubmissionStatisticsService.cs
│   ├── FormSubmissionStatisticsService.cs (moved from Forms/)
│   ├── IFormScoreCalculationService.cs
│   ├── FormScoreCalculationService.cs (moved from Forms/)
│   └── StatisticsDataAdapter.cs
├── Security/
│   ├── IDashboardSecurity.cs
│   ├── DashboardSecurity.cs
│   ├── ISecurityFilter.cs
│   └── SecurityFilter.cs
├── Caching/
│   ├── ICacheManager.cs
│   ├── CacheManager.cs
│   ├── ICachePolicy.cs
│   └── CachePolicy.cs
└── RealTime/
    ├── IRealTimeManager.cs
    ├── RealTimeManager.cs
    ├── IRealTimeWidget.cs
    └── RealTimeWidget.cs

Models/ViewModels/Dashboard/
├── Core/                           # 🆕 Core dashboard models
│   ├── DashboardDefinition.cs
│   ├── WidgetDefinition.cs
│   ├── DataSourceDefinition.cs
│   └── SecurityDefinition.cs
├── Filters/                         # 🆕 Filter system
│   ├── FilterDefinition.cs
│   ├── FilterState.cs
│   ├── FilterDependency.cs
│   └── FilterResult.cs
├── Components/                      # ✅ Keep existing (enhanced)
│   ├── Atomic/                      # StatCard, Chart, Table, Filter
│   └── Composite/                   # DashboardConfig, DashboardSection, FilterPanelConfig
└── Layouts/                         # 🆕 Layout system
    ├── LayoutDefinition.cs
    ├── GridLayout.cs
    └── ResponsiveLayout.cs

Views/Dashboard/
├── Core/                           # 🆕 Core dashboard views
│   ├── _DashboardEngine.cshtml
│   ├── _WidgetContainer.cshtml
│   └── _FilterContainer.cshtml
├── Components/                     # ✅ Keep existing
│   ├── Atomic/
│   └── Composite/
├── Layouts/                        # 🆕 Layout views
│   ├── _GridLayout.cshtml
│   ├── _TabLayout.cshtml
│   └── _ResponsiveLayout.cshtml
└── Templates/                      # 🆕 Dashboard templates
    ├── FormStatistics.cshtml
    ├── Analytics.cshtml
    └── Reports.cshtml
```

## 🎨 Layout Manager System

### Grid-Based Layout Engine
```csharp
public interface ILayoutManager
{
    Task<LayoutResult> BuildLayoutAsync(LayoutRequest request);
    Task<LayoutSchema> GetLayoutSchemaAsync(string layoutKey);
}

public class LayoutManager : ILayoutManager
{
    public async Task<LayoutResult> BuildLayoutAsync(LayoutRequest request)
    {
        if (request.Context.EmbedMode == EmbedMode.Tab)
        {
            // Tab-optimized layout
            return new LayoutResult
            {
                Type = "tab-grid",
                Grid = new LayoutGrid
                {
                    Columns = 12,
                    RowHeight = 60,  // Smaller for tabs
                    Margin = [5, 5]  // Tighter spacing
                }
            };
        }
        
        // Standard full-page layout
        return await _gridLayoutManager.BuildLayoutAsync(request);
    }
}
```

### Integration with Current ViewModels
```csharp
// Enhanced DashboardConfig using current components
public class DashboardConfig
{
    public string Title { get; set; }
    public List<DashboardSection> Sections { get; set; }  // ✅ Current component
    public FilterPanelConfig FilterPanel { get; set; }    // ✅ Current component
    
    // 🆕 Layout integration
    public LayoutDefinition Layout { get; set; }
    public DashboardMetadata Metadata { get; set; }
    public RenderContext Context { get; set; }
}

// Enhanced DashboardSection using current components
public class DashboardSection
{
    public string Id { get; set; }
    public SectionComponentType Type { get; set; }        // ✅ Current enum
    public SectionLoadMethod LoadMethod { get; set; }     // ✅ Current enum
    public string AjaxUrl { get; set; }
    public object Data { get; set; }
    
    // 🆕 Widget integration
    public WidgetDefinition Widget { get; set; }
    public LayoutPosition Position { get; set; }
}
```

## 🔍 Filter System

### Filter Manager Pipeline
```csharp
public class FilterManager : IFilterManager
{
    public async Task<FilterResult> ApplyFiltersAsync(FilterRequest request)
    {
        // 1. Get filter definitions
        var filters = await _registry.GetFiltersAsync(request.DashboardKey);
        
        // 2. Apply security filtering
        var securedFilters = await _securityFilter.ApplySecurityAsync(filters, request.User);
        
        // 3. Process dependencies (region → tenant → template)
        var processedFilters = await ProcessFilterDependenciesAsync(securedFilters, request);
        
        // 4. Generate filtered data requests
        var dataRequests = GenerateDataRequests(processedFilters);
        
        return new FilterResult { Filters = processedFilters, DataRequests = dataRequests };
    }
}
```

### Filter Dependencies
```json
{
  "filters": [
    {
      "id": "region-filter",
      "affects": ["tenant-filter", "template-filter"]
    },
    {
      "id": "tenant-filter", 
      "dependencies": ["region-filter"],
      "affects": ["template-filter"]
    },
    {
      "id": "template-filter",
      "dependencies": ["region-filter", "tenant-filter"]
    }
  ]
}
```

## 📑 Partial Dashboard Integration

### Controller Usage
```csharp
public async Task<IActionResult> AvailableForms()
{
    var forms = await _formService.GetAvailableFormsAsync(User);
    
    // Dashboard automatically scoped to user
    var dashboardStats = await _dashboardEngine.RenderDashboardAsync(new DashboardRequest
    {
        DashboardKey = "form-statistics",
        User = User,
        Context = new RenderContext { IsPartial = true }
    });
    
    ViewBag.DashboardStats = dashboardStats;
    return View(forms);
}
```

### Razor View Integration
```html
<div class="tab-content">
    <div class="tab-pane fade show active" id="forms-tab">
        @await Html.PartialAsync("_FormsList", Model)
    </div>
    <div class="tab-pane fade" id="statistics-tab">
        @await Html.PartialAsync("_DashboardPartial", ViewBag.DashboardStats)
    </div>
</div>
```

## 🔐 Security & Performance System

### Multi-Level Security
```csharp
public interface IDashboardSecurity
{
    Task<bool> CanAccessDashboardAsync(string dashboardKey, ClaimsPrincipal user);
    Task<bool> CanViewWidgetAsync(string widgetId, ClaimsPrincipal user);
    Task<bool> CanAccessDataSourceAsync(string sourceKey, ClaimsPrincipal user);
    Task<SecurityContext> GetSecurityContextAsync(ClaimsPrincipal user);
}

public class SecurityFilter : ISecurityFilter
{
    public async Task<List<FilterDefinition>> ApplySecurityAsync(
        List<FilterDefinition> filters, ClaimsPrincipal user)
    {
        var securityContext = await _securityService.GetSecurityContextAsync(user);
        
        return filters.Where(f => 
            securityContext.AllowedFilters.Contains(f.Id) &&
            securityContext.RequiredRoles.All(r => user.IsInRole(r))
        ).ToList();
    }
}
```

### Multi-Tier Caching
```csharp
public interface ICacheManager
{
    Task<T> GetAsync<T>(string key, Func<Task<T>> factory);
    Task InvalidateAsync(string pattern);
    Task<CacheStats> GetStatsAsync();
}

public class CacheManager : ICacheManager
{
    public async Task<T> GetAsync<T>(string key, Func<Task<T>> factory)
    {
        // Multi-level caching: Memory → Redis → Database
        if (_memoryCache.TryGetValue(key, out T cached))
            return cached;
            
        var result = await factory();
        
        // Cache with policy
        await SetCacheAsync(key, result, GetCachePolicy(key));
        return result;
    }
}

public class CacheDefinition
{
    public CacheLevel Level { get; set; }              // "none", "user", "tenant", "global"
    public TimeSpan Duration { get; set; }
    public List<string> Dependencies { get; set; }
}
```

### Performance Optimization
```csharp
public class PerformanceDashboardEngine : IDashboardEngine
{
    private readonly ICacheManager _cache;
    private readonly IPerformanceMonitor _monitor;
    
    public async Task<DashboardRenderResult> RenderDashboardAsync(DashboardRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Check cache first
        var cacheKey = GenerateCacheKey(request);
        var cached = await _cache.GetAsync<DashboardRenderResult>(cacheKey, null);
        if (cached != null)
        {
            _monitor.RecordCacheHit(cacheKey, stopwatch.Elapsed);
            return cached;
        }
        
        // Render with performance tracking
        var result = await RenderWithTrackingAsync(request);
        
        // Cache result
        await _cache.SetAsync(cacheKey, result, GetCachePolicy(request));
        
        _monitor.RecordRenderTime(request.DashboardKey, stopwatch.Elapsed);
        return result;
    }
}
```

## 🔐 Automatic Scope Handling

### Data Layer Scope Filtering
```csharp
public class ClaimsDataAdapter : IDataAdapter
{
    public async Task<object> GetDataAsync(DataRequest request)
    {
        var userScope = await _scopeService.GetUserScopeAsync(request.User);
        
        // Automatic scope filtering
        var query = _context.FormTemplateSubmissions.AsQueryable();
        
        if (userScope.ScopeCode == "GLOBAL")
        {
            // Global user sees ALL data - no filtering
        }
        else if (userScope.ScopeCode == "REGIONAL")
        {
            query = query.Where(s => userScope.AccessibleRegionIds.Contains(s.Tenant.RegionId));
        }
        else if (userScope.ScopeCode == "TENANT")
        {
            query = query.Where(s => s.TenantId == userScope.PrimaryTenantId);
        }
        
        return await query.ToListAsync();
    }
}
```

**Result:**
- **Global User**: Sees ALL data across all tenants
- **Regional User**: Sees data from their regions only
- **Tenant User**: Sees data from their tenant only

## 🔍 Drill-Down Logic

### Widget-Level Drill-Down
```csharp
public class DrillDownChartWidget : IWidget
{
    public async Task<WidgetRenderResult> RenderAsync(WidgetRequest request)
    {
        var drillDownConfig = new DrillDownConfiguration
        {
            Levels = new[]
            {
                new DrillDownLevel { Level = 1, Type = "region", Target = "region-details" },
                new DrillDownLevel { Level = 2, Type = "tenant", Target = "tenant-details" },
                new DrillDownLevel { Level = 3, Type = "template", Target = "template-details" }
            }
        };
        
        return new WidgetRenderResult
        {
            Html = await RenderChartAsync(chartData, drillDownConfig),
            Metadata = new { DrillDown = drillDownConfig }
        };
    }
}
```

### Dashboard-Level Drill-Down
```json
{
  "dashboardKey": "form-statistics",
  "drillDownTargets": [
    {
      "sourceWidget": "template-stats",
      "targetDashboard": "template-details",
      "parameters": { "templateId": "{selectedTemplate}" }
    }
  ]
}
```

## 🎯 Widget Integration with Current Components

### StatCardWidget Using Current StatCard Component
```csharp
public class StatCardWidget : IWidget
{
    public string Type => "stat-card";
    
    public async Task<WidgetRenderResult> RenderAsync(WidgetRequest request)
    {
        // ✅ Use current StatCard viewmodel
        var statCard = new StatCard
        {
            Title = request.Configuration["title"],
            Value = await GetValueFromDataSource(request.DataSource),
            Icon = request.Configuration["icon"],
            Color = request.Configuration["color"]
        };
        
        // ✅ Use current rendering approach
        var html = await _renderer.RenderViewAsync("Components/Dashboard/Atomic/_StatCard", statCard);
        
        return new WidgetRenderResult
        {
            Html = html,
            Data = statCard,
            Metadata = new { WidgetType = Type }
        };
    }
}
```

### ChartWidget Using Current Chart Component
```csharp
public class ChartWidget : IWidget
{
    public string Type => "chart";
    
    public async Task<WidgetRenderResult> RenderAsync(WidgetRequest request)
    {
        // ✅ Use current Chart viewmodel
        var chart = new Chart
        {
            Type = request.Configuration["chartType"],
            Data = await GetChartData(request.DataSource),
            Options = GetChartOptions(request.Configuration)
        };
        
        // ✅ Use current rendering approach
        var html = await _renderer.RenderViewAsync("Components/Dashboard/Atomic/_Chart", chart);
        
        return new WidgetRenderResult
        {
            Html = html,
            Data = chart,
            Metadata = new { SupportsRealTime = true }
        };
    }
}
```

## 🔄 Real-Time Capabilities

### WebSocket Integration
```csharp
public interface IRealTimeManager
{
    Task SubscribeToUpdatesAsync(string dashboardKey, string connectionId);
    Task UnsubscribeFromUpdatesAsync(string connectionId);
    Task BroadcastUpdateAsync(string dashboardKey, object data);
}

public class RealTimeWidget : IWidget
{
    public bool SupportsRealTime { get; } = true;
    
    public async Task<WidgetRenderResult> RenderAsync(WidgetRequest request)
    {
        var result = await RenderWidgetAsync(request);
        
        // Add real-time subscription
        if (request.Configuration["realTime"] == true)
        {
            await _realTimeManager.SubscribeToUpdatesAsync(request.DashboardKey, request.ConnectionId);
        }
        
        return result;
    }
    
    public async Task HandleRealTimeUpdateAsync(RealTimeUpdate update)
    {
        // Update widget data without full refresh
        var newData = await GetUpdatedDataAsync(update);
        await _clientUpdater.UpdateWidgetAsync(update.WidgetId, newData);
    }
}
```

## 🔄 FormSubmissionStatisticsService Refactor

### Current Service (Keep Logic)
```csharp
// ✅ Keep existing methods - they work perfectly
public class FormSubmissionStatisticsService : IFormSubmissionStatisticsService
{
    public async Task<List<StatCardConfig>> BuildQuickStatsAsync(
        List<int> templateIds, DateTime? startDate, DateTime? endDate, 
        int? tenantId, ClaimsPrincipal currentUser)
    {
        // Existing logic - no changes needed
        var accessibleTemplateIds = await GetAccessibleTemplateIdsAsync(currentUser);
        // ... rest of existing implementation
    }
}
```

### Add Dashboard Integration
```csharp
public class FormSubmissionStatisticsService : IFormSubmissionStatisticsService
{
    // 🆕 Add bridge methods for dashboard engine
    public async Task<object> GetDataAsync(DataRequest request)
    {
        return request.DataSourceKey switch
        {
            "quick-stats" => await BuildQuickStatsAsync(request.Parameters),
            "trend-data" => await GetSubmissionTrendsAsync(request.Parameters),
            "status-data" => await GetSubmissionsByStatusAsync(request.Parameters),
            "recent-submissions" => await GetRecentSubmissionsAsync(request.Parameters),
            _ => throw new NotSupportedException($"Data source {request.DataSourceKey} not supported")
        };
    }
    
    // 🆕 Register with dashboard engine
    public void RegisterWithDashboardEngine(IDashboardRegistry registry)
    {
        registry.RegisterDataSource(new DataSourceDefinition
        {
            Key = "form-submission-stats",
            Type = "service",
            Service = this,
            Security = new SecurityDefinition
            {
                DataFilter = "template_id IN (SELECT template_id FROM user_template_access WHERE user_id = @userId)"
            }
        });
    }
}
```

### Data Adapter Bridge
```csharp
public class StatisticsDataAdapter : IDataAdapter
{
    private readonly IFormSubmissionStatisticsService _submissionStats;
    
    public async Task<object> GetDataAsync(DataRequest request)
    {
        // Route to appropriate service
        return request.DataSourceKey.Split(':')[0] switch
        {
            "submissions" => await _submissionStats.GetDataAsync(request),
            "scores" => await _scoreCalculation.GetDataAsync(request),
            _ => throw new NotSupportedException($"Unknown data source: {request.DataSourceKey}")
        };
    }
}
```

## 🎯 Dashboard Configuration Example

### Form Statistics Dashboard (JSON)
```json
{
  "key": "form-statistics",
  "name": "Form Statistics Dashboard",
  "widgets": [
    {
      "id": "quick-stats",
      "type": "stat-cards",
      "dataSource": "submissions:quick-stats"
    },
    {
      "id": "trend-chart",
      "type": "chart", 
      "dataSource": "submissions:trend-data",
      "drillDown": {
        "target": "template-details",
        "parameters": { "templateId": "{selectedTemplate}" }
      }
    }
  ],
  "dataSources": [
    {
      "key": "submissions:quick-stats",
      "type": "service",
      "service": "FormSubmissionStatisticsService"
    }
  ]
}
```

## 🚀 Complete Architecture Benefits

### **✅ Integration with Current Components**
- **DashboardConfig**: Enhanced with layout and metadata
- **DashboardSection**: Uses existing SectionComponentType and SectionLoadMethod
- **Atomic Components**: StatCard, Chart, Table reused by widgets
- **Composite Components**: FilterPanelConfig integrated with filter system

### **✅ Layout Management**
- **LayoutManager**: Grid-based layouts with responsive design
- **Tab Optimization**: Specialized layouts for embedded dashboards
- **Dynamic Positioning**: Automatic widget positioning and sizing

### **✅ Security & Performance**
- **Multi-Level Security**: Dashboard, widget, and data source authorization
- **Multi-Tier Caching**: Memory → Redis → Database with policies
- **Performance Monitoring**: Render time tracking and cache hit metrics
- **Automatic Scope Handling**: Claims-based data filtering

### **✅ Real-Time Capabilities**
- **WebSocket Integration**: Live dashboard updates
- **Real-Time Widgets**: Widgets that support live data
- **Selective Updates**: Update individual widgets without full refresh

### **✅ Advanced Features**
- **Drill-Down Logic**: Multi-level navigation between dashboards
- **Filter Dependencies**: Cascading filter effects (region → tenant → template)
- **Partial Dashboard Integration**: Use dashboards in tabs and modals
- **Plugin Architecture**: Extensible widgets and data sources

### **✅ Developer Experience**
- **Backward Compatibility**: Existing services work without changes
- **Clear Separation**: Data, UI, and security layers separated
- **Easy Testing**: Each component has single responsibility
- **Configuration-Driven**: JSON-based dashboard definitions
