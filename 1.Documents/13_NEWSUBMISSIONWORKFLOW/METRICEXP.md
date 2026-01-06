┌─────────────────────────────────────────────────────────────────────┐
│  CATEGORY HIERARCHY WITH CONSTRAINTS                                │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  MetricCategory (Parent)          MetricSubCategory (Child)         │
│  ├── SCORE                        ├── Satisfaction Score            │
│  │                                ├── Quality Score                 │
│  │                                └── Compliance Score              │
│  ├── PERFORMANCE                  ├── Efficiency Rate               │
│  │                                ├── Utilization Rate              │
│  │                                └── Availability Rate             │
│  ├── TREND                        ├── Growth Rate                   │
│  │                                └── Change Rate                   │
│  ├── COMPARISON                   ├── Ratio                         │
│  │                                └── Variance                      │
│  ├── TIME                         ├── Duration                      │
│  │                                └── Response Time                 │
│  └── COUNT                        ├── Quantity                      │
│                                   └── Frequency                     │
│                                                                     │
│  Each SubCategory defines:                                          │
│  • Allowed DataTypes                                                │
│  • Allowed AggregationTypes                                         │
│  • Allowed Units (filtered)                                         │
│  • Defaults for each          

// Models/Entities/Metrics/MetricCategory.cs
public class MetricCategory
{
    public int CategoryId { get; set; }
    public string CategoryCode { get; set; }      // SCORE, PERFORMANCE, TREND, etc.
    public string CategoryName { get; set; }
    public string? Description { get; set; }
    public string? IconClass { get; set; }
    public string? ColorHint { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<MetricSubCategory> SubCategories { get; set; }
}

2. MetricSubCategory (NEW - with constraints)
// Models/Entities/Metrics/MetricSubCategory.cs
public class MetricSubCategory
{
    public int SubCategoryId { get; set; }
    public int CategoryId { get; set; }
    public string SubCategoryCode { get; set; }   // SATISFACTION_SCORE, EFFICIENCY_RATE
    public string SubCategoryName { get; set; }   // "Satisfaction Score"
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // === CONSTRAINTS ===
    // Comma-separated allowed values (or JSON array)
    public string AllowedDataTypes { get; set; }        // "Percentage,Decimal"
    public string AllowedAggregationTypes { get; set; } // "AVG,SUM,LAST_VALUE"
    
    // Defaults
    public string DefaultDataType { get; set; }         // "Percentage"
    public string DefaultAggregationType { get; set; }  // "AVG"
    public int? DefaultUnitId { get; set; }

    // Navigation
    public MetricCategory Category { get; set; }
    public MetricUnit? DefaultUnit { get; set; }
    public ICollection<MetricSubCategoryUnit> AllowedUnits { get; set; }
}

3. MetricSubCategoryUnit (Junction table for allowed units)
// Models/Entities/Metrics/MetricSubCategoryUnit.cs
public class MetricSubCategoryUnit
{
    public int SubCategoryId { get; set; }
    public int UnitId { get; set; }
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }

    // Navigation
    public MetricSubCategory SubCategory { get; set; }
    public MetricUnit Unit { get; set; }
}

4. MetricDefinition (updated)
// Models/Entities/Metrics/MetricDefinition.cs
public class MetricDefinition
{
    // ... existing properties ...
    
    // Change from CategoryId to SubCategoryId
    public int SubCategoryId { get; set; }        // Links to subcategory (which links to category)
    
    // These remain but are now constrained by SubCategory
    public string DataType { get; set; }
    public string AggregationType { get; set; }
    public int? UnitId { get; set; }

    // Navigation
    public MetricSubCategory SubCategory { get; set; }
    public MetricUnit? Unit { get; set; }
}

Seed Data Example
// Data/Seeders/MetricCategorySeeder.cs
public static class MetricCategorySeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        // Categories
        modelBuilder.Entity<MetricCategory>().HasData(
            new { CategoryId = 1, CategoryCode = "SCORE", CategoryName = "Score", IconClass = "ri-star-line", ColorHint = "#FFB400", DisplayOrder = 1, IsActive = true },
            new { CategoryId = 2, CategoryCode = "PERFORMANCE", CategoryName = "Performance", IconClass = "ri-speed-line", ColorHint = "#4CAF50", DisplayOrder = 2, IsActive = true },
            new { CategoryId = 3, CategoryCode = "TREND", CategoryName = "Trend", IconClass = "ri-line-chart-line", ColorHint = "#2196F3", DisplayOrder = 3, IsActive = true },
            new { CategoryId = 4, CategoryCode = "COMPARISON", CategoryName = "Comparison", IconClass = "ri-scales-line", ColorHint = "#9C27B0", DisplayOrder = 4, IsActive = true },
            new { CategoryId = 5, CategoryCode = "TIME", CategoryName = "Time", IconClass = "ri-time-line", ColorHint = "#FF5722", DisplayOrder = 5, IsActive = true },
            new { CategoryId = 6, CategoryCode = "COUNT", CategoryName = "Count", IconClass = "ri-hashtag", ColorHint = "#607D8B", DisplayOrder = 6, IsActive = true }
        );

        // SubCategories with constraints
        modelBuilder.Entity<MetricSubCategory>().HasData(
            // SCORE subcategories
            new { 
                SubCategoryId = 1, 
                CategoryId = 1, 
                SubCategoryCode = "SATISFACTION_SCORE", 
                SubCategoryName = "Satisfaction Score",
                AllowedDataTypes = "Percentage,Decimal,Integer",
                AllowedAggregationTypes = "AVG,LAST_VALUE",
                DefaultDataType = "Percentage",
                DefaultAggregationType = "AVG",
                DefaultUnitId = 1,  // Percentage unit
                DisplayOrder = 1,
                IsActive = true
            },
            new { 
                SubCategoryId = 2, 
                CategoryId = 1, 
                SubCategoryCode = "QUALITY_SCORE", 
                SubCategoryName = "Quality Score",
                AllowedDataTypes = "Percentage,Decimal",
                AllowedAggregationTypes = "AVG,MIN",
                DefaultDataType = "Percentage",
                DefaultAggregationType = "AVG",
                DefaultUnitId = 1,
                DisplayOrder = 2,
                IsActive = true
            },
            new { 
                SubCategoryId = 3, 
                CategoryId = 1, 
                SubCategoryCode = "COMPLIANCE_SCORE", 
                SubCategoryName = "Compliance Score",
                AllowedDataTypes = "Percentage,Boolean",
                AllowedAggregationTypes = "AVG,COUNT",
                DefaultDataType = "Percentage",
                DefaultAggregationType = "AVG",
                DefaultUnitId = 1,
                DisplayOrder = 3,
                IsActive = true
            },
            
            // PERFORMANCE subcategories
            new { 
                SubCategoryId = 4, 
                CategoryId = 2, 
                SubCategoryCode = "EFFICIENCY_RATE", 
                SubCategoryName = "Efficiency Rate",
                AllowedDataTypes = "Percentage,Decimal",
                AllowedAggregationTypes = "AVG,MIN,MAX",
                DefaultDataType = "Percentage",
                DefaultAggregationType = "AVG",
                DefaultUnitId = 1,
                DisplayOrder = 1,
                IsActive = true
            },
            new { 
                SubCategoryId = 5, 
                CategoryId = 2, 
                SubCategoryCode = "AVAILABILITY_RATE", 
                SubCategoryName = "Availability Rate",
                AllowedDataTypes = "Percentage",
                AllowedAggregationTypes = "AVG,MIN",
                DefaultDataType = "Percentage",
                DefaultAggregationType = "AVG",
                DefaultUnitId = 1,
                DisplayOrder = 2,
                IsActive = true
            },
            
            // TIME subcategories
            new { 
                SubCategoryId = 6, 
                CategoryId = 5, 
                SubCategoryCode = "DURATION", 
                SubCategoryName = "Duration",
                AllowedDataTypes = "Duration,Decimal,Integer",
                AllowedAggregationTypes = "AVG,SUM,MAX",
                DefaultDataType = "Duration",
                DefaultAggregationType = "AVG",
                DefaultUnitId = 3,  // Hours unit
                DisplayOrder = 1,
                IsActive = true
            },
            new { 
                SubCategoryId = 7, 
                CategoryId = 5, 
                SubCategoryCode = "RESPONSE_TIME", 
                SubCategoryName = "Response Time",
                AllowedDataTypes = "Duration,Decimal",
                AllowedAggregationTypes = "AVG,MAX,MIN",
                DefaultDataType = "Duration",
                DefaultAggregationType = "AVG",
                DefaultUnitId = 4,  // Minutes unit
                DisplayOrder = 2,
                IsActive = true
            },
            
            // COUNT subcategories
            new { 
                SubCategoryId = 8, 
                CategoryId = 6, 
                SubCategoryCode = "QUANTITY", 
                SubCategoryName = "Quantity",
                AllowedDataTypes = "Integer,Decimal",
                AllowedAggregationTypes = "SUM,COUNT,AVG",
                DefaultDataType = "Integer",
                DefaultAggregationType = "SUM",
                DefaultUnitId = 2,  // Count unit
                DisplayOrder = 1,
                IsActive = true
            },
            new { 
                SubCategoryId = 9, 
                CategoryId = 6, 
                SubCategoryCode = "FREQUENCY", 
                SubCategoryName = "Frequency",
                AllowedDataTypes = "Integer",
                AllowedAggregationTypes = "SUM,AVG",
                DefaultDataType = "Integer",
                DefaultAggregationType = "SUM",
                DefaultUnitId = 5,  // Per month/period unit
                DisplayOrder = 2,
                IsActive = true
            }
        );
    }
}

Visual Summary
User selects Category: "Score"
         │
         ▼
┌─────────────────────────────────────┐
│  SubCategory dropdown populates:    │
│  • Satisfaction Score               │
│  • Quality Score                    │
│  • Compliance Score                 │
└─────────────────────────────────────┘
         │
         ▼
User selects SubCategory: "Satisfaction Score"
         │
         ▼
┌─────────────────────────────────────┐
│  Constraints auto-applied:          │
│                                     │
│  DataType: [Percentage ▼]           │
│    (only: Percentage, Decimal, Int) │
│                                     │
│  Aggregation: [AVG ▼]               │
│    (only: AVG, LAST_VALUE)          │
│                                     │
│  Unit: [% ▼]                        │
│    (only: %, Points, Rating)        │
└─────────────────────────────────────┘

How It All Connects: The Complete Flow
Two Levels of Configuration
There are actually two different purposes for these properties that often get confused: MetricDefinition level defines WHAT the metric is:
Category/SubCategory tells us the nature of the metric (Score, Performance, etc.)
DataType tells us what kind of value it holds
AggregationType here means how to aggregate across TIME PERIODS (e.g., how to roll up monthly values into quarterly)
Unit tells us how to display it
Mapping level defines HOW to calculate the metric from form data:
MappingType tells us the calculation method (Direct, Calculated, BinaryCompliance)
AggregationType here means how to handle MULTIPLE VALUES within a single context (e.g., averaging multiple field responses, or aggregating child mappings into a parent)
These are not duplicates - they serve different aggregation contexts.

The Constraint Flow

When a user creates a metric mapping, the SubCategory constraints should cascade down:
1.User selects a field to map
2.User chooses to link to an existing metric OR create a new one
3.If linking to existing: the metric already has its SubCategory constraints defined
4.If creating new: the SubCategory selection constrains what DataType and AggregationType are available at the MetricDefinition level

The Mapping level (FormItemMetricMapping) then has its own MappingType and AggregationType that determine HOW to extract the value from form responses. These mapping-level properties are constrained by the FIELD TYPE, not the metric category. For example, a checkbox field can only use Direct or BinaryCompliance mapping types.

How Calculation Works with This System

When MetricPopulationService processes a form submission:
1.It finds all active FormItemMetricMappings for the template
For each mapping, it looks at the MappingType to determine calculation method
2.It uses the mapping's AggregationType if the field has multiple response values
3.It calculates the numeric value
4.It then looks at the linked MetricDefinition to understand what this value represents
5.It stores the result in TenantMetric with references to both the metric and submission
The SubCategory constraints ensure that when the value reaches TenantMetric, it's compatible with the metric's defined DataType and can be properly aggregated over time using the metric's AggregationType.

Section and Template Level Aggregation

FormSectionMetricMapping aggregates multiple field-level mappings into a section score. Its AggregationType (AVG, SUM, WeightedAverage) determines how child values combine. The linked MetricDefinition at section level should have a SubCategory that allows this aggregation pattern. FormTemplateMetricMapping does the same at the template level, aggregating section metrics into an overall KPI. The SubCategory constraints help ensure consistency. If a section metric is categorized as a "Satisfaction Score" (which defaults to AVG aggregation), the UI would guide users toward averaging the child field metrics rather than summing them.


TenantMetric's Role
TenantMetric is the storage table for all calculated metric values. It stores:
1.The tenant (location/organization)
2.The metric (which links to SubCategory and Category)
3.The reporting period (typically monthly)
4.The numeric and text values
5.The source (which submission or job created it)
When multiple submissions occur in the same period for the same tenant and metric, the MetricDefinition's AggregationType determines what happens. Some metrics use LAST_VALUE (just keep the most recent), others use AVG (average all submissions in the period), and others use SUM (accumulate). The SubCategory's AllowedAggregationTypes constraint ensures that metrics in that subcategory only use aggregation methods that make sense. You wouldn't want a "Satisfaction Score" using SUM (that would be meaningless), so the subcategory restricts it to AVG or LAST_VALUE.

Reports Functionality

Reports query TenantMetric to display metric values over time. The connection works like this: Filtering: Reports can filter by Category or SubCategory. A "Performance Dashboard" might only show metrics in the PERFORMANCE category. A "Compliance Report" might filter to the COMPLIANCE_SCORE subcategory. Display Formatting: The MetricDefinition's Unit (constrained by SubCategory) tells reports how to format values. Percentage metrics show with % symbol, Duration metrics show in hours/minutes, Count metrics show as whole numbers. Threshold Visualization: The MetricDefinition's threshold values (Green/Yellow/Red) are used by reports to show color-coded status. SubCategory could have suggested threshold ranges that guide users during metric creation. Time Aggregation: When reports show quarterly or yearly views, they need to aggregate monthly TenantMetric values. The MetricDefinition's AggregationType (which was constrained by SubCategory) tells the report how to do this. A "Satisfaction Score" averages monthly values, while a "Total Incidents" count sums them. Trend Analysis: SubCategories in the TREND category have specific visualization expectations. Reports know to show these as line charts with change indicators rather than bar charts or gauges. Comparison Reports: SubCategories in the COMPARISON category expect to be shown alongside other metrics or benchmarks. Reports can automatically suggest relevant comparisons within the same SubCategory.
The Validation Chain
When creating or updating mappings, validation should check:
The field's data type is compatible with the mapping type
If linking to an existing metric, the metric's DataType is compatible with what the mapping will produce
If creating a new metric, the SubCategory's AllowedDataTypes includes the type that the mapping will produce
The aggregation types at both levels make sense together
This prevents configurations like a BinaryCompliance mapping (which produces 0 or 100) being linked to a metric with DataType of "Text", or a COUNT aggregation being used on a SATISFACTION_SCORE subcategory.

Summary of Relationships

MetricCategory groups high-level metric types and drives UI organization. MetricSubCategory defines constraints that ensure metrics are configured consistently and meaningfully within their category. MetricDefinition uses those constraints to define individual metrics with appropriate data types, units, and time-aggregation behavior. FormItemMetricMapping defines how to extract values from form fields, with its own mapping logic. FormSectionMetricMapping and FormTemplateMetricMapping define hierarchical aggregation of those field values. TenantMetric stores the final calculated values, respecting the metric's configuration. Reports read from TenantMetric and use the full chain of metadata (Unit, Thresholds, Category, SubCategory) to format, filter, aggregate, and visualize the data appropriately.