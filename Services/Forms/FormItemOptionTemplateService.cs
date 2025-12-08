using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.Entities.Forms;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service implementation for FormItemOptionTemplate operations
    /// Handles all business logic related to option templates
    /// </summary>
    public class FormItemOptionTemplateService : IFormItemOptionTemplateService
    {
        private readonly ApplicationDbContext _context;

        public FormItemOptionTemplateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<FormItemOptionTemplate>> GetActiveTemplatesAsync()
        {
            return await _context.FormItemOptionTemplates
                .Include(t => t.Items.OrderBy(i => i.DisplayOrder))
                .Where(t => t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.TemplateName)
                .ToListAsync();
        }

        public async Task<List<FormItemOptionTemplate>> GetTemplatesByCategoryAsync(string category)
        {
            return await _context.FormItemOptionTemplates
                .Include(t => t.Items.OrderBy(i => i.DisplayOrder))
                .Where(t => t.IsActive && t.Category == category)
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.TemplateName)
                .ToListAsync();
        }

        public async Task<List<FormItemOptionTemplate>> GetTemplatesByFieldTypeAsync(string fieldType)
        {
            return await _context.FormItemOptionTemplates
                .Include(t => t.Items.OrderBy(i => i.DisplayOrder))
                .Where(t => t.IsActive && 
                           (t.ApplicableFieldTypes == null || 
                            t.ApplicableFieldTypes.Contains(fieldType)))
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.TemplateName)
                .ToListAsync();
        }

        public async Task<FormItemOptionTemplate?> GetByIdWithItemsAsync(int templateId)
        {
            return await _context.FormItemOptionTemplates
                .Include(t => t.Items.OrderBy(i => i.DisplayOrder))
                .Include(t => t.Tenant)
                .Include(t => t.Creator)
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);
        }

        public async Task<FormItemOptionTemplate?> GetByCodeWithItemsAsync(string templateCode)
        {
            return await _context.FormItemOptionTemplates
                .Include(t => t.Items.OrderBy(i => i.DisplayOrder))
                .FirstOrDefaultAsync(t => t.TemplateCode == templateCode);
        }

        public async Task<List<FormItemOptionTemplate>> GetAllTemplatesWithItemsAsync()
        {
            return await _context.FormItemOptionTemplates
                .Include(t => t.Items.OrderBy(i => i.DisplayOrder))
                .Include(t => t.Tenant)
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.TemplateName)
                .ToListAsync();
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.FormItemOptionTemplates
                .Where(t => !string.IsNullOrEmpty(t.Category))
                .Select(t => t.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<List<object>> GetTemplateSelectListAsync(string? fieldType = null)
        {
            var query = _context.FormItemOptionTemplates
                .Where(t => t.IsActive);

            if (!string.IsNullOrEmpty(fieldType))
            {
                query = query.Where(t => t.ApplicableFieldTypes == null || 
                                        t.ApplicableFieldTypes.Contains(fieldType));
            }

            return await query
                .OrderBy(t => t.Category)
                .ThenBy(t => t.DisplayOrder)
                .ThenBy(t => t.TemplateName)
                .Select(t => new
                {
                    TemplateId = t.TemplateId,
                    TemplateName = t.TemplateName,
                    TemplateCode = t.TemplateCode,
                    Category = t.Category,
                    HasScoring = t.HasScoring,
                    OptionCount = t.Items.Count // Count without loading items
                })
                .ToListAsync<object>();
        }

        public async Task IncrementUsageCountAsync(int templateId)
        {
            var template = await _context.FormItemOptionTemplates
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

            if (template != null)
            {
                template.UsageCount++;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> TemplateCodeExistsAsync(string templateCode, int? excludeTemplateId = null)
        {
            var query = _context.FormItemOptionTemplates
                .Where(t => t.TemplateCode == templateCode);

            if (excludeTemplateId.HasValue)
            {
                query = query.Where(t => t.TemplateId != excludeTemplateId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<(List<FormItemOptionTemplate> templates, int totalCount)> GetTemplatesPagedAsync(
            string? search = null,
            string? category = null,
            string? status = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.FormItemOptionTemplates
                .Include(t => t.Items)
                .Include(t => t.Tenant)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t =>
                    t.TemplateName.Contains(search) ||
                    t.TemplateCode.Contains(search) ||
                    (t.Description != null && t.Description.Contains(search)) ||
                    (t.Category != null && t.Category.Contains(search)));
            }

            // Apply category filter
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(t => t.Category == category);
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status.ToLower() == "active";
                query = query.Where(t => t.IsActive == isActive);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var skip = (page - 1) * pageSize;
            var templates = await query
                .OrderBy(t => t.Category)
                .ThenBy(t => t.DisplayOrder)
                .ThenBy(t => t.TemplateName)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return (templates, totalCount);
        }

        public async Task<FormItemOptionTemplate> CreateTemplateAsync(FormItemOptionTemplate template)
        {
            template.CreatedDate = DateTime.UtcNow;
            _context.FormItemOptionTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task UpdateTemplateAsync(FormItemOptionTemplate template)
        {
            template.ModifiedDate = DateTime.UtcNow;
            _context.FormItemOptionTemplates.Update(template);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteTemplateAsync(int templateId)
        {
            var template = await _context.FormItemOptionTemplates
                .Include(t => t.Items)
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

            if (template == null)
            {
                return false;
            }

            _context.FormItemOptionTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
