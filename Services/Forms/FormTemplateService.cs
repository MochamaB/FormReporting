using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Models.ViewModels.Components;
using System.Text.RegularExpressions;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service implementation for FormTemplate operations
    /// Handles template code generation, uniqueness checking, validation, and progress tracking
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

        /// <summary>
        /// Load template with all related entities for editing/resume
        /// Includes: Sections, Items, Assignments, Workflow, Category, MetricMappings
        /// </summary>
        public async Task<FormTemplate?> LoadTemplateForEditingAsync(int templateId)
        {
            return await _context.FormTemplates
                .Include(t => t.Category)
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Items)
                        .ThenInclude(i => i.MetricMappings)
                .Include(t => t.Items)
                    .ThenInclude(i => i.MetricMappings)
                .Include(t => t.Assignments)
                .Include(t => t.Workflow)
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);
        }

        /// <summary>
        /// Analyze template progress and determine current step for resume functionality
        /// Uses existing data to intelligently detect which step user should resume from
        /// Simplified 3-step wizard: Setup → Build → Publish
        /// Note: Assignments, Workflow, Metrics, Reports are configured AFTER publish
        /// </summary>
        public FormBuilderResumeInfo AnalyzeTemplateProgress(FormTemplate template)
        {
            var completedSteps = new Dictionary<FormBuilderStep, bool>();
            var stepStatuses = new Dictionary<FormBuilderStep, StepStatus>();

            // ═══════════════════════════════════════════════════════════
            // STEP 1: TEMPLATE SETUP
            // ═══════════════════════════════════════════════════════════
            bool step1Complete =
                !string.IsNullOrEmpty(template.TemplateName) &&
                !string.IsNullOrEmpty(template.TemplateCode) &&
                template.CategoryId > 0 &&
                !string.IsNullOrEmpty(template.TemplateType);

            completedSteps[FormBuilderStep.TemplateSetup] = step1Complete;
            stepStatuses[FormBuilderStep.TemplateSetup] = step1Complete
                ? StepStatus.Completed
                : StepStatus.Active;

            // ═══════════════════════════════════════════════════════════
            // STEP 2: FORM BUILDER
            // ═══════════════════════════════════════════════════════════
            // Check if template has sections and each section has at least one field
            bool hasSections = template.Sections != null && template.Sections.Any();
            bool allSectionsHaveFields = hasSections && template.Sections.All(s => s.Items != null && s.Items.Any());
            bool step2Complete = hasSections && allSectionsHaveFields;

            completedSteps[FormBuilderStep.FormBuilder] = step2Complete;
            stepStatuses[FormBuilderStep.FormBuilder] =
                !step1Complete ? StepStatus.Pending :
                step2Complete ? StepStatus.Completed :
                StepStatus.Active;

            // ═══════════════════════════════════════════════════════════
            // STEP 3: REVIEW & PUBLISH
            // ═══════════════════════════════════════════════════════════
            bool step3Complete = template.PublishStatus == "Published";

            completedSteps[FormBuilderStep.ReviewPublish] = step3Complete;
            stepStatuses[FormBuilderStep.ReviewPublish] =
                !step2Complete ? StepStatus.Pending :
                step3Complete ? StepStatus.Completed :
                StepStatus.Active;

            // ═══════════════════════════════════════════════════════════
            // DETERMINE CURRENT STEP (First incomplete step)
            // Order: Setup → Build → Publish
            // ═══════════════════════════════════════════════════════════
            FormBuilderStep currentStep = FormBuilderStep.TemplateSetup;

            if (!step1Complete)
                currentStep = FormBuilderStep.TemplateSetup;
            else if (!step2Complete)
                currentStep = FormBuilderStep.FormBuilder;
            else
                currentStep = FormBuilderStep.ReviewPublish;

            // ═══════════════════════════════════════════════════════════
            // CALCULATE COMPLETION PERCENTAGE
            // ═══════════════════════════════════════════════════════════
            int completedCount = completedSteps.Count(kvp => kvp.Value);
            int completionPercentage = (completedCount * 100) / 3;

            return new FormBuilderResumeInfo
            {
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                TemplateCode = template.TemplateCode,
                PublishStatus = template.PublishStatus,
                CurrentStep = currentStep,
                CompletedSteps = completedSteps,
                StepStatuses = stepStatuses,
                CompletionPercentage = completionPercentage,
                LastModifiedDate = template.ModifiedDate
            };
        }

        /// <summary>
        /// Create a new version from a published template
        /// Copies template data, sections, items, and assignments
        /// Increments version number and sets status to Draft
        /// </summary>
        public async Task<FormTemplate> CreateNewVersionAsync(int publishedTemplateId, int userId)
        {
            // Load the published template with all related data
            var publishedTemplate = await _context.FormTemplates
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Items)
                .Include(t => t.Items)
                .Include(t => t.Assignments)
                .FirstOrDefaultAsync(t => t.TemplateId == publishedTemplateId);

            if (publishedTemplate == null)
                throw new InvalidOperationException($"Template with ID {publishedTemplateId} not found.");

            if (!CanCreateVersion(publishedTemplate))
                throw new InvalidOperationException($"Cannot create version from template with status '{publishedTemplate.PublishStatus}'. Only published templates can be versioned.");

            // Create new template version
            var newVersion = new FormTemplate
            {
                TemplateName = publishedTemplate.TemplateName,
                TemplateCode = publishedTemplate.TemplateCode,
                Description = publishedTemplate.Description,
                CategoryId = publishedTemplate.CategoryId,
                TemplateType = publishedTemplate.TemplateType,
                Version = publishedTemplate.Version + 1, // Increment version
                IsActive = true,
                RequiresApproval = publishedTemplate.RequiresApproval,
                WorkflowId = publishedTemplate.WorkflowId,
                PublishStatus = "Draft", // New version starts as Draft
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = userId
            };

            // Add new template to context
            _context.FormTemplates.Add(newVersion);
            await _context.SaveChangesAsync(); // Save to get TemplateId

            // Copy sections and items
            var sectionMapping = new Dictionary<int, int>(); // Old section ID -> New section ID

            foreach (var oldSection in publishedTemplate.Sections.OrderBy(s => s.DisplayOrder))
            {
                var newSection = new FormTemplateSection
                {
                    TemplateId = newVersion.TemplateId,
                    SectionName = oldSection.SectionName,
                    SectionDescription = oldSection.SectionDescription,
                    DisplayOrder = oldSection.DisplayOrder,
                    IsCollapsible = oldSection.IsCollapsible,
                    IsCollapsedByDefault = oldSection.IsCollapsedByDefault,
                    IconClass = oldSection.IconClass,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };

                _context.FormTemplateSections.Add(newSection);
                await _context.SaveChangesAsync(); // Save to get SectionId

                sectionMapping[oldSection.SectionId] = newSection.SectionId;

                // Copy items for this section
                foreach (var oldItem in oldSection.Items.OrderBy(i => i.DisplayOrder))
                {
                    var newItem = new FormTemplateItem
                    {
                        TemplateId = newVersion.TemplateId,
                        SectionId = newSection.SectionId,
                        ItemCode = oldItem.ItemCode,
                        ItemName = oldItem.ItemName,
                        ItemDescription = oldItem.ItemDescription,
                        DisplayOrder = oldItem.DisplayOrder,
                        DataType = oldItem.DataType,
                        IsRequired = oldItem.IsRequired,
                        DefaultValue = oldItem.DefaultValue,
                        PlaceholderText = oldItem.PlaceholderText,
                        HelpText = oldItem.HelpText,
                        PrefixText = oldItem.PrefixText,
                        SuffixText = oldItem.SuffixText,
                        ConditionalLogic = oldItem.ConditionalLogic,
                        LayoutType = oldItem.LayoutType,
                        MatrixGroupId = oldItem.MatrixGroupId,
                        MatrixRowLabel = oldItem.MatrixRowLabel,
                        LibraryFieldId = oldItem.LibraryFieldId,
                        IsLibraryOverride = oldItem.IsLibraryOverride,
                        Version = 1, // Reset item version for new template version
                        IsActive = oldItem.IsActive,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.FormTemplateItems.Add(newItem);
                }
            }

            // Copy assignments
            foreach (var oldAssignment in publishedTemplate.Assignments)
            {
                var newAssignment = new FormTemplateAssignment
                {
                    TemplateId = newVersion.TemplateId,
                    AssignmentType = oldAssignment.AssignmentType,
                    TenantType = oldAssignment.TenantType,
                    TenantGroupId = oldAssignment.TenantGroupId,
                    TenantId = oldAssignment.TenantId,
                    RoleId = oldAssignment.RoleId,
                    DepartmentId = oldAssignment.DepartmentId,
                    UserGroupId = oldAssignment.UserGroupId,
                    UserId = oldAssignment.UserId,
                    AssignedBy = userId,
                    AssignedDate = DateTime.UtcNow,
                    IsActive = oldAssignment.IsActive,
                    Notes = $"Copied from v{publishedTemplate.Version}"
                };

                _context.FormTemplateAssignments.Add(newAssignment);
            }

            // Save all changes
            await _context.SaveChangesAsync();

            // Reload the new template with all relationships
            return await LoadTemplateForEditingAsync(newVersion.TemplateId) 
                ?? throw new InvalidOperationException("Failed to reload new version after creation.");
        }

        /// <summary>
        /// Check if a new version can be created from this template
        /// Only published templates can be versioned
        /// </summary>
        public bool CanCreateVersion(FormTemplate template)
        {
            return template.PublishStatus == "Published";
        }
    }
}
