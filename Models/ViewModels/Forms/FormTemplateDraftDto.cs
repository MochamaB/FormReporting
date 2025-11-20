namespace FormReporting.Models.ViewModels.Forms
{
    /// <summary>
    /// DTO for saving form template drafts (auto-save and manual save)
    /// </summary>
    public class FormTemplateDraftDto
    {
        public int? TemplateId { get; set; }
        public string? TemplateName { get; set; }
        public string? TemplateCode { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string? TemplateType { get; set; }
    }
}
