namespace FormReporting.Models.ViewModels.Components
{
    // ============================================================================
    // ENUMS
    // ============================================================================

    /// <summary>
    /// Form Builder steps
    /// </summary>
    public enum FormBuilderStep
    {
        TemplateConfiguration = 1,  // Basic Info + Settings
        SectionBuilder = 2,          // Add sections and fields
        Validation = 3               // Preview, validate, publish
    }

    /// <summary>
    /// Step status
    /// </summary>
    public enum StepStatus
    {
        Pending,      // Not started, grayed out
        Active,       // Current step, highlighted
        Completed,    // Done, green checkmark
        Error         // Has validation errors, red
    }

    // ============================================================================
    // CONFIGURATION CLASSES (What controllers create)
    // ============================================================================

    /// <summary>
    /// Configuration for Form Builder Progress Tracker
    /// </summary>
    public class FormBuilderProgressConfig
    {
        /// <summary>
        /// Unique ID for this form builder session
        /// </summary>
        public string BuilderId { get; set; } = string.Empty;

        /// <summary>
        /// Current step
        /// </summary>
        public FormBuilderStep CurrentStep { get; set; } = FormBuilderStep.TemplateConfiguration;

        /// <summary>
        /// Template ID (if editing existing template)
        /// </summary>
        public int? TemplateId { get; set; }

        /// <summary>
        /// Template name (shown in header)
        /// </summary>
        public string? TemplateName { get; set; }

        /// <summary>
        /// Template version (shown in header)
        /// </summary>
        public string? TemplateVersion { get; set; }

        /// <summary>
        /// Publish status
        /// </summary>
        public string? PublishStatus { get; set; }

        /// <summary>
        /// Step statuses (tracks which steps are completed)
        /// </summary>
        public Dictionary<FormBuilderStep, StepStatus> StepStatuses { get; set; } = new()
        {
            { FormBuilderStep.TemplateConfiguration, StepStatus.Active },
            { FormBuilderStep.SectionBuilder, StepStatus.Pending },
            { FormBuilderStep.Validation, StepStatus.Pending }
        };

        /// <summary>
        /// Allow navigation to previous steps
        /// </summary>
        public bool AllowBackNavigation { get; set; } = true;

        /// <summary>
        /// Show save draft button
        /// </summary>
        public bool ShowSaveDraft { get; set; } = true;

        /// <summary>
        /// Show exit button
        /// </summary>
        public bool ShowExit { get; set; } = true;

        /// <summary>
        /// Exit URL
        /// </summary>
        public string ExitUrl { get; set; } = "/Forms/FormTemplates";
    }

    /// <summary>
    /// Individual step configuration
    /// </summary>
    public class FormBuilderStepConfig
    {
        public FormBuilderStep Step { get; set; }
        public string StepNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public StepStatus Status { get; set; } = StepStatus.Pending;
        public string? NavigateUrl { get; set; }
        public bool IsNavigable { get; set; } = false;
    }

    // ============================================================================
    // VIEW MODEL CLASSES (What partials receive)
    // ============================================================================

    /// <summary>
    /// Form Builder Progress Tracker ViewModel - ready for rendering
    /// </summary>
    public class FormBuilderProgressViewModel
    {
        public string BuilderId { get; set; } = string.Empty;
        public FormBuilderStep CurrentStep { get; set; }
        public int? TemplateId { get; set; }
        public string? TemplateName { get; set; }
        public string? TemplateVersion { get; set; }
        public string? PublishStatus { get; set; }
        public List<FormBuilderStepViewModel> Steps { get; set; } = new();
        public bool AllowBackNavigation { get; set; }
        public bool ShowSaveDraft { get; set; }
        public bool ShowExit { get; set; }
        public string ExitUrl { get; set; } = string.Empty;

        // Pre-computed properties
        public int CurrentStepNumber => (int)CurrentStep;
        public int TotalSteps => Steps.Count;
        public int ProgressPercentage => TotalSteps > 0 ? (CurrentStepNumber * 100) / TotalSteps : 0;
        public string ProgressBarClass => ProgressPercentage switch
        {
            < 50 => "bg-warning",
            < 100 => "bg-info",
            _ => "bg-success"
        };
    }

    /// <summary>
    /// Individual step view model
    /// </summary>
    public class FormBuilderStepViewModel
    {
        public FormBuilderStep Step { get; set; }
        public string StepNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public StepStatus Status { get; set; }
        public string? NavigateUrl { get; set; }
        public bool IsNavigable { get; set; }
        public bool IsActive { get; set; }
        public bool IsCompleted { get; set; }
        public bool HasError { get; set; }
        public bool IsPending { get; set; }

        // Pre-computed CSS classes
        public string StepClasses { get; set; } = string.Empty;
        public string IconClasses { get; set; } = string.Empty;
        public string TitleClasses { get; set; } = string.Empty;
        public string StatusIconClass { get; set; } = string.Empty;
        public string StatusIconColor { get; set; } = string.Empty;
    }
}
