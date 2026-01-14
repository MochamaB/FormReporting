using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Models.Entities.Forms;

namespace FormReporting.Controllers.Forms
{
    /// <summary>
    /// Controller for managing form categories
    /// </summary>
    [Route("Forms/[controller]")]
    public class FormCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FormCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Display the form categories index page with datatable
        /// </summary>
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var categories = await _context.FormCategories
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .Select(c => new FormCategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryCode = c.CategoryCode,
                    Description = c.Description,
                    IconClass = c.IconClass,
                    Color = c.Color,
                    DisplayOrder = c.DisplayOrder,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate,
                    ModifiedDate = c.ModifiedDate,
                    FormCount = c.FormTemplates.Count
                })
                .ToListAsync();

            var viewModel = new FormCategoriesIndexViewModel
            {
                Categories = categories,
                TotalCategories = categories.Count,
                ActiveCategories = categories.Count(c => c.IsActive),
                InactiveCategories = categories.Count(c => !c.IsActive),
                TotalForms = categories.Sum(c => c.FormCount)
            };

            return View("~/Views/Forms/FormCategories/Index.cshtml",viewModel);
        }

        /// <summary>
        /// API endpoint to get categories data for DataTables
        /// </summary>
        [HttpGet("GetCategoriesData")]
        public async Task<IActionResult> GetCategoriesData()
        {
            var categories = await _context.FormCategories
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .Select(c => new FormCategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryCode = c.CategoryCode,
                    Description = c.Description,
                    IconClass = c.IconClass,
                    Color = c.Color,
                    DisplayOrder = c.DisplayOrder,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate,
                    ModifiedDate = c.ModifiedDate,
                    FormCount = c.FormTemplates.Count
                })
                .ToListAsync();

            return Json(new { data = categories });
        }

        /// <summary>
        /// Show create form category page
        /// </summary>
        [HttpGet("Create")]
        public IActionResult Create()
        {
            var viewModel = new FormCategoryEditViewModel
            {
                DisplayOrder = 0,
                IsActive = true
            };

            return View("~/Views/Forms/FormCategories/Create.cshtml",viewModel);
        }

        /// <summary>
        /// Handle create form category submission
        /// </summary>
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FormCategoryEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var category = new FormCategory
            {
                CategoryName = model.CategoryName,
                CategoryCode = model.CategoryCode,
                Description = model.Description,
                IconClass = model.IconClass,
                Color = model.Color,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            _context.FormCategories.Add(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Form category '{category.CategoryName}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Show edit form category page
        /// </summary>
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.FormCategories.FindAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Form category not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new FormCategoryEditViewModel
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                CategoryCode = category.CategoryCode,
                Description = category.Description,
                IconClass = category.IconClass,
                Color = category.Color,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            };

            return View("~/Views/Forms/FormCategories/Create.cshtml",viewModel);
        }

        /// <summary>
        /// Handle edit form category submission
        /// </summary>
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FormCategoryEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var category = await _context.FormCategories.FindAsync(model.CategoryId);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Form category not found.";
                return RedirectToAction(nameof(Index));
            }

            category.CategoryName = model.CategoryName;
            category.CategoryCode = model.CategoryCode;
            category.Description = model.Description;
            category.IconClass = model.IconClass;
            category.Color = model.Color;
            category.DisplayOrder = model.DisplayOrder;
            category.IsActive = model.IsActive;
            category.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Form category '{category.CategoryName}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Display form category details with template lists by status
        /// </summary>
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id, string? status = null, string? search = null, int? page = null)
        {
            // Get category details first (without templates)
            var category = await _context.FormCategories
                .FirstOrDefaultAsync(c => c.CategoryId == id);
            
            if (category == null)
            {
                TempData["ErrorMessage"] = "Form category not found.";
                return RedirectToAction(nameof(Index));
            }

            // Parse query parameters for tab navigation
            var activeTab = status?.ToLower() ?? "published";
            var currentPage = page ?? 1;
            var pageSize = 10;

            // Build EF query for templates (database-side filtering)
            var templatesQuery = _context.FormTemplates
                .Include(t => t.Creator) // Include FIRST
                .Where(t => t.CategoryId == id); // Then filter

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                templatesQuery = templatesQuery.Where(t => t.PublishStatus == status);
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                templatesQuery = templatesQuery.Where(t => 
                    t.TemplateName.Contains(search) || 
                    t.TemplateCode.Contains(search) ||
                    (t.Description != null && t.Description.Contains(search)));
            }

            // Order and paginate
            var totalTemplates = await templatesQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalTemplates / (double)pageSize);
            var skip = (currentPage - 1) * pageSize;

            var templates = await templatesQuery
                .OrderBy(t => t.TemplateName)
                .Skip(skip)
                .Take(pageSize)
                .Select(t => new FormTemplateViewModel
                {
                    TemplateId = t.TemplateId,
                    TemplateName = t.TemplateName,
                    TemplateCode = t.TemplateCode,
                    Description = t.Description,
                    TemplateType = t.TemplateType,
                    Version = t.Version,
                    PublishStatus = t.PublishStatus,
                    IsActive = t.IsActive,
                    ModifiedDate = t.ModifiedDate,
                    CreatedBy = t.Creator != null ? $"{t.Creator.FirstName} {t.Creator.LastName}" : "System",
                    SubmissionCount = 0, // TODO: Calculate from submissions if needed
                    SubmissionMode = t.SubmissionMode,
                    AllowAnonymousAccess = t.AllowAnonymousAccess
                })
                .ToListAsync();

            // Calculate template statistics (separate queries for efficiency)
            var allTemplatesCount = await _context.FormTemplates
                .Where(t => t.CategoryId == id)
                .CountAsync();
            
            var publishedTemplatesCount = await _context.FormTemplates
                .Where(t => t.CategoryId == id && t.PublishStatus == "Published")
                .CountAsync();
            
            var draftTemplatesCount = await _context.FormTemplates
                .Where(t => t.CategoryId == id && t.PublishStatus == "Draft")
                .CountAsync();
            
            var archivedTemplatesCount = await _context.FormTemplates
                .Where(t => t.CategoryId == id && t.PublishStatus == "Archived")
                .CountAsync();

            // Build ViewModel
            var viewModel = new FormCategoryDetailsViewModel
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                CategoryCode = category.CategoryCode,
                Description = category.Description,
                IconClass = category.IconClass,
                Color = category.Color,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive,
                CreatedDate = category.CreatedDate,
                ModifiedDate = category.ModifiedDate,
                
                // Template statistics
                TotalTemplates = allTemplatesCount,
                PublishedTemplates = publishedTemplatesCount,
                DraftTemplates = draftTemplatesCount,
                ArchivedTemplates = archivedTemplatesCount,
                
                // Current tab data
                ActiveTab = activeTab,
                Templates = templates,
                
                // Pagination data
                CurrentPage = currentPage,
                TotalPages = totalPages,
                TotalRecords = totalTemplates,
                PageSize = pageSize,
                CurrentSearch = search,
                CurrentStatus = status
            };

            return View("~/Views/Forms/FormCategories/Details.cshtml", viewModel);
        }

        /// <summary>
        /// Delete a form category
        /// </summary>
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.FormCategories
                .Include(c => c.FormTemplates)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return Json(new { success = false, message = "Form category not found." });
            }

            // Check if category has associated forms
            if (category.FormTemplates.Any())
            {
                return Json(new 
                { 
                    success = false, 
                    message = $"Cannot delete '{category.CategoryName}' because it has {category.FormTemplates.Count} associated form(s). Please reassign or delete the forms first." 
                });
            }

            _context.FormCategories.Remove(category);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Form category '{category.CategoryName}' deleted successfully." });
        }
    }
}
