// Data/Seeders/FormItemOptionTemplateSeeder.cs
using FormReporting.Models.Entities.Forms;

namespace FormReporting.Data.Seeders
{
    /// <summary>
    /// Seeds pre-defined option templates for form builder
    /// </summary>
    public static class FormItemOptionTemplateSeeder
    {
        public static void SeedOptionTemplates(ApplicationDbContext context)
        {
            // Check if templates already exist
            if (context.FormItemOptionTemplates.Any())
                return;

            var templates = new List<FormItemOptionTemplate>
            {
                // ===== CATEGORY: SATISFACTION =====
                new FormItemOptionTemplate
                {
                    TemplateName = "Satisfaction Scale (5-point)",
                    TemplateCode = "SATISFACTION_5PT",
                    Category = "Rating",
                    SubCategory = "Sentiment",
                    Description = "Standard 5-point customer satisfaction scale",
                    ApplicableFieldTypes = "[\"Radio\",\"Dropdown\",\"Rating\"]",
                    RecommendedFor = "Customer feedback, service quality assessment, user experience surveys",
                    HasScoring = true,
                    ScoringType = "Linear",
                    IsSystemTemplate = true,
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedDate = DateTime.UtcNow,
                    Items = new List<FormItemOptionTemplateItem>
                    {
                        new() { OptionValue = "very_satisfied", OptionLabel = "Very Satisfied", DisplayOrder = 1, ScoreValue = 5, ScoreWeight = 1.0m, ColorHint = "#28a745" },
                        new() { OptionValue = "satisfied", OptionLabel = "Satisfied", DisplayOrder = 2, ScoreValue = 4, ScoreWeight = 1.0m, ColorHint = "#6fbf73" },
                        new() { OptionValue = "neutral", OptionLabel = "Neutral", DisplayOrder = 3, ScoreValue = 3, ScoreWeight = 1.0m, ColorHint = "#ffc107" },
                        new() { OptionValue = "dissatisfied", OptionLabel = "Dissatisfied", DisplayOrder = 4, ScoreValue = 2, ScoreWeight = 1.0m, ColorHint = "#fd7e14" },
                        new() { OptionValue = "very_dissatisfied", OptionLabel = "Very Dissatisfied", DisplayOrder = 5, ScoreValue = 1, ScoreWeight = 1.0m, ColorHint = "#dc3545" }
                    }
                },

                new FormItemOptionTemplate
                {
                    TemplateName = "Satisfaction Scale (3-point)",
                    TemplateCode = "SATISFACTION_3PT",
                    Category = "Rating",
                    SubCategory = "Sentiment",
                    Description = "Simple 3-point satisfaction scale",
                    ApplicableFieldTypes = "[\"Radio\",\"Dropdown\"]",
                    RecommendedFor = "Quick surveys, simple feedback forms",
                    HasScoring = true,
                    ScoringType = "Linear",
                    IsSystemTemplate = true,
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedDate = DateTime.UtcNow,
                    Items = new List<FormItemOptionTemplateItem>
                    {
                        new() { OptionValue = "satisfied", OptionLabel = "Satisfied", DisplayOrder = 1, ScoreValue = 3, ColorHint = "#28a745" },
                        new() { OptionValue = "neutral", OptionLabel = "Neutral", DisplayOrder = 2, ScoreValue = 2, ColorHint = "#ffc107" },
                        new() { OptionValue = "dissatisfied", OptionLabel = "Dissatisfied", DisplayOrder = 3, ScoreValue = 1, ColorHint = "#dc3545" }
                    }
                },

                // ===== CATEGORY: AGREEMENT =====
                new FormItemOptionTemplate
                {
                    TemplateName = "Agree - Disagree (5-point)",
                    TemplateCode = "AGREE_DISAGREE_5PT",
                    Category = "Agreement",
                    SubCategory = "Likert",
                    Description = "Standard 5-point Likert agreement scale",
                    ApplicableFieldTypes = "[\"Radio\",\"Dropdown\"]",
                    RecommendedFor = "Opinion surveys, policy feedback, statement evaluation",
                    HasScoring = true,
                    ScoringType = "Linear",
                    IsSystemTemplate = true,
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedDate = DateTime.UtcNow,
                    Items = new List<FormItemOptionTemplateItem>
                    {
                        new() { OptionValue = "strongly_agree", OptionLabel = "Strongly Agree", DisplayOrder = 1, ScoreValue = 5, ColorHint = "#28a745" },
                        new() { OptionValue = "agree", OptionLabel = "Agree", DisplayOrder = 2, ScoreValue = 4, ColorHint = "#6fbf73" },
                        new() { OptionValue = "neutral", OptionLabel = "Neutral", DisplayOrder = 3, ScoreValue = 3, ColorHint = "#ffc107" },
                        new() { OptionValue = "disagree", OptionLabel = "Disagree", DisplayOrder = 4, ScoreValue = 2, ColorHint = "#fd7e14" },
                        new() { OptionValue = "strongly_disagree", OptionLabel = "Strongly Disagree", DisplayOrder = 5, ScoreValue = 1, ColorHint = "#dc3545" }
                    }
                },

                new FormItemOptionTemplate
                {
                    TemplateName = "Agree - Disagree (7-point)",
                    TemplateCode = "AGREE_DISAGREE_7PT",
                    Category = "Agreement",
                    SubCategory = "Likert",
                    Description = "Extended 7-point Likert agreement scale for nuanced responses",
                    ApplicableFieldTypes = "[\"Radio\",\"Dropdown\"]",
                    RecommendedFor = "Detailed research surveys, academic studies",
                    HasScoring = true,
                    ScoringType = "Linear",
                    IsSystemTemplate = true,
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedDate = DateTime.UtcNow,
                    Items = new List<FormItemOptionTemplateItem>
                    {
                        new() { OptionValue = "strongly_agree", OptionLabel = "Strongly Agree", DisplayOrder = 1, ScoreValue = 7 },
                        new() { OptionValue = "agree", OptionLabel = "Agree", DisplayOrder = 2, ScoreValue = 6 },
                        new() { OptionValue = "somewhat_agree", OptionLabel = "Somewhat Agree", DisplayOrder = 3, ScoreValue = 5 },
                        new() { OptionValue = "neutral", OptionLabel = "Neutral", DisplayOrder = 4, ScoreValue = 4 },
                        new() { OptionValue = "somewhat_disagree", OptionLabel = "Somewhat Disagree", DisplayOrder = 5, ScoreValue = 3 },
                        new() { OptionValue = "disagree", OptionLabel = "Disagree", DisplayOrder = 6, ScoreValue = 2 },
                        new() { OptionValue = "strongly_disagree", OptionLabel = "Strongly Disagree", DisplayOrder = 7, ScoreValue = 1 }
                    }
                },

                // ===== CATEGORY: BINARY =====
                new FormItemOptionTemplate
                {
                    TemplateName = "Yes - No",
                    TemplateCode = "YES_NO",
                    Category = "Binary",
                    SubCategory = "Boolean",
                    Description = "Simple yes/no binary choice",
                    ApplicableFieldTypes = "[\"Radio\",\"Dropdown\"]",
                    RecommendedFor = "Compliance checks, simple confirmations, binary questions",
                    HasScoring = true,
                    ScoringType = "Binary",
                    IsSystemTemplate = true,
                    IsActive = true,
                    DisplayOrder = 5,
                    CreatedDate = DateTime.UtcNow,
                    Items = new List<FormItemOptionTemplateItem>
                    {
                        new() { OptionValue = "yes", OptionLabel = "Yes", DisplayOrder = 1, ScoreValue = 1, ColorHint = "#28a745" },
                        new() { OptionValue = "no", OptionLabel = "No", DisplayOrder = 2, ScoreValue = 0, ColorHint = "#dc3545" }
                    }
                },

                new FormItemOptionTemplate
                {
                    TemplateName = "True - False",
                    TemplateCode = "TRUE_FALSE",
                    Category = "Binary",
                    SubCategory = "Boolean",
                    Description = "Boolean true/false options",
                    ApplicableFieldTypes = "[\"Radio\",\"Dropdown\"]",
                    RecommendedFor = "Quizzes, knowledge checks, factual verification",
                    HasScoring = true,
                    ScoringType = "Binary",
                    IsSystemTemplate = true,
                    IsActive = true,
                    DisplayOrder = 6,
                    CreatedDate = DateTime.UtcNow,
                    Items = new List<FormItemOptionTemplateItem>
                    {
                        new() { OptionValue = "true", OptionLabel = "True", DisplayOrder = 1, ScoreValue = 1 },
                        new() { OptionValue = "false", OptionLabel = "False", DisplayOrder = 2, ScoreValue = 0 }
                    }
                },

                // ===== CATEGORY: FREQUENCY =====
                new FormItemOptionTemplate
                {
                    TemplateName = "Frequency Scale",
                    TemplateCode = "FREQUENCY_5PT",
                    Category = "Frequency",
                    SubCategory = "Occurrence",
                    Description = "How often something occurs (5-point scale)",
                    ApplicableFieldTypes = "[\"Radio\",\"Dropdown\"]",
                    RecommendedFor = "Behavior tracking, usage patterns, occurrence measurement",
                    HasScoring = true,
                    ScoringType = "Linear",
                    IsSystemTemplate = true,
                    IsActive = true,
                    DisplayOrder = 7,
                    CreatedDate = DateTime.UtcNow,
                    Items = new List<FormItemOptionTemplateItem>
                    {
                        new() { OptionValue = "always", OptionLabel = "Always", DisplayOrder = 1, ScoreValue = 5 },
                        new() { OptionValue = "often", OptionLabel = "Often", DisplayOrder = 2, ScoreValue = 4 },
                        new() { OptionValue = "sometimes", OptionLabel = "Sometimes", DisplayOrder = 3, ScoreValue = 3 },
                        new() { OptionValue = "rarely", OptionLabel = "Rarely", DisplayOrder = 4, ScoreValue = 2 },
                        new() { OptionValue = "never", OptionLabel = "Never", DisplayOrder = 5, ScoreValue = 1 }
                    }
                },

                // ===== CATEGORY: QUALITY =====
                new FormItemOptionTemplate
                {
                    TemplateName = "Quality Scale",
                    TemplateCode = "QUALITY_5PT",
                    Category = "Rating",
                    SubCategory = "Quality",
                    Description = "Assess quality from excellent to poor",
                    ApplicableFieldTypes = "[\"Radio\",\"Dropdown\"]",
                    RecommendedFor = "Product quality, service assessment, performance evaluation",
                    HasScoring = true,
                    ScoringType = "Linear",
                    IsSystemTemplate = true,
                    IsActive = true,
                    DisplayOrder = 8,
                    CreatedDate = DateTime.UtcNow,
                    Items = new List<FormItemOptionTemplateItem>
                    {
                        new() { OptionValue = "excellent", OptionLabel = "Excellent", DisplayOrder = 1, ScoreValue = 5, ColorHint = "#28a745" },
                        new() { OptionValue = "good", OptionLabel = "Good", DisplayOrder = 2, ScoreValue = 4, ColorHint = "#6fbf73" },
                        new() { OptionValue = "average", OptionLabel = "Average", DisplayOrder = 3, ScoreValue = 3, ColorHint = "#ffc107" },
                        new() { OptionValue = "below_average", OptionLabel = "Below Average", DisplayOrder = 4, ScoreValue = 2, ColorHint = "#fd7e14" },
                        new() { OptionValue = "poor", OptionLabel = "Poor", DisplayOrder = 5, ScoreValue = 1, ColorHint = "#dc3545" }
                    }
                },

                // ===== CATEGORY: LIKELIHOOD =====
                new FormItemOptionTemplate
                {
                    TemplateName = "Likelihood Scale",
                    TemplateCode = "LIKELIHOOD_5PT",
                    Category = "Rating",
                    SubCategory = "Probability",
                    Description = "Measure likelihood or probability",
                    ApplicableFieldTypes = "[\"Radio\",\"Dropdown\"]",
                    RecommendedFor = "Intent surveys, probability assessment, future behavior prediction",
                    HasScoring = true,
                    ScoringType = "Linear",
                    IsSystemTemplate = true,
                    IsActive = true,
                    DisplayOrder = 9,
                    CreatedDate = DateTime.UtcNow,
                    Items = new List<FormItemOptionTemplateItem>
                    {
                        new() { OptionValue = "very_likely", OptionLabel = "Very Likely", DisplayOrder = 1, ScoreValue = 5 },
                        new() { OptionValue = "likely", OptionLabel = "Likely", DisplayOrder = 2, ScoreValue = 4 },
                        new() { OptionValue = "neutral", OptionLabel = "Neutral", DisplayOrder = 3, ScoreValue = 3 },
                        new() { OptionValue = "unlikely", OptionLabel = "Unlikely", DisplayOrder = 4, ScoreValue = 2 },
                        new() { OptionValue = "very_unlikely", OptionLabel = "Very Unlikely", DisplayOrder = 5, ScoreValue = 1 }
                    }
                },

                // ===== CATEGORY: KTDA-SPECIFIC =====
                new FormItemOptionTemplate
                {
                    TemplateName = "Operational Status",
                    TemplateCode = "OPERATIONAL_STATUS",
                    Category = "Custom",
                    SubCategory = "KTDA",
                    Description = "ICT infrastructure operational status assessment",
                    ApplicableFieldTypes = "[\"Radio\",\"Dropdown\"]",
                    RecommendedFor = "ICT infrastructure reports, operational assessments",
                    HasScoring = true,
                    ScoringType = "Custom",
                    IsSystemTemplate = true,
                    IsActive = true,
                    DisplayOrder = 10,
                    CreatedDate = DateTime.UtcNow,
                    Items = new List<FormItemOptionTemplateItem>
                    {
                        new() { OptionValue = "fully_operational", OptionLabel = "Fully Operational", DisplayOrder = 1, ScoreValue = 10, ColorHint = "#28a745" },
                        new() { OptionValue = "partially_operational", OptionLabel = "Partially Operational", DisplayOrder = 2, ScoreValue = 5, ColorHint = "#ffc107" },
                        new() { OptionValue = "not_operational", OptionLabel = "Not Operational", DisplayOrder = 3, ScoreValue = 0, ColorHint = "#dc3545" }
                    }
                },

                new FormItemOptionTemplate
                {
                    TemplateName = "Condition Assessment",
                    TemplateCode = "CONDITION_ASSESSMENT",
                    Category = "Custom",
                    SubCategory = "KTDA",
                    Description = "Hardware/software condition rating for ICT assets",
                    ApplicableFieldTypes = "[\"Radio\",\"Dropdown\"]",
                    RecommendedFor = "Asset condition tracking, maintenance planning",
                    HasScoring = true,
                    ScoringType = "Custom",
                    IsSystemTemplate = true,
                    IsActive = true,
                    DisplayOrder = 11,
                    CreatedDate = DateTime.UtcNow,
                    Items = new List<FormItemOptionTemplateItem>
                    {
                        new() { OptionValue = "excellent", OptionLabel = "Excellent", DisplayOrder = 1, ScoreValue = 10, ColorHint = "#28a745" },
                        new() { OptionValue = "good", OptionLabel = "Good", DisplayOrder = 2, ScoreValue = 7, ColorHint = "#6fbf73" },
                        new() { OptionValue = "fair", OptionLabel = "Fair", DisplayOrder = 3, ScoreValue = 5, ColorHint = "#ffc107" },
                        new() { OptionValue = "poor", OptionLabel = "Poor", DisplayOrder = 4, ScoreValue = 2, ColorHint = "#fd7e14" },
                        new() { OptionValue = "critical", OptionLabel = "Critical", DisplayOrder = 5, ScoreValue = 0, ColorHint = "#dc3545" }
                    }
                },

                new FormItemOptionTemplate
                {
                    TemplateName = "Compliance Status",
                    TemplateCode = "COMPLIANCE_STATUS",
                    Category = "Custom",
                    SubCategory = "KTDA",
                    Description = "Compliance/conformance status",
                    ApplicableFieldTypes = "[\"Radio\",\"Dropdown\"]",
                    RecommendedFor = "Compliance checks, policy adherence tracking",
                    HasScoring = true,
                    ScoringType = "Custom",
                    IsSystemTemplate = true,
                    IsActive = true,
                    DisplayOrder = 12,
                    CreatedDate = DateTime.UtcNow,
                    Items = new List<FormItemOptionTemplateItem>
                    {
                        new() { OptionValue = "compliant", OptionLabel = "Compliant", DisplayOrder = 1, ScoreValue = 10, ColorHint = "#28a745" },
                        new() { OptionValue = "partially_compliant", OptionLabel = "Partially Compliant", DisplayOrder = 2, ScoreValue = 5, ColorHint = "#ffc107" },
                        new() { OptionValue = "non_compliant", OptionLabel = "Non-Compliant", DisplayOrder = 3, ScoreValue = 0, ColorHint = "#dc3545" }
                    }
                }
            };

            context.FormItemOptionTemplates.AddRange(templates);
            context.SaveChanges();
        }
    }
}
