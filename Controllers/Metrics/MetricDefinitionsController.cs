using FormReporting.Data;
using FormReporting.Models.ViewModels.Metrics;
using FormReporting.Models.ViewModels.Components;
using FormReporting.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Controllers.Metrics
{
    [Route("Metrics/[controller]")]
    public class MetricDefinitionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MetricDefinitionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Display the metric definitions index page with statistics and datatable
        /// </summary>
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? search, string? category, string? sourceType, int? page)
        {
            // 1. BUILD QUERY with filters
            var query = _context.MetricDefinitions.AsQueryable();

            // 2. APPLY SEARCH FILTER
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m =>
                    m.MetricName.Contains(search) ||
                    m.MetricCode.Contains(search) ||
                    (m.Description != null && m.Description.Contains(search)));
            }

            // 3. APPLY CATEGORY FILTER
            if (!string.IsNullOrEmpty(category) && int.TryParse(category, out var categoryId))
            {
                query = query.Where(m => m.SubCategory.CategoryId == categoryId);
            }

            // 4. APPLY SOURCE TYPE FILTER
            if (!string.IsNullOrEmpty(sourceType))
            {
                query = query.Where(m => m.SourceType == sourceType);
            }

            // 5. CALCULATE STATISTICS (for stat cards)
            var allMetrics = await _context.MetricDefinitions.ToListAsync();
            var totalMetrics = allMetrics.Count;
            var kpiMetrics = allMetrics.Count(m => m.IsKPI);
            var activeMetrics = allMetrics.Count(m => m.IsActive);
            var userInputMetrics = allMetrics.Count(m => m.SourceType == "UserInput");

            // Get unique categories and source types for filter dropdowns
            var categories = await _context.MetricCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => c.CategoryName)
                .ToListAsync();

            var sourceTypes = await _context.MetricDefinitions
                .Select(m => m.SourceType)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            // 6. PAGINATION
            var pageSize = 15;
            var totalRecords = await query.CountAsync();
            var currentPage = page ?? 1;
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            var skip = (currentPage - 1) * pageSize;

            // 7. GET PAGINATED DATA
            var metrics = await query
                .Include(m => m.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Include(m => m.Unit)
                .OrderBy(m => m.SubCategory.Category.CategoryName)
                .ThenBy(m => m.SubCategory.SubCategoryName)
                .ThenBy(m => m.MetricName)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            // 8. BUILD VIEW MODEL
            var viewModel = new MetricDefinitionsIndexViewModel
            {
                Metrics = metrics.Select(m => new MetricDefinitionItemViewModel
                {
                    MetricId = m.MetricId,
                    MetricCode = m.MetricCode,
                    MetricName = m.MetricName,
                    SubCategoryId = m.SubCategoryId,
                    SubCategoryName = m.SubCategory?.SubCategoryName ?? "Uncategorized",
                    CategoryId = m.SubCategory?.CategoryId,
                    CategoryName = m.SubCategory?.Category?.CategoryName ?? "Uncategorized",
                    Description = m.Description,
                    SourceType = m.SourceType,
                    DataType = m.DataType,
                    UnitId = m.UnitId,
                    UnitName = m.Unit?.UnitName,
                    IsKPI = m.IsKPI,
                    IsActive = m.IsActive,
                    ThresholdGreen = m.ThresholdGreen,
                    ThresholdYellow = m.ThresholdYellow,
                    ThresholdRed = m.ThresholdRed,
                    CreatedDate = m.CreatedDate
                }),
                TotalMetrics = totalMetrics,
                KpiMetrics = kpiMetrics,
                ActiveMetrics = activeMetrics,
                UserInputMetrics = userInputMetrics,
                Categories = categories,
                SourceTypes = sourceTypes
            };

            // 9. PASS PAGINATION DATA TO VIEW
            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentSourceType = sourceType;

            return View("~/Views/Metrics/MetricDefinitions/Index.cshtml", viewModel);
        }

        /// <summary>
        /// Build wizard configuration for metric definition
        /// </summary>
        private WizardViewModel BuildMetricWizard()
        {
            var wizardConfig = new WizardConfig
            {
                FormId = "metricDefinitionWizard",
                Layout = WizardLayout.Vertical,
                Steps = new List<WizardStep>
                {
                    // STEP 1: Basic Information
                    new WizardStep
                    {
                        StepId = "basic-info",
                        StepNumber = 1,
                        Title = "Basic Information",
                        Description = "Core details",
                        Instructions = "Enter metric code, name, category, and description",
                        Icon = "ri-information-line",
                        State = WizardStepState.Active,
                        ContentPartialPath = "~/Views/Metrics/MetricDefinitions/_BasicInfo.cshtml",
                        ShowPrevious = false,
                        ShowNext = true,
                        NextButtonText = "Next: Data Configuration"
                    },

                    // STEP 2: Data Configuration
                    new WizardStep
                    {
                        StepId = "data-config",
                        StepNumber = 2,
                        Title = "Data Configuration",
                        Description = "Collection setup",
                        Instructions = "Configure how this metric's data is collected and stored",
                        Icon = "ri-settings-3-line",
                        State = WizardStepState.Pending,
                        ContentPartialPath = "~/Views/Metrics/MetricDefinitions/_DataConfiguration.cshtml",
                        ShowPrevious = true,
                        ShowNext = true,
                        NextButtonText = "Next: KPI & Thresholds"
                    },

                    // STEP 3: KPI & Thresholds
                    new WizardStep
                    {
                        StepId = "kpi-thresholds",
                        StepNumber = 3,
                        Title = "KPI & Thresholds",
                        Description = "Performance tracking",
                        Instructions = "Set performance thresholds and mark as KPI if needed",
                        Icon = "ri-line-chart-line",
                        State = WizardStepState.Pending,
                        ContentPartialPath = "~/Views/Metrics/MetricDefinitions/_KpiThresholds.cshtml",
                        ShowPrevious = true,
                        ShowNext = true,
                        NextButtonText = "Next: Advanced Settings"
                    },

                    // STEP 4: Advanced Settings
                    new WizardStep
                    {
                        StepId = "advanced",
                        StepNumber = 4,
                        Title = "Advanced Settings",
                        Description = "Compliance rules",
                        Instructions = "Configure compliance rules and expected values (optional)",
                        Icon = "ri-shield-check-line",
                        State = WizardStepState.Pending,
                        ContentPartialPath = "~/Views/Metrics/MetricDefinitions/_AdvancedSettings.cshtml",
                        ShowPrevious = true,
                        ShowNext = false,
                        CustomButtonHtml = "<button type='submit' class='btn btn-primary'><i class='ri-save-line me-1'></i>Create Metric</button>"
                    }
                }
            };

            return wizardConfig.BuildWizard();
        }

        /// <summary>
        /// Display the create metric definition form with wizard
        /// </summary>
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var model = new CreateMetricDefinitionDto
            {
                // Set defaults
                SourceType = "UserInput",
                DataType = "Integer",
                IsKPI = false
            };

            var wizard = BuildMetricWizard();
            ViewData["Wizard"] = wizard;

            // Populate ViewBag with hierarchical dropdown data
            await LoadHierarchicalDropdowns();

            return View("~/Views/Metrics/MetricDefinitions/Create.cshtml", model);
        }

        /// <summary>
        /// Handle metric definition creation
        /// </summary>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMetricDefinitionDto model)
        {
            if (!ModelState.IsValid)
            {
                // Rebuild wizard for error display
                var wizard = BuildMetricWizard();
                ViewData["Wizard"] = wizard;
                await LoadHierarchicalDropdowns();
                return View("~/Views/Metrics/MetricDefinitions/Create.cshtml", model);
            }

            // Check for duplicate metric code
            var exists = await _context.MetricDefinitions
                .AnyAsync(m => m.MetricCode == model.MetricCode);

            if (exists)
            {
                ModelState.AddModelError("MetricCode", "A metric with this code already exists.");
                
                // Rebuild wizard for error display
                var wizard = BuildMetricWizard();
                ViewData["Wizard"] = wizard;
                await LoadHierarchicalDropdowns();
                return View("~/Views/Metrics/MetricDefinitions/Create.cshtml", model);
            }

            // Create new metric definition
            var metric = new Models.Entities.Metrics.MetricDefinition
            {
                MetricCode = model.MetricCode,
                MetricName = model.MetricName,
                SubCategoryId = model.SubCategoryId,
                Description = model.Description,
                SourceType = model.SourceType,
                DataType = model.DataType,
                UnitId = model.UnitId,
                AggregationType = model.AggregationType,
                IsKPI = model.IsKPI,
                ThresholdGreen = model.ThresholdGreen,
                ThresholdYellow = model.ThresholdYellow,
                ThresholdRed = model.ThresholdRed,
                ExpectedValue = model.ExpectedValue,
                ComplianceRule = model.ComplianceRule,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.MetricDefinitions.Add(metric);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Metric '{metric.MetricName}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Display the edit metric definition form
        /// </summary>
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var metric = await _context.MetricDefinitions.FindAsync(id);

            if (metric == null)
            {
                TempData["ErrorMessage"] = "Metric definition not found.";
                return RedirectToAction(nameof(Index));
            }

            // Map to DTO (we'll use the full entity for editing)
            var model = new CreateMetricDefinitionDto
            {
                MetricCode = metric.MetricCode,
                MetricName = metric.MetricName,
                SubCategoryId = metric.SubCategoryId,
                Description = metric.Description,
                SourceType = metric.SourceType,
                DataType = metric.DataType,
                UnitId = metric.UnitId,
                AggregationType = metric.AggregationType,
                IsKPI = metric.IsKPI,
                ThresholdGreen = metric.ThresholdGreen,
                ThresholdYellow = metric.ThresholdYellow,
                ThresholdRed = metric.ThresholdRed,
                ExpectedValue = metric.ExpectedValue,
                ComplianceRule = metric.ComplianceRule,
                IsActive = metric.IsActive
            };

            ViewBag.MetricId = id;
            ViewBag.IsEdit = true;
            await LoadHierarchicalDropdowns();

            return View("~/Views/Metrics/MetricDefinitions/Edit.cshtml", model);
        }

        /// <summary>
        /// Handle metric definition update
        /// </summary>
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateMetricDefinitionDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MetricId = id;
                ViewBag.IsEdit = true;
                await LoadHierarchicalDropdowns();
                return View("~/Views/Metrics/MetricDefinitions/Edit.cshtml", model);
            }

            var metric = await _context.MetricDefinitions.FindAsync(id);

            if (metric == null)
            {
                TempData["ErrorMessage"] = "Metric definition not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check for duplicate metric code (excluding current metric)
            var duplicateExists = await _context.MetricDefinitions
                .AnyAsync(m => m.MetricCode == model.MetricCode && m.MetricId != id);

            if (duplicateExists)
            {
                ModelState.AddModelError("MetricCode", "A metric with this code already exists.");
                ViewBag.MetricId = id;
                ViewBag.IsEdit = true;
                await LoadHierarchicalDropdowns();
                return View("~/Views/Metrics/MetricDefinitions/Edit.cshtml", model);
            }

            // Update metric
            metric.MetricCode = model.MetricCode;
            metric.MetricName = model.MetricName;
            metric.SubCategoryId = model.SubCategoryId;
            metric.Description = model.Description;
            metric.SourceType = model.SourceType;
            metric.DataType = model.DataType;
            metric.UnitId = model.UnitId;
            metric.AggregationType = model.AggregationType;
            metric.IsKPI = model.IsKPI;
            metric.ThresholdGreen = model.ThresholdGreen;
            metric.ThresholdYellow = model.ThresholdYellow;
            metric.ThresholdRed = model.ThresholdRed;
            metric.ExpectedValue = model.ExpectedValue;
            metric.ComplianceRule = model.ComplianceRule;
            metric.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Metric '{metric.MetricName}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Load hierarchical dropdown data for categories, subcategories and units
        /// </summary>
        private async Task LoadHierarchicalDropdowns()
        {
            // Load categories for hierarchical selection
            ViewBag.MetricCategories = await _context.MetricCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            // Load all subcategories grouped by category for client-side filtering
            ViewBag.MetricSubCategories = await _context.MetricSubCategories
                .Include(sc => sc.Category)
                .Where(sc => sc.IsActive && sc.Category.IsActive)
                .OrderBy(sc => sc.Category.DisplayOrder)
                .ThenBy(sc => sc.DisplayOrder)
                .ToListAsync();

            // Load all units for client-side filtering
            ViewBag.MetricUnits = await _context.MetricUnits
                .Where(u => u.IsActive)
                .OrderBy(u => u.DisplayOrder)
                .ToListAsync();
        }

        /// <summary>
        /// API: Get subcategories for a specific category
        /// </summary>
        [HttpGet("api/subcategories/{categoryId}")]
        public async Task<IActionResult> GetSubCategories(int categoryId)
        {
            var subcategories = await _context.MetricSubCategories
                .Where(sc => sc.CategoryId == categoryId && sc.IsActive)
                .OrderBy(sc => sc.DisplayOrder)
                .Select(sc => new
                {
                    sc.SubCategoryId,
                    sc.SubCategoryCode,
                    sc.SubCategoryName,
                    sc.Description,
                    sc.AllowedScopes,
                    sc.DefaultScope,
                    sc.AllowedDataTypes,
                    sc.AllowedAggregationTypes,
                    sc.DefaultDataType,
                    sc.DefaultAggregationType,
                    sc.DefaultUnitId,
                    sc.SuggestedThresholdGreen,
                    sc.SuggestedThresholdYellow,
                    sc.SuggestedThresholdRed
                })
                .ToListAsync();

            return Json(subcategories);
        }

        /// <summary>
        /// API: Get units for a specific subcategory
        /// </summary>
        [HttpGet("api/subcategory-units/{subCategoryId}")]
        public async Task<IActionResult> GetSubCategoryUnits(int subCategoryId)
        {
            var units = await _context.MetricSubCategoryUnits
                .Include(scu => scu.Unit)
                .Where(scu => scu.SubCategoryId == subCategoryId && scu.Unit.IsActive)
                .OrderBy(scu => scu.DisplayOrder)
                .Select(scu => new
                {
                    scu.Unit.UnitId,
                    scu.Unit.UnitCode,
                    scu.Unit.UnitName,
                    scu.Unit.UnitSymbol,
                    scu.IsDefault
                })
                .ToListAsync();

            return Json(units);
        }

        /// <summary>
        /// API: Validate scope constraints for a subcategory
        /// </summary>
        [HttpGet("api/validate-scope/{subCategoryId}/{scope}")]
        public async Task<IActionResult> ValidateScopeConstraints(int subCategoryId, string scope)
        {
            var subcategory = await _context.MetricSubCategories
                .FirstOrDefaultAsync(sc => sc.SubCategoryId == subCategoryId);

            if (subcategory == null)
            {
                return Json(new { isValid = false, message = "Subcategory not found" });
            }

            var allowedScopes = subcategory.AllowedScopes?.Split(',').Select(s => s.Trim()).ToList() ?? new List<string>();
            var isValid = allowedScopes.Contains(scope);

            return Json(new 
            { 
                isValid, 
                allowedScopes,
                defaultScope = subcategory.DefaultScope,
                message = isValid ? "Valid scope" : $"Invalid scope. Allowed: {string.Join(", ", allowedScopes)}"
            });
        }
    }
}
