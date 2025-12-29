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
        private readonly IWorkflowService _workflowService;
        private const int MAX_CODE_LENGTH = 50;
        private const string CODE_PREFIX = "TPL_";

        public FormTemplateService(ApplicationDbContext context, IWorkflowService workflowService)
        {
            _context = context;
            _workflowService = workflowService;
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
            // DETERMINE CURRENT STEP FOR RESUME
            // Order: Setup → Build → Publish
            // Note: Always resume to FormBuilder if Step 1 is complete.
            // ReviewPublish should only be accessed via "Continue" button.
            // ═══════════════════════════════════════════════════════════
            FormBuilderStep currentStep = FormBuilderStep.TemplateSetup;

            if (!step1Complete)
                currentStep = FormBuilderStep.TemplateSetup;
            else
                currentStep = FormBuilderStep.FormBuilder; // Always go to FormBuilder, never auto-advance to ReviewPublish

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
                    EffectiveFrom = oldAssignment.EffectiveFrom,
                    EffectiveUntil = oldAssignment.EffectiveUntil,
                    AllowAnonymous = oldAssignment.AllowAnonymous,
                    Status = oldAssignment.Status,
                    AssignedBy = userId,
                    AssignedDate = DateTime.UtcNow,
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

        // ===== Template Readiness Validation Methods =====

        public async Task<TemplateReadinessDto> ValidateTemplateReadinessAsync(int templateId)
        {
            var result = new TemplateReadinessDto();

            // Load template with all necessary relationships
            var template = await _context.FormTemplates
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Items.Where(i => i.IsActive))
                .Include(t => t.Assignments.Where(a => a.Status == "Active"))
                .Include(t => t.Workflow)
                    .ThenInclude(w => w.Steps.OrderBy(s => s.StepOrder))
                        .ThenInclude(s => s.Action)
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

            if (template == null)
            {
                result.BlockingIssues.Add("Template not found");
                return result;
            }

            result.SubmissionMode = template.SubmissionMode.ToString();
            result.Configuration = await GetTemplateConfigurationStatusAsync(templateId);

            // Validate based on submission mode
            switch (template.SubmissionMode)
            {
                case Models.Common.SubmissionMode.Individual:
                    await ValidateIndividualModeReadiness(template, result);
                    break;

                case Models.Common.SubmissionMode.Collaborative:
                    await ValidateCollaborativeModeReadiness(template, result);
                    break;

                default:
                    result.BlockingIssues.Add("Invalid submission mode");
                    break;
            }

            result.IsReady = !result.BlockingIssues.Any();
            return result;
        }

        public async Task<bool> CanAcceptSubmissionsAsync(int templateId)
        {
            var readiness = await ValidateTemplateReadinessAsync(templateId);
            return readiness.IsReady;
        }

        public async Task<bool> IsReadyForCollaborativeWorkflowAsync(int templateId)
        {
            var template = await _context.FormTemplates.FindAsync(templateId);
            if (template?.SubmissionMode != Models.Common.SubmissionMode.Collaborative)
            {
                return false;
            }

            var readiness = await ValidateTemplateReadinessAsync(templateId);
            return readiness.IsReady;
        }

        public async Task<TemplateConfigurationStatusDto> GetTemplateConfigurationStatusAsync(int templateId)
        {
            var status = new TemplateConfigurationStatusDto
            {
                LastValidated = DateTime.UtcNow
            };

            var template = await _context.FormTemplates
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Items.Where(i => i.IsActive))
                .Include(t => t.Assignments.Where(a => a.Status == "Active"))
                .Include(t => t.Workflow)
                    .ThenInclude(w => w.Steps)
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

            if (template == null) return status;

            // Form Structure
            status.SectionCount = template.Sections.Count;
            status.FieldCount = template.Sections.SelectMany(s => s.Items).Count();
            status.HasFormStructure = status.SectionCount > 0 && status.FieldCount > 0;

            // Assignments
            var activeAssignments = template.Assignments.Where(a =>
                a.Status == "Active" &&
                a.EffectiveFrom <= DateTime.UtcNow &&
                (a.EffectiveUntil == null || a.EffectiveUntil >= DateTime.UtcNow)).ToList();

            status.ActiveAssignmentCount = activeAssignments.Count;
            status.HasAssignments = status.ActiveAssignmentCount > 0;
            status.AssignmentsCoverUsers = await CheckAssignmentCoverageAsync(activeAssignments);

            // Workflow
            status.HasWorkflow = template.WorkflowId.HasValue && template.Workflow != null;
            if (status.HasWorkflow)
            {
                status.WorkflowName = template.Workflow!.WorkflowName;
                status.WorkflowStepCount = template.Workflow.Steps.Count;

                // Validate workflow for current template mode
                var workflowValidation = await ValidateWorkflowForTemplateMode(template);
                status.WorkflowValidForMode = workflowValidation.IsValid;
                status.WorkflowIssues = workflowValidation.Errors;
            }

            // Template Status
            status.TemplateStatus = template.PublishStatus;

            // Overall readiness
            status.ReadyForSubmissions = await DetermineOverallReadiness(template, status);

            return status;
        }

        #region Private Validation Helper Methods

        private async Task ValidateIndividualModeReadiness(FormTemplate template, TemplateReadinessDto result)
        {
            var individualResult = await ValidateIndividualModeReadiness(template);
            result.BlockingIssues.AddRange(individualResult.BlockingIssues);
            result.Warnings.AddRange(individualResult.Warnings);
            result.IsReady = individualResult.IsReady;
        }

        private async Task<TemplateReadinessDto> ValidateIndividualModeReadiness(FormTemplate template)
        {
            var result = new TemplateReadinessDto
            {
                TemplateId = template.TemplateId,
                SubmissionMode = "Individual",
                IsReady = false
            };

            // 1. Check basic template requirements
            if (template.PublishStatus != "Published")
            {
                result.BlockingIssues.Add("Template must be published");
            }

            // 2. Check form structure
            var hasStructure = await HasFormStructure(template.TemplateId);
            if (!hasStructure)
            {
                result.BlockingIssues.Add("Template must have form structure (sections with fields)");
            }

            // 3. Handle Anonymous Access - no workflow or assignments required
            if (template.AllowAnonymousAccess)
            {
                result.Warnings.Add("Anonymous access enabled - no workflow or assignments required");
                // Skip assignment and workflow validation for anonymous forms
                result.IsReady = !result.BlockingIssues.Any();
                return result;
            }

            // 4. Check assignments - Individual mode requires assignments for access control (except anonymous)
            var hasAssignments = await HasActiveAssignments(template.TemplateId);
            if (!hasAssignments)
            {
                result.BlockingIssues.Add("Individual mode requires active assignments for user access control");
            }

            // 5. Check workflow based on RequiresApproval flag
            if (template.RequiresApproval)
            {
                if (!template.WorkflowId.HasValue)
                {
                    result.BlockingIssues.Add("Template requires approval workflow but none is assigned");
                }
                else
                {
                    // Validate workflow is compatible with Individual mode
                    var workflowValidation = await _workflowService.ValidateIndividualModeWorkflowAsync(template.WorkflowId.Value);
                    if (!workflowValidation.IsValid)
                    {
                        result.BlockingIssues.AddRange(workflowValidation.Errors.Select(e => $"Workflow issue: {e}"));
                    }
                }
            }
            else
            {
                // RequiresApproval = false, workflow is optional
                if (template.WorkflowId.HasValue)
                {
                    result.Warnings.Add("Workflow assigned but RequiresApproval is false - workflow will be ignored");
                }
                else
                {
                    result.Warnings.Add("No approval workflow required for this template");
                }
            }

            // Set overall readiness
            result.IsReady = !result.BlockingIssues.Any();
            return result;
        }

        private async Task ValidateCollaborativeModeReadiness(FormTemplate template, TemplateReadinessDto result)
        {
            // Collaborative Mode Requirements:
            // 1. Must be published
            if (template.PublishStatus != "Published")
            {
                result.BlockingIssues.Add("Template must be published before collaborative workflow can begin");
            }

            // 2. Must have form structure
            if (!template.Sections.Any() || !template.Sections.SelectMany(s => s.Items).Any())
            {
                result.BlockingIssues.Add("Template must have at least one section with fields");
            }

            // 3. Check if anonymous access is enabled - collaborative + anonymous is invalid
            if (template.AllowAnonymousAccess)
            {
                result.BlockingIssues.Add("Collaborative mode cannot be used with anonymous access - collaborative workflows require authenticated users");
                return; // No point checking further if this fundamental conflict exists
            }

            // 4. Collaborative mode ALWAYS requires workflows (ignore RequiresApproval flag)
            if (!template.WorkflowId.HasValue || template.Workflow == null)
            {
                result.BlockingIssues.Add("Collaborative mode always requires a workflow with Fill steps");
            }
            else
            {
                var fillSteps = template.Workflow.Steps.Where(s => s.Action?.ActionCode == "FILL").ToList();
                if (!fillSteps.Any())
                {
                    result.BlockingIssues.Add("Collaborative mode workflow must have at least one Fill step");
                }

                // 5. Workflow must be valid for Collaborative mode  
                var workflowValidation = await ValidateWorkflowForTemplateMode(template);
                if (!workflowValidation.IsValid)
                {
                    result.BlockingIssues.AddRange(workflowValidation.Errors.Select(e => $"Workflow error: {e}"));
                }

                // Note: RequiresApproval flag is ignored for Collaborative mode
                if (!template.RequiresApproval)
                {
                    result.Warnings.Add("RequiresApproval is false but Collaborative mode always uses workflows");
                }
            }

            // 6. Assignments are optional but warn if missing
            var activeAssignments = template.Assignments.Where(a =>
                a.Status == "Active" &&
                a.EffectiveFrom <= DateTime.UtcNow &&
                (a.EffectiveUntil == null || a.EffectiveUntil >= DateTime.UtcNow)).ToList();

            if (!activeAssignments.Any())
            {
                result.Warnings.Add("No assignments found. Users may not be able to view submissions unless workflow assignees cover viewing permissions");
            }
        }

        private async Task<WorkflowValidationResultDto> ValidateWorkflowForTemplateMode(FormTemplate template)
        {
            if (!template.WorkflowId.HasValue)
            {
                return new WorkflowValidationResultDto { IsValid = false, Errors = new List<string> { "No workflow assigned" } };
            }

            // Use WorkflowService validation methods
            return await _workflowService.ValidateWorkflowForModeAsync(template.WorkflowId.Value, template.TemplateId);
        }

        private async Task<bool> CheckAssignmentCoverageAsync(List<FormTemplateAssignment> assignments)
        {
            // Simple check - if we have at least one assignment that could resolve to users
            return assignments.Any(a =>
                a.AssignmentType == "All" ||
                a.AssignmentType == "Role" ||
                a.AssignmentType == "Department" ||
                a.AssignmentType == "UserGroup" ||
                a.AssignmentType == "SpecificUser");
        }

        private async Task<bool> DetermineOverallReadiness(FormTemplate template, TemplateConfigurationStatusDto status)
        {
            bool basicRequirements = status.HasFormStructure && status.TemplateStatus == "Published";

            return template.SubmissionMode switch
            {
                Models.Common.SubmissionMode.Individual =>
                    await DetermineIndividualModeReadiness(template, basicRequirements, status),

                Models.Common.SubmissionMode.Collaborative =>
                    await DetermineCollaborativeModeReadiness(template, basicRequirements, status),

                _ => false
            };
        }

        private async Task<bool> DetermineIndividualModeReadiness(FormTemplate template, bool basicRequirements, TemplateConfigurationStatusDto status)
        {
            if (!basicRequirements) return false;

            // Anonymous access bypasses assignment and workflow requirements
            if (template.AllowAnonymousAccess)
            {
                return true; // Only needs basic requirements (published + form structure)
            }

            // Non-anonymous Individual mode requirements
            bool hasAssignments = status.HasAssignments;
            bool workflowRequired = template.RequiresApproval;
            bool workflowValid = workflowRequired ? (status.HasWorkflow && status.WorkflowValidForMode) : true;

            return hasAssignments && workflowValid;
        }

        private async Task<bool> DetermineCollaborativeModeReadiness(FormTemplate template, bool basicRequirements, TemplateConfigurationStatusDto status)
        {
            if (!basicRequirements) return false;

            // Collaborative mode cannot work with anonymous access
            if (template.AllowAnonymousAccess)
            {
                return false;
            }

            // Collaborative mode ALWAYS requires workflows (ignores RequiresApproval flag)
            return status.HasWorkflow && status.WorkflowValidForMode;
        }

        private async Task<bool> HasFormStructure(int templateId)
        {
            var hasStructure = await _context.FormTemplateSections
                .Where(s => s.TemplateId == templateId)
                .SelectMany(s => s.Items)
                .Where(i => i.IsActive)
                .AnyAsync();

            return hasStructure;
        }

        private async Task<bool> HasActiveAssignments(int templateId)
        {
            var now = DateTime.UtcNow;
            return await _context.FormTemplateAssignments
                .AnyAsync(a => a.TemplateId == templateId &&
                              a.Status == "Active" &&
                              a.EffectiveFrom <= now &&
                              (a.EffectiveUntil == null || a.EffectiveUntil >= now));
        }

        #endregion
    }
}
