# Metric Population Service

**Purpose:** Automatically extract form submission data and populate KPI metrics in the reporting system  
**Trigger:** After submission approval (or immediate if no approval required)  
**Flow:** Approved Submission → Extract Responses → Apply Mappings → Populate TenantMetrics

---

## Overview

The Metric Population Service bridges form submissions with the reporting/analytics system by automatically extracting form response data and populating the `TenantMetrics` table. This eliminates manual data entry and ensures real-time metric updates.

### Key Concepts

**Metric Mapping Types:**
1. **Direct** - Field value directly becomes metric value
2. **Calculated** - Formula computes metric from multiple fields
3. **BinaryCompliance** - Boolean check converts to 100% or 0%

**Data Flow:**
```
Form Submission (Approved)
    ↓
Metric Population Service (Triggered)
    ↓
Read FormItemMetricMappings
    ↓
For Each Mapping:
    ├─ Extract Response Value(s)
    ├─ Apply Transformation Logic
    └─ Insert/Update TenantMetrics
    ↓
Log Results in MetricPopulationLog
    ↓
Complete
```

---

## 1. Service Trigger

### Trigger Points

**Primary Trigger:** Submission Status → "Approved"

```csharp
// In ApprovalWorkflowService (after final approval)
if (allStepsApproved)
{
    submission.Status = "Approved";
    submission.ApprovedDate = DateTime.UtcNow;
    
    await _context.SaveChangesAsync();
    
    // Trigger metric population
    await _metricPopulationService.PopulateMetrics(submission.SubmissionId);
}

// In SubmitForm (for templates without approval)
if (!submission.Assignment.Template.RequiresApproval)
{
    submission.Status = "Approved";
    submission.ApprovedDate = DateTime.UtcNow;
    
    await _context.SaveChangesAsync();
    
    // Trigger metric population immediately
    await _metricPopulationService.PopulateMetrics(submission.SubmissionId);
}
```

**Alternative Triggers:**
- Manual trigger by administrator
- Scheduled batch processing for failed populations
- Reprocessing after metric definition changes

---

## 2. Metric Mapping Configuration

### Database Schema

**Table: FormItemMetricMappings**
```sql
CREATE TABLE FormItemMetricMappings (
    MappingId INT PRIMARY KEY IDENTITY(1,1),
    ItemId INT NOT NULL,                    -- FK to FormTemplateItems
    MetricId INT NOT NULL,                  -- FK to MetricDefinitions
    MappingType NVARCHAR(30) NOT NULL,      -- 'Direct', 'Calculated', 'BinaryCompliance', 'Derived'
    TransformationLogic NVARCHAR(MAX) NULL, -- JSON: {"formula": "(item21 / item20) * 100", "items": [21, 20]}
    ExpectedValue NVARCHAR(100) NULL,       -- For BinaryCompliance: 'TRUE', 'Yes', '100%', etc.
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_ItemMetricMap_Item FOREIGN KEY (ItemId)
        REFERENCES FormTemplateItems(ItemId) ON DELETE CASCADE,
    CONSTRAINT FK_ItemMetricMap_Metric FOREIGN KEY (MetricId)
        REFERENCES MetricDefinitions(MetricId),
    CONSTRAINT UQ_ItemMetricMap UNIQUE (ItemId, MetricId)
)
```

### Mapping Examples

**Example 1: Direct Mapping**
```json
{
    "itemId": 45,
    "itemName": "Number of Computers",
    "metricId": 12,
    "metricName": "Total Computers per Tenant",
    "mappingType": "Direct",
    "transformationLogic": null
}
```

**Example 2: Calculated Mapping**
```json
{
    "itemId": 52,
    "metricId": 15,
    "metricName": "Network Uptime Percentage",
    "mappingType": "Calculated",
    "transformationLogic": {
        "formula": "(item47 - item48) / item47 * 100",
        "items": [47, 48],
        "description": "Total Hours - Downtime Hours / Total Hours * 100"
    }
}
```

**Example 3: Binary Compliance Mapping**
```json
{
    "itemId": 38,
    "itemName": "Is LAN Working?",
    "metricId": 20,
    "metricName": "LAN Compliance",
    "mappingType": "BinaryCompliance",
    "expectedValue": "Yes",
    "transformationLogic": null
}
```

---

## 3. Metric Population Service Implementation

### Service Class Structure

**File:** `Services/MetricPopulationService.cs`

```csharp
public class MetricPopulationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MetricPopulationService> _logger;
    
    public MetricPopulationService(
        ApplicationDbContext context,
        ILogger<MetricPopulationService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<MetricPopulationResult> PopulateMetrics(int submissionId)
    {
        var result = new MetricPopulationResult
        {
            SubmissionId = submissionId,
            StartTime = DateTime.UtcNow,
            SuccessCount = 0,
            FailureCount = 0,
            Details = new List<MetricPopulationDetail>()
        };
        
        try
        {
            _logger.LogInformation("Starting metric population for submission {SubmissionId}", submissionId);
            
            // Load submission with all related data
            var submission = await LoadSubmissionData(submissionId);
            
            if (submission == null)
            {
                _logger.LogError("Submission {SubmissionId} not found", submissionId);
                result.OverallStatus = "Failed";
                result.ErrorMessage = "Submission not found";
                return result;
            }
            
            // Get all metric mappings for this template
            var mappings = await GetMetricMappings(submission.Assignment.TemplateId);
            
            if (!mappings.Any())
            {
                _logger.LogWarning("No metric mappings found for template {TemplateId}", 
                    submission.Assignment.TemplateId);
                result.OverallStatus = "NoMappings";
                return result;
            }
            
            // Process each mapping
            foreach (var mapping in mappings)
            {
                var detail = await ProcessMapping(submission, mapping);
                result.Details.Add(detail);
                
                if (detail.Status == "Success")
                    result.SuccessCount++;
                else
                    result.FailureCount++;
            }
            
            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime.Value.Subtract(result.StartTime);
            result.OverallStatus = result.FailureCount == 0 ? "Success" : "Partial";
            
            // Log summary
            await LogPopulationSummary(result);
            
            _logger.LogInformation(
                "Metric population completed for submission {SubmissionId}. Success: {Success}, Failed: {Failed}",
                submissionId, result.SuccessCount, result.FailureCount);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Metric population failed for submission {SubmissionId}", submissionId);
            result.OverallStatus = "Failed";
            result.ErrorMessage = ex.Message;
            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime.Value.Subtract(result.StartTime);
            
            await LogPopulationSummary(result);
            
            throw;
        }
    }
    
    private async Task<FormTemplateSubmission> LoadSubmissionData(int submissionId)
    {
        return await _context.FormTemplateSubmissions
            .Include(s => s.Assignment)
                .ThenInclude(a => a.Template)
            .Include(s => s.Assignment.Tenant)
            .Include(s => s.Responses)
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
    }
    
    private async Task<List<FormItemMetricMapping>> GetMetricMappings(int templateId)
    {
        // Get all items for this template
        var templateItemIds = await _context.FormTemplateItems
            .Where(i => i.TemplateId == templateId)
            .Select(i => i.ItemId)
            .ToListAsync();
        
        // Get mappings for these items
        return await _context.FormItemMetricMappings
            .Include(m => m.Metric)
            .Include(m => m.Item)
            .Where(m => templateItemIds.Contains(m.ItemId) && m.IsActive)
            .ToListAsync();
    }
}
```

---

## 4. Mapping Processing Logic

### Process Individual Mapping

```csharp
private async Task<MetricPopulationDetail> ProcessMapping(
    FormTemplateSubmission submission,
    FormItemMetricMapping mapping)
{
    var detail = new MetricPopulationDetail
    {
        MappingId = mapping.MappingId,
        MetricId = mapping.MetricId,
        MetricName = mapping.Metric.MetricName,
        ItemId = mapping.ItemId,
        ItemName = mapping.Item.ItemName,
        MappingType = mapping.MappingType,
        StartTime = DateTime.UtcNow
    };
    
    try
    {
        decimal? metricValue = null;
        
        // Process based on mapping type
        switch (mapping.MappingType)
        {
            case "Direct":
                metricValue = await ProcessDirectMapping(submission, mapping, detail);
                break;
            
            case "Calculated":
                metricValue = await ProcessCalculatedMapping(submission, mapping, detail);
                break;
            
            case "BinaryCompliance":
                metricValue = await ProcessBinaryComplianceMapping(submission, mapping, detail);
                break;
            
            default:
                detail.Status = "Failed";
                detail.ErrorMessage = $"Unknown mapping type: {mapping.MappingType}";
                _logger.LogWarning("Unknown mapping type {MappingType} for mapping {MappingId}",
                    mapping.MappingType, mapping.MappingId);
                return detail;
        }
        
        if (metricValue.HasValue)
        {
            // Save to TenantMetrics
            await SaveMetricValue(
                submission.Assignment.TenantId,
                mapping.MetricId,
                submission.Assignment.ReportingPeriod,
                metricValue.Value,
                submission.SubmissionId
            );
            
            detail.CalculatedValue = metricValue.Value;
            detail.Status = "Success";
            detail.Message = "Metric populated successfully";
        }
        else
        {
            detail.Status = "Failed";
            detail.ErrorMessage = "Calculated value is null";
        }
    }
    catch (Exception ex)
    {
        detail.Status = "Failed";
        detail.ErrorMessage = ex.Message;
        _logger.LogError(ex, 
            "Failed to process mapping {MappingId} for submission {SubmissionId}",
            mapping.MappingId, submission.SubmissionId);
    }
    
    detail.EndTime = DateTime.UtcNow;
    detail.Duration = detail.EndTime.Subtract(detail.StartTime);
    
    // Log to database
    await LogMetricPopulation(submission.SubmissionId, detail);
    
    return detail;
}
```

### Direct Mapping

```csharp
private async Task<decimal?> ProcessDirectMapping(
    FormTemplateSubmission submission,
    FormItemMetricMapping mapping,
    MetricPopulationDetail detail)
{
    var response = submission.Responses
        .FirstOrDefault(r => r.ItemId == mapping.ItemId);
    
    if (response == null || string.IsNullOrEmpty(response.ResponseValue))
    {
        detail.ErrorMessage = "No response value found";
        _logger.LogWarning(
            "No response found for item {ItemId} in submission {SubmissionId}",
            mapping.ItemId, submission.SubmissionId);
        return null;
    }
    
    // Parse numeric value
    if (decimal.TryParse(response.ResponseValue, out var value))
    {
        detail.SourceValues = new Dictionary<string, string>
        {
            { mapping.Item.ItemName, response.ResponseValue }
        };
        
        return value;
    }
    
    detail.ErrorMessage = $"Cannot parse value '{response.ResponseValue}' as number";
    _logger.LogWarning(
        "Cannot parse response value '{Value}' as decimal for item {ItemId}",
        response.ResponseValue, mapping.ItemId);
    
    return null;
}
```

### Calculated Mapping

```csharp
private async Task<decimal?> ProcessCalculatedMapping(
    FormTemplateSubmission submission,
    FormItemMetricMapping mapping,
    MetricPopulationDetail detail)
{
    var logic = JsonConvert.DeserializeObject<TransformationLogic>(
        mapping.TransformationLogic);
    
    if (logic == null || string.IsNullOrEmpty(logic.Formula))
    {
        detail.ErrorMessage = "Invalid transformation logic";
        return null;
    }
    
    // Build formula with actual values
    var formula = logic.Formula;
    var sourceValues = new Dictionary<string, string>();
    
    foreach (var itemId in logic.Items)
    {
        var response = submission.Responses
            .FirstOrDefault(r => r.ItemId == itemId);
        
        if (response == null || !decimal.TryParse(response.ResponseValue, out var value))
        {
            detail.ErrorMessage = $"Missing or invalid value for item {itemId}";
            _logger.LogWarning(
                "Cannot find or parse value for item {ItemId} in formula",
                itemId);
            return null;
        }
        
        // Replace placeholder with actual value
        formula = formula.Replace($"item{itemId}", value.ToString());
        
        // Track source values
        var item = await _context.FormTemplateItems
            .FirstOrDefaultAsync(i => i.ItemId == itemId);
        sourceValues[item?.ItemName ?? $"Item{itemId}"] = response.ResponseValue;
    }
    
    detail.SourceValues = sourceValues;
    detail.Formula = formula;
    
    // Evaluate formula
    try
    {
        var expression = new Expression(formula);
        var result = expression.Evaluate();
        
        if (result == null)
        {
            detail.ErrorMessage = "Formula evaluation returned null";
            return null;
        }
        
        return Convert.ToDecimal(result);
    }
    catch (DivideByZeroException)
    {
        detail.ErrorMessage = "Division by zero in formula";
        _logger.LogWarning("Division by zero in formula: {Formula}", formula);
        return null;
    }
    catch (Exception ex)
    {
        detail.ErrorMessage = $"Formula evaluation error: {ex.Message}";
        _logger.LogError(ex, "Formula evaluation failed: {Formula}", formula);
        return null;
    }
}
```

### Binary Compliance Mapping

```csharp
private async Task<decimal?> ProcessBinaryComplianceMapping(
    FormTemplateSubmission submission,
    FormItemMetricMapping mapping,
    MetricPopulationDetail detail)
{
    var response = submission.Responses
        .FirstOrDefault(r => r.ItemId == mapping.ItemId);
    
    if (response == null || string.IsNullOrEmpty(response.ResponseValue))
    {
        detail.ErrorMessage = "No response value found";
        return null;
    }
    
    detail.SourceValues = new Dictionary<string, string>
    {
        { mapping.Item.ItemName, response.ResponseValue },
        { "ExpectedValue", mapping.ExpectedValue }
    };
    
    // Check if response matches expected value
    var isCompliant = response.ResponseValue.Equals(
        mapping.ExpectedValue, 
        StringComparison.OrdinalIgnoreCase);
    
    // Return 100 for compliant, 0 for non-compliant
    var complianceValue = isCompliant ? 100m : 0m;
    
    detail.Message = isCompliant 
        ? $"Compliant (matches '{mapping.ExpectedValue}')"
        : $"Non-compliant (expected '{mapping.ExpectedValue}', got '{response.ResponseValue}')";
    
    return complianceValue;
}
```

---

## 5. Metric Storage

### Save to TenantMetrics

```csharp
private async Task SaveMetricValue(
    int tenantId,
    int metricId,
    DateTime reportingPeriod,
    decimal value,
    int sourceReferenceId)
{
    // Check if metric already exists for this period
    var existingMetric = await _context.TenantMetrics
        .FirstOrDefaultAsync(m =>
            m.TenantId == tenantId &&
            m.MetricId == metricId &&
            m.ReportingPeriod == reportingPeriod);
    
    if (existingMetric != null)
    {
        // Update existing metric
        existingMetric.NumericValue = value;
        existingMetric.SourceType = "UserInput";
        existingMetric.SourceReferenceId = sourceReferenceId;
        existingMetric.LastUpdated = DateTime.UtcNow;
        
        _logger.LogInformation(
            "Updated metric {MetricId} for tenant {TenantId}, period {Period}: {Value}",
            metricId, tenantId, reportingPeriod, value);
    }
    else
    {
        // Insert new metric
        var newMetric = new TenantMetric
        {
            TenantId = tenantId,
            MetricId = metricId,
            ReportingPeriod = reportingPeriod,
            NumericValue = value,
            SourceType = "UserInput",
            SourceReferenceId = sourceReferenceId,
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        
        _context.TenantMetrics.Add(newMetric);
        
        _logger.LogInformation(
            "Created metric {MetricId} for tenant {TenantId}, period {Period}: {Value}",
            metricId, tenantId, reportingPeriod, value);
    }
    
    await _context.SaveChangesAsync();
}
```

---

## 6. Logging & Audit Trail

### Metric Population Logs

**Table:** `MetricPopulationLog` (from your schema)
```sql
CREATE TABLE MetricPopulationLog (
    LogId BIGINT PRIMARY KEY IDENTITY(1,1),
    SubmissionId INT NOT NULL,
    MetricId INT NOT NULL,
    MappingId INT NOT NULL,
    SourceItemId INT NOT NULL,
    
    -- Source and calculated values
    SourceValue NVARCHAR(MAX),           -- Raw value from form response
    CalculatedValue DECIMAL(18,4),       -- Final calculated value for metric
    CalculationFormula NVARCHAR(MAX),    -- Audit trail of calculation used
    
    -- Processing metadata
    PopulatedDate DATETIME2 DEFAULT GETUTCDATE(),
    PopulatedBy INT NULL,                -- NULL if system/automated, UserId if manual override
    Status NVARCHAR(20) NOT NULL,        -- Success, Failed, Skipped, Pending
    ErrorMessage NVARCHAR(500),
    ProcessingTimeMs INT,                -- Performance tracking
    
    CONSTRAINT FK_MetricLog_Submission FOREIGN KEY (SubmissionId)
        REFERENCES FormTemplateSubmissions(SubmissionId),
    CONSTRAINT FK_MetricLog_Metric FOREIGN KEY (MetricId)
        REFERENCES MetricDefinitions(MetricId),
    CONSTRAINT FK_MetricLog_Mapping FOREIGN KEY (MappingId)
        REFERENCES FormItemMetricMappings(MappingId),
    CONSTRAINT FK_MetricLog_Item FOREIGN KEY (SourceItemId)
        REFERENCES FormTemplateItems(ItemId),
    CONSTRAINT FK_MetricLog_User FOREIGN KEY (PopulatedBy)
        REFERENCES Users(UserId),
    CONSTRAINT CK_MetricLog_Status CHECK (
        Status IN ('Success', 'Failed', 'Skipped', 'Pending')
    )
)
```

### Log Individual Metric

```csharp
private async Task LogMetricPopulation(
    int submissionId,
    MetricPopulationDetail detail)
{
    var log = new MetricPopulationLog
    {
        SubmissionId = submissionId,
        MetricId = detail.MetricId,
        MappingId = detail.MappingId,
        SourceItemId = detail.ItemId,
        SourceValue = detail.SourceValues != null 
            ? JsonConvert.SerializeObject(detail.SourceValues) 
            : null,
        CalculatedValue = detail.CalculatedValue,
        CalculationFormula = detail.Formula,
        Status = detail.Status,
        ErrorMessage = detail.ErrorMessage,
        ProcessingTimeMs = detail.Duration.HasValue 
            ? (int)detail.Duration.Value.TotalMilliseconds 
            : null,
        PopulatedDate = DateTime.UtcNow,
        PopulatedBy = null  // NULL for automated system population
    };
    
    _context.MetricPopulationLog.Add(log);
    await _context.SaveChangesAsync();
}
```

### Log Summary

**Note:** The schema includes detailed per-metric logging in `MetricPopulationLog`. For high-level summary tracking, you can optionally add a summary log or use aggregated queries on `MetricPopulationLog` grouped by `SubmissionId`.

```csharp
private async Task LogPopulationSummary(MetricPopulationResult result)
{
    // Optional: Log high-level summary
    // All details are already in MetricPopulationLog per metric
    // This is for quick overview queries
    
    _logger.LogInformation(
        "Metric population completed for submission {SubmissionId}. " +
        "Status: {Status}, Success: {Success}, Failed: {Failed}, Duration: {Duration}ms",
        result.SubmissionId,
        result.OverallStatus,
        result.SuccessCount,
        result.FailureCount,
        (int)result.Duration.TotalMilliseconds
    );
}
```

---

## 7. Error Handling & Retry Logic

### Retry for Failed Populations

**Background Job (Optional):**
```csharp
public class MetricPopulationRetryService : IHostedService
{
    private Timer _timer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MetricPopulationRetryService> _logger;
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Metric Population Retry Service started");
        
        // Run every hour
        _timer = new Timer(RetryFailedPopulations, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        
        return Task.CompletedTask;
    }
    
    private async void RetryFailedPopulations(object state)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var populationService = scope.ServiceProvider.GetRequiredService<MetricPopulationService>();
        
        // Find submissions with failed population in last 7 days
        var failedSubmissions = await context.MetricPopulationLog
            .Where(s => 
                s.Status == "Failed" &&
                s.PopulatedDate >= DateTime.UtcNow.AddDays(-7))
            .GroupBy(s => s.SubmissionId)
            .Select(g => g.Key)
            .ToListAsync();
        
        foreach (var submissionId in failedSubmissions)
        {
            try
            {
                _logger.LogInformation("Retrying metric population for submission {SubmissionId}", submissionId);
                await populationService.PopulateMetrics(submissionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Retry failed for submission {SubmissionId}", submissionId);
            }
        }
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
}
```

---

## 8. Manual Reprocessing

### Admin Endpoint for Reprocessing

```csharp
[HttpPost("api/admin/metrics/reprocess/{submissionId}")]
[Authorize(Roles = "Administrator")]
public async Task<IActionResult> ReprocessMetrics(int submissionId)
{
    try
    {
        var result = await _metricPopulationService.PopulateMetrics(submissionId);
        
        return Ok(new
        {
            success = true,
            result = new
            {
                submissionId = result.SubmissionId,
                status = result.OverallStatus,
                successCount = result.SuccessCount,
                failureCount = result.FailureCount,
                duration = result.Duration.TotalSeconds,
                details = result.Details.Select(d => new
                {
                    metricName = d.MetricName,
                    status = d.Status,
                    value = d.CalculatedValue,
                    error = d.ErrorMessage
                })
            }
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Manual reprocessing failed for submission {SubmissionId}", submissionId);
        return StatusCode(500, new { success = false, error = ex.Message });
    }
}
```

---

## 9. Result Models

### MetricPopulationResult

```csharp
public class MetricPopulationResult
{
    public int SubmissionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string OverallStatus { get; set; } // Success, Partial, Failed, NoMappings
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public string ErrorMessage { get; set; }
    public List<MetricPopulationDetail> Details { get; set; }
}

public class MetricPopulationDetail
{
    public int MappingId { get; set; }
    public int MetricId { get; set; }
    public string MetricName { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public string MappingType { get; set; }
    public decimal? CalculatedValue { get; set; }
    public string Formula { get; set; }
    public Dictionary<string, string> SourceValues { get; set; }
    public string Status { get; set; } // Success, Failed
    public string Message { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan? Duration { get; set; }
}

public class TransformationLogic
{
    public string Formula { get; set; }
    public List<int> Items { get; set; }
    public string Description { get; set; }
}
```

---

## 10. Reporting Note

**Form submission reporting and analytics** are handled in **Section 10: Reporting & Analytics** which will cover:

- Submission compliance dashboards
- Metric trend analysis over time
- Form response analytics
- Export capabilities (Excel, PDF)
- Visual charts and graphs
- Comparative analysis across tenants/regions
- Approval workflow performance metrics
- Submission timeline tracking

---

## Summary

The Metric Population Service provides:

✅ **Automated metric extraction** from approved form submissions  
✅ **Three mapping types** for flexible data transformation  
✅ **Error handling** with detailed logging  
✅ **Audit trail** for all population activities  
✅ **Retry mechanism** for failed populations  
✅ **Manual reprocessing** capability for administrators  
✅ **Real-time metric updates** for reporting dashboards

This ensures form data seamlessly flows into the reporting system without manual intervention.
