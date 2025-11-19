using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.Entities.Forms;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service implementation for FormCategory operations
    /// Handles all business logic related to form categories
    /// </summary>
    public class FormCategoryService : IFormCategoryService
    {
        private readonly ApplicationDbContext _context;

        public FormCategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all active categories ordered by display order
        /// </summary>
        public async Task<List<FormCategory>> GetActiveCategoriesAsync()
        {
            return await _context.FormCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();
        }

        /// <summary>
        /// Get active categories as select list items
        /// Returns anonymous objects optimized for dropdown binding
        /// </summary>
        public async Task<List<object>> GetCategorySelectListAsync()
        {
            return await _context.FormCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .Select(c => new
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryCode = c.CategoryCode,
                    IconClass = c.IconClass,
                    Color = c.Color
                })
                .ToListAsync<object>();
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        public async Task<FormCategory?> GetByIdAsync(int categoryId)
        {
            return await _context.FormCategories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }

        /// <summary>
        /// Get all categories (including inactive)
        /// </summary>
        public async Task<List<FormCategory>> GetAllCategoriesAsync()
        {
            return await _context.FormCategories
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();
        }

        /// <summary>
        /// Check if category exists and is active
        /// </summary>
        public async Task<bool> IsActiveCategoryAsync(int categoryId)
        {
            return await _context.FormCategories
                .AnyAsync(c => c.CategoryId == categoryId && c.IsActive);
        }

        /// <summary>
        /// Get category name by ID
        /// </summary>
        public async Task<string?> GetCategoryNameAsync(int categoryId)
        {
            return await _context.FormCategories
                .Where(c => c.CategoryId == categoryId)
                .Select(c => c.CategoryName)
                .FirstOrDefaultAsync();
        }
    }
}
