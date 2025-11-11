using FormReporting.Models.Common;

namespace FormReporting.Models.ViewModels.Components
{
    /// <summary>
    /// Wizard view model - ready for rendering
    /// Created by WizardExtensions.BuildWizard()
    /// </summary>
    public class WizardViewModel
    {
        public List<WizardStep> Steps { get; set; } = new();
        public WizardLayout Layout { get; set; }
        public bool ShowSummary { get; set; }
        public string? SummaryTitle { get; set; }
        public List<SummaryItem>? SummaryItems { get; set; }
        public string FormId { get; set; } = string.Empty;
        public string FormAction { get; set; } = string.Empty;
        public string FormMethod { get; set; } = string.Empty;
        public string? ContainerCssClass { get; set; }
        
        /// <summary>
        /// Total number of steps
        /// </summary>
        public int TotalSteps => Steps.Count;
        
        /// <summary>
        /// Current active step index (0-based)
        /// </summary>
        public int CurrentStepIndex => Steps.FindIndex(s => s.State == WizardStepState.Active);
        
        /// <summary>
        /// Current active step number (1-based)
        /// </summary>
        public int CurrentStepNumber => CurrentStepIndex + 1;
        
        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public int ProgressPercentage => TotalSteps > 0 
            ? (int)((double)Steps.Count(s => s.State == WizardStepState.Done) / TotalSteps * 100) 
            : 0;
    }
}
