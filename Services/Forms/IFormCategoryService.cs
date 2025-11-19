using FormReporting.Models.Entities.Forms;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service interface for FormCategory operations
    /// Provides methods for retrieving and managing form categories
    /// </summary>
    public interface IFormCategoryService
    {
        /// <summary>
        /// Get all active categories ordered by display order
        /// </summary>
        /// <returns>List of active categories</returns>
        Task<List<FormCategory>> GetActiveCategoriesAsync();

        /// <summary>
        /// Get active categories as select list items (Id, Name)
        /// Useful for dropdown population
        /// </summary>
        /// <returns>Anonymous objects with CategoryId and CategoryName</returns>
        Task<List<object>> GetCategorySelectListAsync();

        /// <summary>
        /// Get category by ID
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>Category if found, null otherwise</returns>
        Task<FormCategory?> GetByIdAsync(int categoryId);

        /// <summary>
        /// Get all categories (including inactive)
        /// </summary>
        /// <returns>List of all categories</returns>
        Task<List<FormCategory>> GetAllCategoriesAsync();

        /// <summary>
        /// Check if category exists and is active
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>True if exists and active, false otherwise</returns>
        Task<bool> IsActiveCategoryAsync(int categoryId);

        /// <summary>
        /// Get category name by ID
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>Category name if found, null otherwise</returns>
        Task<string?> GetCategoryNameAsync(int categoryId);
    }
}
