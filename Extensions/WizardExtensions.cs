using FormReporting.Models.ViewModels.Components;

namespace FormReporting.Extensions
{
    /// <summary>
    /// Extension methods for building wizards from configuration objects
    /// Handles all transformation logic - Views only provide data, Extensions build the structure
    /// Follows three-layer pattern: Config → Extension → ViewModel
    /// </summary>
    public static class WizardExtensions
    {
        /// <summary>
        /// Transforms WizardConfig into WizardViewModel
        /// Validates, auto-numbers, applies defaults, transforms to render-ready ViewModels
        /// ALL logic happens here - partials receive fully prepared data with ZERO logic needed
        /// </summary>
        public static WizardViewModel BuildWizard(this WizardConfig config)
        {
            // 1. Validate
            if (config.Steps == null || !config.Steps.Any())
                throw new ArgumentException("Wizard must have at least one step");

            // 2. Auto-number steps if not set
            for (int i = 0; i < config.Steps.Count; i++)
            {
                if (config.Steps[i].StepNumber == 0)
                    config.Steps[i].StepNumber = i + 1;

                // Generate ID if not provided
                if (string.IsNullOrEmpty(config.Steps[i].StepId))
                    config.Steps[i].StepId = $"step-{i + 1}";
            }

            // 3. Ensure at least one step is active
            if (!config.Steps.Any(s => s.State == WizardStepState.Active))
                config.Steps[0].State = WizardStepState.Active;

            // 4. Apply default button texts
            foreach (var step in config.Steps)
            {
                var stepIndex = config.Steps.IndexOf(step);

                // Previous button
                if (string.IsNullOrEmpty(step.PreviousButtonText))
                    step.PreviousButtonText = "Back";

                // Next button
                if (string.IsNullOrEmpty(step.NextButtonText))
                {
                    step.NextButtonText = stepIndex == config.Steps.Count - 1
                        ? "Complete"
                        : "Next";
                }

                // Hide previous button on first step
                if (stepIndex == 0)
                    step.ShowPrevious = false;
            }

            // 5. Transform Config Steps → ViewModel Steps
            var viewModelSteps = new List<WizardStepViewModel>();

            for (int i = 0; i < config.Steps.Count; i++)
            {
                var configStep = config.Steps[i];
                var prevStep = i > 0 ? config.Steps[i - 1] : null;
                var nextStep = i < config.Steps.Count - 1 ? config.Steps[i + 1] : null;

                // Determine icon based on state (if not provided)
                var icon = !string.IsNullOrEmpty(configStep.Icon)
                    ? configStep.Icon
                    : configStep.State.GetStepIcon();

                // Create ViewModel step with all pre-computed values
                var viewModelStep = new WizardStepViewModel
                {
                    StepId = configStep.StepId,
                    StepNumber = configStep.StepNumber,
                    Title = configStep.Title,
                    Label = configStep.Label,
                    Description = configStep.Description,
                    Instructions = configStep.Instructions,
                    Icon = icon,
                    StateClass = configStep.State.GetStepClass(),
                    ColorClass = configStep.State.GetStepColorClass(),
                    IsActive = configStep.State == WizardStepState.Active,
                    IsDone = configStep.State == WizardStepState.Done,
                    IsPending = configStep.State == WizardStepState.Pending,
                    ContentPartialPath = configStep.ContentPartialPath,
                    ShowPrevious = configStep.ShowPrevious,
                    ShowNext = configStep.ShowNext,
                    PreviousButtonText = configStep.PreviousButtonText ?? "Back",
                    NextButtonText = configStep.NextButtonText ?? "Next",
                    CustomButtonHtml = configStep.CustomButtonHtml,
                    PreviousStepId = prevStep?.StepId,
                    NextStepId = nextStep?.StepId
                };

                viewModelSteps.Add(viewModelStep);
            }

            // 6. Transform Summary Items (if any)
            List<SummaryItemViewModel>? summaryItems = null;
            if (config.SummaryItems != null && config.SummaryItems.Any())
            {
                summaryItems = config.SummaryItems.Select(si => new SummaryItemViewModel
                {
                    Title = si.Title,
                    Description = si.Description,
                    Value = si.Value,
                    CssClass = si.CssClass,
                    Icon = si.Icon
                }).ToList();
            }

            // 7. Return fully prepared ViewModel
            return new WizardViewModel
            {
                Steps = viewModelSteps,
                Layout = config.Layout,
                ShowSummary = config.ShowSummary,
                SummaryTitle = config.SummaryTitle,
                SummaryItems = summaryItems,
                FormId = config.FormId,
                FormAction = config.FormAction,
                FormMethod = config.FormMethod,
                ContainerCssClass = config.ContainerCssClass
            };
        }

        // ========== Fluent API Methods ==========

        /// <summary>
        /// Set wizard to vertical layout
        /// </summary>
        public static WizardConfig WithVerticalLayout(this WizardConfig config)
        {
            config.Layout = WizardLayout.Vertical;
            return config;
        }

        /// <summary>
        /// Set wizard to horizontal layout
        /// </summary>
        public static WizardConfig WithHorizontalLayout(this WizardConfig config)
        {
            config.Layout = WizardLayout.Horizontal;
            return config;
        }

        /// <summary>
        /// Add summary panel
        /// </summary>
        public static WizardConfig WithSummary(this WizardConfig config,
            string title, List<SummaryItem> items)
        {
            config.ShowSummary = true;
            config.SummaryTitle = title;
            config.SummaryItems = items;
            return config;
        }

        /// <summary>
        /// Set form configuration
        /// </summary>
        public static WizardConfig WithForm(this WizardConfig config,
            string formId, string action, string method = "POST")
        {
            config.FormId = formId;
            config.FormAction = action;
            config.FormMethod = method;
            return config;
        }

        // ========== Helper Methods (For Extension Use Only - NOT for Views) ==========

        /// <summary>
        /// Get CSS class for step state
        /// </summary>
        public static string GetStepClass(this WizardStepState state)
        {
            return state switch
            {
                WizardStepState.Done => "done",
                WizardStepState.Active => "active",
                _ => ""
            };
        }

        /// <summary>
        /// Get icon for step state
        /// </summary>
        public static string GetStepIcon(this WizardStepState state)
        {
            return state switch
            {
                WizardStepState.Done => "ri-checkbox-circle-fill",
                WizardStepState.Active => "ri-close-circle-fill",
                _ => "ri-close-circle-fill"
            };
        }

        /// <summary>
        /// Get color class for step state
        /// </summary>
        public static string GetStepColorClass(this WizardStepState state)
        {
            return state switch
            {
                WizardStepState.Done => "text-success",
                WizardStepState.Active => "text-primary",
                _ => "text-muted"
            };
        }
    }
}
