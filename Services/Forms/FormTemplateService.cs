using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using System.Text.RegularExpressions;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service implementation for FormTemplate operations
    /// Handles template code generation, uniqueness checking, and validation
    /// </summary>
    public class FormTemplateService : IFormTemplateService
    {
        private readonly ApplicationDbContext _context;
        private const int MAX_CODE_LENGTH = 50;
        private const string CODE_PREFIX = "TPL_";

        public FormTemplateService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Generate a unique template code from template name
        /// Adds numeric suffix (_2, _3, etc.) if code already exists
        /// </summary>
        public async Task<string> GenerateUniqueTemplateCodeAsync(string templateName, int? excludeTemplateId = null)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentException("Template name cannot be empty", nameof(templateName));
            }

            // Generate base code
            var baseCode = GenerateTemplateCode(templateName);

            // Check if base code is unique
            if (!await TemplateCodeExistsAsync(baseCode, excludeTemplateId))
            {
                return baseCode;
            }

            // Find unique suffix
            int suffix = 2;
            string uniqueCode;

            do
            {
                // Calculate max length for base to leave room for suffix
                var suffixStr = $"_{suffix}";
                var maxBaseLength = MAX_CODE_LENGTH - suffixStr.Length;

                // Truncate base code if needed and add suffix
                uniqueCode = baseCode.Substring(0, Math.Min(baseCode.Length, maxBaseLength)) + suffixStr;
                suffix++;

                // Safety check to prevent infinite loop
                if (suffix > 999)
                {
                    throw new InvalidOperationException($"Could not generate unique code for template name: {templateName}");
                }
            }
            while (await TemplateCodeExistsAsync(uniqueCode, excludeTemplateId));

            return uniqueCode;
        }

        /// <summary>
        /// Check if template code already exists in database
        /// </summary>
        public async Task<bool> TemplateCodeExistsAsync(string templateCode, int? excludeTemplateId = null)
        {
            if (string.IsNullOrWhiteSpace(templateCode))
            {
                return false;
            }

            var query = _context.FormTemplates
                .Where(t => t.TemplateCode == templateCode);

            // Exclude specific template ID (for edit scenarios)
            if (excludeTemplateId.HasValue)
            {
                query = query.Where(t => t.TemplateId != excludeTemplateId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Generate template code from template name (without uniqueness check)
        /// Format: TPL_UPPERCASE_NAME (max 50 chars)
        /// </summary>
        public string GenerateTemplateCode(string templateName)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                return CODE_PREFIX + "UNNAMED";
            }

            // Convert to uppercase and replace non-alphanumeric with underscores
            var sanitized = templateName.ToUpperInvariant();
            sanitized = Regex.Replace(sanitized, @"[^A-Z0-9]+", "_");

            // Remove leading/trailing underscores
            sanitized = sanitized.Trim('_');

            // Add prefix and ensure max length
            var code = CODE_PREFIX + sanitized;

            if (code.Length > MAX_CODE_LENGTH)
            {
                code = code.Substring(0, MAX_CODE_LENGTH);
            }

            // Remove trailing underscore if truncation created one
            code = code.TrimEnd('_');

            return code;
        }

        /// <summary>
        /// Validate template code format
        /// Must start with TPL_, contain only uppercase letters, numbers, and underscores
        /// </summary>
        public bool IsValidTemplateCodeFormat(string templateCode)
        {
            if (string.IsNullOrWhiteSpace(templateCode))
            {
                return false;
            }

            // Must start with TPL_ and contain only uppercase letters, numbers, and underscores
            var pattern = @"^TPL_[A-Z0-9_]+$";
            return Regex.IsMatch(templateCode, pattern) && templateCode.Length <= MAX_CODE_LENGTH;
        }
    }
}
