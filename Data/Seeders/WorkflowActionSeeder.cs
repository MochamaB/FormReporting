using FormReporting.Models.Entities.Forms;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Data.Seeders
{
    public static class WorkflowActionSeeder
    {
        public static void SeedWorkflowActions(ApplicationDbContext context)
        {
            if (context.WorkflowActions.Any())
                return;

            var actions = new List<WorkflowAction>
            {
                new()
                {
                    ActionCode = "Fill",
                    ActionName = "Fill Form",
                    Description = "Complete form fields with data",
                    RequiresSignature = false,
                    RequiresComment = false,
                    AllowDelegate = true,
                    IconClass = "bi-pencil-square",
                    CssClass = "text-primary",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new()
                {
                    ActionCode = "Sign",
                    ActionName = "Sign",
                    Description = "Digitally sign the form or section",
                    RequiresSignature = true,
                    RequiresComment = false,
                    AllowDelegate = false,
                    IconClass = "bi-pen",
                    CssClass = "text-info",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new()
                {
                    ActionCode = "Review",
                    ActionName = "Review & Approve",
                    Description = "Review the submission and approve or reject",
                    RequiresSignature = false,
                    RequiresComment = false,
                    AllowDelegate = true,
                    IconClass = "bi-clipboard-check",
                    CssClass = "text-info",
                    DisplayOrder = 3,
                    IsActive = true
                },
                new()
                {
                    ActionCode = "Verify",
                    ActionName = "Verify",
                    Description = "Verify data accuracy and completeness",
                    RequiresSignature = false,
                    RequiresComment = false,
                    AllowDelegate = true,
                    IconClass = "bi-shield-check",
                    CssClass = "text-warning",
                    DisplayOrder = 4,
                    IsActive = true
                }
            };

            context.WorkflowActions.AddRange(actions);
            context.SaveChanges();
        }
    }
}
