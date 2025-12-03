using FormReporting.Models.ViewModels.Components;

namespace FormReporting.Models.ViewModels.Forms
{
    /// <summary>
    /// Contains resume information for form template creation workflow
    /// Used to determine where user left off and restore progress
    /// </summary>
    public class FormBuilderResumeInfo
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateCode { get; set; } = string.Empty;
        public string PublishStatus { get; set; } = string.Empty;
        
        /// <summary>
        /// The step user should resume from (first incomplete step)
        /// </summary>
        public FormBuilderStep CurrentStep { get; set; }
        
        /// <summary>
        /// Which steps have been completed
        /// </summary>
        public Dictionary<FormBuilderStep, bool> CompletedSteps { get; set; } = new();
        
        /// <summary>
        /// Status of each step (Active, Completed, Pending, Error)
        /// </summary>
        public Dictionary<FormBuilderStep, StepStatus> StepStatuses { get; set; } = new();
        
        /// <summary>
        /// Overall completion percentage (0-100)
        /// </summary>
        public int CompletionPercentage { get; set; }
        
        /// <summary>
        /// When template was last modified
        /// </summary>
        public DateTime LastModifiedDate { get; set; }
        
        /// <summary>
        /// Can this template be edited? (Only drafts can be edited)
        /// </summary>
        public bool CanEdit => PublishStatus == "Draft";
        
        /// <summary>
        /// User-friendly step name for display
        /// Order: Setup → Build → Publish
        /// </summary>
        public string CurrentStepName => CurrentStep switch
        {
            FormBuilderStep.TemplateSetup => "Template Setup",
            FormBuilderStep.FormBuilder => "Form Builder",
            FormBuilderStep.ReviewPublish => "Review & Publish",
            _ => "Unknown"
        };
    }
}
