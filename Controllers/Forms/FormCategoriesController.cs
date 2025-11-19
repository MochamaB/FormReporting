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
