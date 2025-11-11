using FormReporting.Models.Common;

namespace FormReporting.Models.ViewModels.Components
{
    /// <summary>
    /// Configuration object for creating wizards
    /// Used to define WHAT data to display (steps, layout, summary)
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
    /// Individual wizard step configuration
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
        /// Optional description (shown in horizontal layout)
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Remix icon class (default: ri-close-circle-fill)
        /// </summary>
        public string Icon { get; set; } = "ri-close-circle-fill";
        
        /// <summary>
        /// Current state of the step
        /// </summary>
        public WizardStepState State { get; set; } = WizardStepState.Pending;
        
        /// <summary>
        /// Path to partial view containing step content
        /// </summary>
        public string ContentPartialPath { get; set; } = string.Empty;
        
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
    /// Summary panel item
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
}
