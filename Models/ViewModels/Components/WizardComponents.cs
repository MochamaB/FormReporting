namespace FormReporting.Models.ViewModels.Components
{
    // ============================================================================
    // ENUMS
    // ============================================================================

    /// <summary>
    /// Wizard layout types
    /// </summary>
    public enum WizardLayout
    {
        /// <summary>
        /// Vertical layout: Steps on left, content center, summary right (3-column)
        /// Best for: Simple workflows (User Setup, Role Creation)
        /// </summary>
        Vertical = 1,

        /// <summary>
        /// Horizontal layout: Steps on top, content below (2-row)
        /// Best for: Complex workflows (Form Submissions, Template Builder)
        /// </summary>
        Horizontal = 2
    }

    /// <summary>
    /// Step state for visual indicators
    /// </summary>
    public enum WizardStepState
    {
        /// <summary>
        /// Step not yet started (default icon)
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Currently active step (highlighted)
        /// </summary>
        Active = 2,

        /// <summary>
        /// Completed step (checkmark icon)
        /// </summary>
        Done = 3
    }

    // ============================================================================
    // CONFIGURATION CLASSES (What users create in views)
    // ============================================================================

    /// <summary>
    /// Configuration object for creating wizards
    /// User creates this in views to define WHAT to display
    /// Extensions handle HOW to transform this into WizardViewModel
    /// </summary>
    public class WizardConfig
    {
        /// <summary>
        /// List of wizard steps (required)
        /// </summary>
        public List<WizardStep> Steps { get; set; } = new();

        /// <summary>
        /// Layout type: Vertical or Horizontal
        /// </summary>
        public WizardLayout Layout { get; set; } = WizardLayout.Vertical;

        /// <summary>
        /// Show summary panel on right (Vertical) or bottom (Horizontal)
        /// </summary>
        public bool ShowSummary { get; set; } = false;

        /// <summary>
        /// Summary panel title (e.g., "Order Summary", "Form Preview")
        /// </summary>
        public string? SummaryTitle { get; set; }

        /// <summary>
        /// Items to display in summary panel
        /// </summary>
        public List<SummaryItem>? SummaryItems { get; set; }

        /// <summary>
        /// Form ID attribute
        /// </summary>
        public string FormId { get; set; } = "wizard-form";

        /// <summary>
        /// Form action URL
        /// </summary>
        public string FormAction { get; set; } = "#";

        /// <summary>
        /// Form method (POST, GET)
        /// </summary>
        public string FormMethod { get; set; } = "POST";

        /// <summary>
        /// Additional CSS classes for the wizard container
        /// </summary>
        public string? ContainerCssClass { get; set; }
    }

    /// <summary>
    /// Individual wizard step configuration (Config layer)
    /// </summary>
    public class WizardStep
    {
        /// <summary>
        /// Unique step identifier (auto-generated if not provided)
        /// </summary>
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// Step number (1, 2, 3...) - auto-assigned if not provided
        /// </summary>
        public int StepNumber { get; set; }

        /// <summary>
        /// Step title (e.g., "Step 1", "Step 2")
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Step label/name (e.g., "Billing Info", "Address", "Payment")
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Optional description (shown in sidebar/stepper)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Instructions/helper text shown in the content area header
        /// (e.g., "Define the organizational structure for this tenant")
        /// </summary>
        public string? Instructions { get; set; }

        /// <summary>
        /// Remix icon class (optional - will be auto-assigned based on state if not provided)
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Current state of the step
        /// </summary>
        public WizardStepState State { get; set; } = WizardStepState.Pending;

        /// <summary>
        /// Path to partial view containing step content
        /// </summary>
        public string? ContentPartialPath { get; set; }

        /// <summary>
        /// Show "Previous" button for this step
        /// </summary>
        public bool ShowPrevious { get; set; } = true;

        /// <summary>
        /// Show "Next" button for this step
        /// </summary>
        public bool ShowNext { get; set; } = true;

        /// <summary>
        /// Custom text for Previous button (default: "Back")
        /// </summary>
        public string? PreviousButtonText { get; set; }

        /// <summary>
        /// Custom text for Next button (default: "Next" or "Complete" for last step)
        /// </summary>
        public string? NextButtonText { get; set; }

        /// <summary>
        /// Custom HTML for additional buttons (e.g., Submit button on final step)
        /// </summary>
        public string? CustomButtonHtml { get; set; }

        /// <summary>
        /// Enable validation for this step before proceeding
        /// </summary>
        public bool RequireValidation { get; set; } = false;
    }

    /// <summary>
    /// Summary panel item configuration (Config layer)
    /// </summary>
    public class SummaryItem
    {
        /// <summary>
        /// Item title/name
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Optional description/subtitle
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Item value (e.g., "$25", "3 items", "John Doe")
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Optional CSS class for styling (e.g., "text-success", "text-danger")
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Optional icon
        /// </summary>
        public string? Icon { get; set; }
    }

    // ============================================================================
    // VIEW MODEL CLASSES (What partials receive for rendering)
    // ============================================================================

    /// <summary>
    /// Wizard view model - ready for rendering
    /// Created by WizardExtensions.BuildWizard()
    /// Contains all render-ready data, no logic needed in views
    /// </summary>
    public class WizardViewModel
    {
        /// <summary>
        /// List of transformed wizard steps (ViewModel version)
        /// </summary>
        public List<WizardStepViewModel> Steps { get; set; } = new();

        /// <summary>
        /// Layout type: Vertical or Horizontal
        /// </summary>
        public WizardLayout Layout { get; set; }

        /// <summary>
        /// Show summary panel
        /// </summary>
        public bool ShowSummary { get; set; }

        /// <summary>
        /// Summary panel title
        /// </summary>
        public string? SummaryTitle { get; set; }

        /// <summary>
        /// Transformed summary items (ViewModel version)
        /// </summary>
        public List<SummaryItemViewModel>? SummaryItems { get; set; }

        /// <summary>
        /// Form ID attribute
        /// </summary>
        public string FormId { get; set; } = string.Empty;

        /// <summary>
        /// Form action URL
        /// </summary>
        public string FormAction { get; set; } = string.Empty;

        /// <summary>
        /// Form method
        /// </summary>
        public string FormMethod { get; set; } = string.Empty;

        /// <summary>
        /// Container CSS classes
        /// </summary>
        public string? ContainerCssClass { get; set; }

        // Computed properties for convenience
        /// <summary>
        /// Total number of steps
        /// </summary>
        public int TotalSteps => Steps.Count;

        /// <summary>
        /// Current active step index (0-based)
        /// </summary>
        public int CurrentStepIndex => Steps.FindIndex(s => s.IsActive);

        /// <summary>
        /// Current active step number (1-based)
        /// </summary>
        public int CurrentStepNumber => CurrentStepIndex + 1;

        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public int ProgressPercentage => TotalSteps > 0
            ? (int)((double)Steps.Count(s => s.IsDone) / TotalSteps * 100)
            : 0;
    }

    /// <summary>
    /// Individual wizard step view model (ViewModel layer)
    /// All properties pre-computed for rendering - NO logic in partial views
    /// </summary>
    public class WizardStepViewModel
    {
        /// <summary>
        /// Unique step identifier
        /// </summary>
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// Step number (1, 2, 3...)
        /// </summary>
        public int StepNumber { get; set; }

        /// <summary>
        /// Step title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Step label/name
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Optional description (shown in sidebar/stepper)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Instructions/helper text shown in the content area header
        /// </summary>
        public string? Instructions { get; set; }

        /// <summary>
        /// Icon class (pre-computed based on state)
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// CSS class for step state (e.g., "done", "active", "")
        /// </summary>
        public string StateClass { get; set; } = string.Empty;

        /// <summary>
        /// Color class for step (e.g., "text-success", "text-primary", "text-muted")
        /// </summary>
        public string ColorClass { get; set; } = string.Empty;

        /// <summary>
        /// Is this step currently active?
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Is this step completed?
        /// </summary>
        public bool IsDone { get; set; }

        /// <summary>
        /// Is this step pending?
        /// </summary>
        public bool IsPending { get; set; }

        /// <summary>
        /// Path to partial view containing step content
        /// </summary>
        public string? ContentPartialPath { get; set; }

        /// <summary>
        /// Show previous button
        /// </summary>
        public bool ShowPrevious { get; set; }

        /// <summary>
        /// Show next button
        /// </summary>
        public bool ShowNext { get; set; }

        /// <summary>
        /// Previous button text
        /// </summary>
        public string PreviousButtonText { get; set; } = "Back";

        /// <summary>
        /// Next button text
        /// </summary>
        public string NextButtonText { get; set; } = "Next";

        /// <summary>
        /// Custom button HTML
        /// </summary>
        public string? CustomButtonHtml { get; set; }

        /// <summary>
        /// Previous step ID (for navigation)
        /// </summary>
        public string? PreviousStepId { get; set; }

        /// <summary>
        /// Next step ID (for navigation)
        /// </summary>
        public string? NextStepId { get; set; }

        /// <summary>
        /// ARIA selected attribute value ("true" or "false")
        /// </summary>
        public string AriaSelected => IsActive ? "true" : "false";

        /// <summary>
        /// Bootstrap tab class for active state
        /// </summary>
        public string TabActiveClass => IsActive ? "show active" : "";
    }

    /// <summary>
    /// Summary item view model (ViewModel layer)
    /// </summary>
    public class SummaryItemViewModel
    {
        /// <summary>
        /// Item title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Optional description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Item value
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// CSS class for styling
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// Icon class
        /// </summary>
        public string? Icon { get; set; }
    }
}
