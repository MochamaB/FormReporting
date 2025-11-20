using FormReporting.Models.ViewModels.Components;

namespace FormReporting.Extensions
{
    /// <summary>
    /// Extension methods for Form Builder Progress Tracker
    /// Handles all transformation logic - Views only provide data
    /// </summary>
    public static class FormBuilderProgressExtensions
    {
        /// <summary>
        /// Main transformation: Config → ViewModel
        /// </summary>
        public static FormBuilderProgressViewModel BuildProgress(this FormBuilderProgressConfig config)
        {
            // Define step configurations
            var stepConfigs = GetStepConfigurations(config);

            // Transform each step
            var stepViewModels = stepConfigs
                .OrderBy(s => (int)s.Step)
                .Select(s => TransformStep(s, config))
                .ToList();

            // Build final ViewModel
            return new FormBuilderProgressViewModel
            {
                BuilderId = config.BuilderId,
                CurrentStep = config.CurrentStep,
                TemplateId = config.TemplateId,
                TemplateName = config.TemplateName,
                TemplateVersion = config.TemplateVersion,
                PublishStatus = config.PublishStatus,
                Steps = stepViewModels,
                AllowBackNavigation = config.AllowBackNavigation,
                ShowSaveDraft = config.ShowSaveDraft,
                ShowExit = config.ShowExit,
                ExitUrl = config.ExitUrl
            };
        }

        /// <summary>
        /// Fluent API: Mark step as completed
        /// </summary>
        public static FormBuilderProgressConfig WithStepCompleted(
            this FormBuilderProgressConfig config,
            FormBuilderStep step)
        {
            config.StepStatuses[step] = StepStatus.Completed;
            return config;
        }

        /// <summary>
        /// Fluent API: Mark step as error
        /// </summary>
        public static FormBuilderProgressConfig WithStepError(
            this FormBuilderProgressConfig config,
            FormBuilderStep step)
        {
            config.StepStatuses[step] = StepStatus.Error;
            return config;
        }

        /// <summary>
        /// Fluent API: Set current step
        /// </summary>
        public static FormBuilderProgressConfig AtStep(
            this FormBuilderProgressConfig config,
            FormBuilderStep step)
        {
            config.CurrentStep = step;

            // Update status: previous steps should be completed, current is active
            foreach (var s in config.StepStatuses.Keys.ToList())
            {
                if ((int)s < (int)step && config.StepStatuses[s] != StepStatus.Error)
                {
                    config.StepStatuses[s] = StepStatus.Completed;
                }
                else if (s == step)
                {
                    config.StepStatuses[s] = StepStatus.Active;
                }
                else if ((int)s > (int)step && config.StepStatuses[s] == StepStatus.Active)
                {
                    config.StepStatuses[s] = StepStatus.Pending;
                }
            }

            return config;
        }

        // ========== Helper Methods ==========

        /// <summary>
        /// Get step configurations with metadata - Complete 7-step wizard
        /// </summary>
        private static List<FormBuilderStepConfig> GetStepConfigurations(FormBuilderProgressConfig config)
        {
            var baseUrl = config.TemplateId.HasValue
                ? $"/Forms/FormTemplates/Edit/{config.TemplateId}"
                : "/Forms/FormTemplates/Create";

            return new List<FormBuilderStepConfig>
            {
                new FormBuilderStepConfig
                {
                    Step = FormBuilderStep.TemplateSetup,
                    StepNumber = "1",
                    Title = "Template Setup",
                    Description = "Basic information and settings",
                    Icon = "ri-file-settings-line",
                    Status = config.StepStatuses[FormBuilderStep.TemplateSetup],
                    NavigateUrl = baseUrl,
                    IsNavigable = true // Always navigable
                },
                new FormBuilderStepConfig
                {
                    Step = FormBuilderStep.FormBuilder,
                    StepNumber = "2",
                    Title = "Build Form",
                    Description = "Sections, fields & validation",
                    Icon = "ri-layout-grid-line",
                    Status = config.StepStatuses[FormBuilderStep.FormBuilder],
                    NavigateUrl = config.TemplateId.HasValue ? $"/Forms/FormTemplates/FormBuilder/{config.TemplateId}" : null,
                    IsNavigable = config.TemplateId.HasValue // Can only navigate if template saved
                },
                new FormBuilderStepConfig
                {
                    Step = FormBuilderStep.MetricMapping,
                    StepNumber = "3",
                    Title = "Metric Mapping",
                    Description = "Map fields to KPI metrics",
                    Icon = "ri-line-chart-line",
                    Status = config.StepStatuses[FormBuilderStep.MetricMapping],
                    NavigateUrl = config.TemplateId.HasValue ? $"/Forms/FormTemplates/MetricMapping/{config.TemplateId}" : null,
                    IsNavigable = config.TemplateId.HasValue
                },
                new FormBuilderStepConfig
                {
                    Step = FormBuilderStep.ApprovalWorkflow,
                    StepNumber = "4",
                    Title = "Approval Workflow",
                    Description = "Define approval levels",
                    Icon = "ri-shield-check-line",
                    Status = config.StepStatuses[FormBuilderStep.ApprovalWorkflow],
                    NavigateUrl = config.TemplateId.HasValue ? $"/Forms/FormTemplates/ApprovalWorkflow/{config.TemplateId}" : null,
                    IsNavigable = config.TemplateId.HasValue
                },
                new FormBuilderStepConfig
                {
                    Step = FormBuilderStep.FormAssignments,
                    StepNumber = "5",
                    Title = "Form Assignments",
                    Description = "Assign to users & tenants",
                    Icon = "ri-user-add-line",
                    Status = config.StepStatuses[FormBuilderStep.FormAssignments],
                    NavigateUrl = config.TemplateId.HasValue ? $"/Forms/FormTemplates/Assignments/{config.TemplateId}" : null,
                    IsNavigable = config.TemplateId.HasValue
                },
                new FormBuilderStepConfig
                {
                    Step = FormBuilderStep.ReportConfiguration,
                    StepNumber = "6",
                    Title = "Report Configuration",
                    Description = "Configure dashboards & reports",
                    Icon = "ri-dashboard-line",
                    Status = config.StepStatuses[FormBuilderStep.ReportConfiguration],
                    NavigateUrl = config.TemplateId.HasValue ? $"/Forms/FormTemplates/Reports/{config.TemplateId}" : null,
                    IsNavigable = config.TemplateId.HasValue
                },
                new FormBuilderStepConfig
                {
                    Step = FormBuilderStep.ReviewPublish,
                    StepNumber = "7",
                    Title = "Review & Publish",
                    Description = "Validate and publish template",
                    Icon = "ri-checkbox-circle-line",
                    Status = config.StepStatuses[FormBuilderStep.ReviewPublish],
                    NavigateUrl = config.TemplateId.HasValue ? $"/Forms/FormTemplates/ReviewPublish/{config.TemplateId}" : null,
                    IsNavigable = config.TemplateId.HasValue
                }
            };
        }

        /// <summary>
        /// Transform step config → view model
        /// </summary>
        private static FormBuilderStepViewModel TransformStep(
            FormBuilderStepConfig stepConfig,
            FormBuilderProgressConfig config)
        {
            var isActive = stepConfig.Step == config.CurrentStep;
            var isCompleted = stepConfig.Status == StepStatus.Completed;
            var hasError = stepConfig.Status == StepStatus.Error;
            var isPending = stepConfig.Status == StepStatus.Pending;

            return new FormBuilderStepViewModel
            {
                Step = stepConfig.Step,
                StepNumber = stepConfig.StepNumber,
                Title = stepConfig.Title,
                Description = stepConfig.Description,
                Icon = stepConfig.Icon,
                Status = stepConfig.Status,
                NavigateUrl = stepConfig.NavigateUrl,
                IsNavigable = stepConfig.IsNavigable && (isCompleted || isActive || (config.AllowBackNavigation && (int)stepConfig.Step < (int)config.CurrentStep)),
                IsActive = isActive,
                IsCompleted = isCompleted,
                HasError = hasError,
                IsPending = isPending,

                // Pre-computed CSS classes
                StepClasses = BuildStepClasses(isActive, isCompleted, hasError, isPending, stepConfig.IsNavigable),
                IconClasses = BuildIconClasses(stepConfig.Icon, isActive, isCompleted, hasError),
                TitleClasses = BuildTitleClasses(isActive, isCompleted, isPending),
                StatusIconClass = GetStatusIcon(stepConfig.Status),
                StatusIconColor = GetStatusColor(stepConfig.Status)
            };
        }

        /// <summary>
        /// Build step container CSS classes
        /// </summary>
        private static string BuildStepClasses(
            bool isActive,
            bool isCompleted,
            bool hasError,
            bool isPending,
            bool isNavigable)
        {
            var classes = new List<string> { "progress-step" };

            if (isActive)
                classes.Add("active");
            if (isCompleted)
                classes.Add("completed");
            if (hasError)
                classes.Add("error");
            if (isPending)
                classes.Add("pending");
            if (isNavigable)
                classes.Add("navigable");

            return string.Join(" ", classes);
        }

        /// <summary>
        /// Build icon CSS classes
        /// </summary>
        private static string BuildIconClasses(string icon, bool isActive, bool isCompleted, bool hasError)
        {
            var classes = new List<string> { icon, "fs-20" };

            if (isActive)
                classes.Add("text-primary");
            else if (isCompleted)
                classes.Add("text-success");
            else if (hasError)
                classes.Add("text-danger");
            else
                classes.Add("text-muted");

            return string.Join(" ", classes);
        }

        /// <summary>
        /// Build title CSS classes
        /// </summary>
        private static string BuildTitleClasses(bool isActive, bool isCompleted, bool isPending)
        {
            var classes = new List<string> { "step-title" };

            if (isActive)
                classes.Add("fw-semibold");
            if (isPending)
                classes.Add("text-muted");

            return string.Join(" ", classes);
        }

        /// <summary>
        /// Get status icon
        /// </summary>
        private static string GetStatusIcon(StepStatus status)
        {
            return status switch
            {
                StepStatus.Completed => "ri-checkbox-circle-fill",
                StepStatus.Error => "ri-error-warning-fill",
                StepStatus.Active => "ri-arrow-right-circle-fill",
                _ => "ri-checkbox-blank-circle-line"
            };
        }

        /// <summary>
        /// Get status color
        /// </summary>
        private static string GetStatusColor(StepStatus status)
        {
            return status switch
            {
                StepStatus.Completed => "text-success",
                StepStatus.Error => "text-danger",
                StepStatus.Active => "text-primary",
                _ => "text-muted"
            };
        }
    }
}
