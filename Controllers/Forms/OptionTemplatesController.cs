using Microsoft.AspNetCore.Mvc;
using FormReporting.Services.Forms;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.ViewModels.Components;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Controllers.Forms
{
    /// <summary>
    /// Controller for managing form item option templates
    /// Provides pre-defined option sets for rapid form building
    /// </summary>
    [Route("Forms/[controller]")]
    public class OptionTemplatesController : Controller
    {
        private readonly IFormItemOptionTemplateService _templateService;

        public OptionTemplatesController(IFormItemOptionTemplateService templateService)
        {
            _templateService = templateService;
        }

        /// <summary>
        /// Display the option templates index page with filtering and pagination
        /// </summary>
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? search, string? category, string? status, int? page)
        {
            // 1. SET PAGINATION PARAMETERS
            var pageSize = 15;
            var currentPage = page ?? 1;

            // 2. GET PAGINATED TEMPLATES WITH FILTERS
            var (templates, totalRecords) = await _templateService.GetTemplatesPagedAsync(
                search: search,
                category: category,
                status: status,
                page: currentPage,
                pageSize: pageSize
            );

            // 3. GET AVAILABLE CATEGORIES FOR FILTER DROPDOWN
            var categories = await _templateService.GetCategoriesAsync();

            // 4. CALCULATE PAGINATION
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // 5. BUILD VIEW MODEL
            var viewModel = new OptionTemplatesIndexViewModel
            {
                Templates = templates.Select(t => new OptionTemplateViewModel
                {
                    TemplateId = t.TemplateId,
                    TemplateName = t.TemplateName,
                    TemplateCode = t.TemplateCode,
                    Category = t.Category,
                    SubCategory = t.SubCategory,
                    Description = t.Description,
                    ItemCount = t.Items?.Count ?? 0,
                    UsageCount = t.UsageCount,
                    HasScoring = t.HasScoring,
                    ScoringType = t.ScoringType,
                    IsSystemTemplate = t.IsSystemTemplate,
                    IsActive = t.IsActive,
                    TenantName = t.Tenant?.TenantName,
                    CreatedDate = t.CreatedDate
                }),
                Categories = categories
            };

            // 6. PASS DATA TO VIEW VIA VIEWBAG
            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentStatus = status;

            return View("~/Views/Forms/OptionTemplates/Index.cshtml", viewModel);
        }

        /// <summary>
        /// Show template details
        /// </summary>
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var template = await _templateService.GetByIdWithItemsAsync(id);

            if (template == null)
            {
                return NotFound();
            }

            return View("~/Views/Forms/OptionTemplates/Details.cshtml", template);
        }

        /// <summary>
        /// Show create template page
        /// </summary>
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var model = new OptionTemplateEditViewModel
            {
                IsActive = true,
                HasScoring = false,
                DisplayOrder = 0,
                Items = new List<OptionTemplateItemEditViewModel>() // Initialize empty items list
            };

            await LoadDropdownOptions();
            return View("~/Views/Forms/OptionTemplates/Create.cshtml", model);
        }

        /// <summary>
        /// Handle create template submission
        /// </summary>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OptionTemplateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate template code
                if (await _templateService.TemplateCodeExistsAsync(model.TemplateCode))
                {
                    ModelState.AddModelError("TemplateCode", "A template with this code already exists.");
                    await LoadDropdownOptions();
                    return View("~/Views/Forms/OptionTemplates/Create.cshtml", model);
                }

                // Create template entity
                var template = new FormItemOptionTemplate
                {
                    TemplateName = model.TemplateName,
                    TemplateCode = model.TemplateCode.ToUpper(),
                    Category = model.Category,
                    SubCategory = model.SubCategory,
                    Description = model.Description,
                    DisplayOrder = model.DisplayOrder,
                    ApplicableFieldTypes = model.ApplicableFieldTypes,
                    RecommendedFor = model.RecommendedFor,
                    HasScoring = model.HasScoring,
                    ScoringType = model.ScoringType,
                    IsSystemTemplate = false, // Custom templates
                    TenantId = null, // Global for now
                    IsActive = model.IsActive,
                    UsageCount = 0
                };

                // Add items
                foreach (var itemModel in model.Items.OrderBy(i => i.DisplayOrder))
                {
                    template.Items.Add(new FormItemOptionTemplateItem
                    {
                        OptionValue = itemModel.OptionValue,
                        OptionLabel = itemModel.OptionLabel,
                        DisplayOrder = itemModel.DisplayOrder,
                        ScoreValue = itemModel.ScoreValue,
                        ScoreWeight = itemModel.ScoreWeight,
                        IconClass = itemModel.IconClass,
                        ColorHint = itemModel.ColorHint,
                        IsDefault = itemModel.IsDefault
                    });
                }

                await _templateService.CreateTemplateAsync(template);

                TempData["SuccessMessage"] = $"Option template '{template.TemplateName}' created successfully.";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownOptions();
            return View("~/Views/Forms/OptionTemplates/Create.cshtml", model);
        }

        /// <summary>
        /// Show edit template page
        /// </summary>
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var template = await _templateService.GetByIdWithItemsAsync(id);

            if (template == null)
            {
                TempData["ErrorMessage"] = "Template not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new OptionTemplateEditViewModel
            {
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                TemplateCode = template.TemplateCode,
                Category = template.Category,
                SubCategory = template.SubCategory,
                Description = template.Description,
                DisplayOrder = template.DisplayOrder,
                ApplicableFieldTypes = template.ApplicableFieldTypes,
                RecommendedFor = template.RecommendedFor,
                HasScoring = template.HasScoring,
                ScoringType = template.ScoringType,
                IsActive = template.IsActive,
                Items = template.Items?.Select(i => new OptionTemplateItemEditViewModel
                {
                    TemplateItemId = i.TemplateItemId,
                    OptionValue = i.OptionValue,
                    OptionLabel = i.OptionLabel,
                    DisplayOrder = i.DisplayOrder,
                    ScoreValue = i.ScoreValue,
                    ScoreWeight = i.ScoreWeight,
                    IconClass = i.IconClass,
                    ColorHint = i.ColorHint,
                    IsDefault = i.IsDefault
                }).ToList() ?? new List<OptionTemplateItemEditViewModel>()
            };

            await LoadDropdownOptions();
            return View("~/Views/Forms/OptionTemplates/Edit.cshtml", model);
        }

        /// <summary>
        /// Handle edit template submission
        /// </summary>
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OptionTemplateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var template = await _templateService.GetByIdWithItemsAsync(model.TemplateId);

                if (template == null)
                {
                    TempData["ErrorMessage"] = "Template not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check for duplicate template code (excluding current)
                if (await _templateService.TemplateCodeExistsAsync(model.TemplateCode, model.TemplateId))
                {
                    ModelState.AddModelError("TemplateCode", "A template with this code already exists.");
                    await LoadDropdownOptions();
                    return View("~/Views/Forms/OptionTemplates/Edit.cshtml", model);
                }

                // Update template properties
                template.TemplateName = model.TemplateName;
                template.TemplateCode = model.TemplateCode.ToUpper();
                template.Category = model.Category;
                template.SubCategory = model.SubCategory;
                template.Description = model.Description;
                template.DisplayOrder = model.DisplayOrder;
                template.ApplicableFieldTypes = model.ApplicableFieldTypes;
                template.RecommendedFor = model.RecommendedFor;
                template.HasScoring = model.HasScoring;
                template.ScoringType = model.ScoringType;
                template.IsActive = model.IsActive;

                // Remove existing items and add updated ones
                template.Items.Clear();
                foreach (var itemModel in model.Items.OrderBy(i => i.DisplayOrder))
                {
                    template.Items.Add(new FormItemOptionTemplateItem
                    {
                        OptionValue = itemModel.OptionValue,
                        OptionLabel = itemModel.OptionLabel,
                        DisplayOrder = itemModel.DisplayOrder,
                        ScoreValue = itemModel.ScoreValue,
                        ScoreWeight = itemModel.ScoreWeight,
                        IconClass = itemModel.IconClass,
                        ColorHint = itemModel.ColorHint,
                        IsDefault = itemModel.IsDefault
                    });
                }

                await _templateService.UpdateTemplateAsync(template);

                TempData["SuccessMessage"] = $"Option template '{template.TemplateName}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownOptions();
            return View("~/Views/Forms/OptionTemplates/Edit.cshtml", model);
        }

        /// <summary>
        /// Helper method to load dropdown options
        /// </summary>
        private async Task LoadDropdownOptions()
        {
            // Get existing categories for dropdown
            var categories = await _templateService.GetCategoriesAsync();
            ViewBag.Categories = categories;

            // Standard field types
            ViewBag.FieldTypes = new List<string>
            {
                "Radio",
                "Dropdown",
                "Checkbox",
                "Rating"
            };

            // Scoring types
            ViewBag.ScoringTypes = new List<string>
            {
                "Linear",
                "Custom",
                "Weighted",
                "Binary"
            };
        }
    }
}
