using FormReporting.Models.ViewModels.Components;

namespace FormReporting.Extensions
{
    /// <summary>
    /// Extension methods for building row action buttons (View/Edit/Delete)
    /// Transforms RowActionsConfig → RowActionsViewModel for rendering
    /// </summary>
    public static class RowActionsExtensions
    {
        /// <summary>
        /// Main transformation method: builds RowActionsViewModel from config
        /// </summary>
        /// <param name="config">Row actions configuration</param>
        /// <param name="rowId">The ID of the current row (replaces {id} in URL templates)</param>
        /// <returns>ViewModel ready for rendering</returns>
        public static RowActionsViewModel BuildRowActions(this RowActionsConfig config, object rowId)
        {
            var viewModel = new RowActionsViewModel
            {
                DisplayStyle = config.DisplayStyle,
                DropdownText = config.DropdownText,
                DropdownIconClass = config.DropdownIconClass,
                UseSoftButtons = config.UseSoftButtons,
                RowId = rowId.ToString() ?? "",
                ButtonSizeClass = config.ButtonSize switch
                {
                    "sm" => "btn-sm",
                    "lg" => "btn-lg",
                    _ => ""
                }
            };

            // Transform each action
            viewModel.Actions = config.Actions
                .Select(action => TransformAction(action, rowId.ToString() ?? ""))
                .Where(action => action.IsVisible) // Filter out actions based on permissions
                .ToList();

            return viewModel;
        }

        // ========== FLUENT API METHODS ==========

        /// <summary>
        /// Fluent API: Add a custom action
        /// </summary>
        public static RowActionsConfig WithAction(
            this RowActionsConfig config,
            string text,
            string iconClass,
            string urlTemplate,
            string colorClass = "primary",
            bool requiresConfirmation = false,
            string? confirmationMessage = null,
            bool iconOnly = true)
        {
            config.Actions.Add(new RowActionConfig
            {
                Text = text,
                IconClass = iconClass,
                UrlTemplate = urlTemplate,
                ColorClass = colorClass,
                RequiresConfirmation = requiresConfirmation,
                ConfirmationMessage = confirmationMessage,
                IconOnly = iconOnly
            });
            return config;
        }

        /// <summary>
        /// Fluent API: Add View action
        /// </summary>
        public static RowActionsConfig WithViewAction(this RowActionsConfig config, string urlTemplate = "/view/{id}")
        {
            return config.WithAction("View", "ri-eye-line", urlTemplate, "primary", false, null, true);
        }

        /// <summary>
        /// Fluent API: Add Edit action
        /// </summary>
        public static RowActionsConfig WithEditAction(this RowActionsConfig config, string urlTemplate = "/edit/{id}")
        {
            return config.WithAction("Edit", "ri-pencil-line", urlTemplate, "success", false, null, true);
        }

        /// <summary>
        /// Fluent API: Add Delete action
        /// </summary>
        public static RowActionsConfig WithDeleteAction(
            this RowActionsConfig config,
            string urlTemplate = "/delete/{id}",
            string confirmationMessage = "Are you sure you want to delete this item?")
        {
            return config.WithAction("Delete", "ri-delete-bin-line", urlTemplate, "danger", true, confirmationMessage, true);
        }

        /// <summary>
        /// Fluent API: Add Duplicate action
        /// </summary>
        public static RowActionsConfig WithDuplicateAction(this RowActionsConfig config, string urlTemplate = "/duplicate/{id}")
        {
            return config.WithAction("Duplicate", "ri-file-copy-line", urlTemplate, "info", false, null, true);
        }

        /// <summary>
        /// Fluent API: Add Download action
        /// </summary>
        public static RowActionsConfig WithDownloadAction(this RowActionsConfig config, string urlTemplate = "/download/{id}")
        {
            return config.WithAction("Download", "ri-download-line", urlTemplate, "secondary", false, null, true);
        }

        /// <summary>
        /// Fluent API: Add Archive action
        /// </summary>
        public static RowActionsConfig WithArchiveAction(
            this RowActionsConfig config,
            string urlTemplate = "/archive/{id}",
            string confirmationMessage = "Are you sure you want to archive this item?")
        {
            return config.WithAction("Archive", "ri-archive-line", urlTemplate, "warning", true, confirmationMessage, true);
        }

        /// <summary>
        /// Fluent API: Set display style to Dropdown
        /// </summary>
        public static RowActionsConfig AsDropdown(this RowActionsConfig config, string dropdownText = "Actions")
        {
            config.DisplayStyle = RowActionDisplayStyle.Dropdown;
            config.DropdownText = dropdownText;
            return config;
        }

        /// <summary>
        /// Fluent API: Set display style to Inline
        /// </summary>
        public static RowActionsConfig AsInline(this RowActionsConfig config)
        {
            config.DisplayStyle = RowActionDisplayStyle.Inline;
            return config;
        }

        /// <summary>
        /// Fluent API: Use solid buttons instead of soft buttons
        /// </summary>
        public static RowActionsConfig UseSolidButtons(this RowActionsConfig config)
        {
            config.UseSoftButtons = false;
            return config;
        }

        /// <summary>
        /// Create a default row actions config with View, Edit, Delete
        /// </summary>
        public static RowActionsConfig CreateDefaultRowActions(
            string viewUrlTemplate = "/view/{id}",
            string editUrlTemplate = "/edit/{id}",
            string deleteUrlTemplate = "/delete/{id}")
        {
            return new RowActionsConfig()
                .WithViewAction(viewUrlTemplate)
                .WithEditAction(editUrlTemplate)
                .WithDeleteAction(deleteUrlTemplate);
        }

        /// <summary>
        /// Generate HTML string for row actions (for use in StringBuilder scenarios)
        /// </summary>
        /// <param name="config">Row actions configuration</param>
        /// <param name="rowId">The ID of the current row</param>
        /// <returns>HTML string of action buttons</returns>
        public static string RenderActionsHtml(this RowActionsConfig config, object rowId)
        {
            var viewModel = config.BuildRowActions(rowId);
            var sb = new System.Text.StringBuilder();

            if (viewModel.DisplayStyle == RowActionDisplayStyle.Inline)
            {
                // Render inline buttons
                sb.Append("<div class='d-flex gap-2'>");
                foreach (var action in viewModel.Actions)
                {
                    var buttonClass = viewModel.UseSoftButtons ? $"btn-soft-{action.ColorClass}" : $"btn-{action.ColorClass}";

                    if (action.RequiresConfirmation)
                    {
                        sb.Append($@"<a href='#' class='btn {buttonClass} {viewModel.ButtonSizeClass} action-with-confirm'
                                    data-url='{action.Url}' data-message='{action.ConfirmationMessage}' title='{action.Text}'>
                                    <i class='{action.IconClass}'></i>");
                        if (!action.IconOnly)
                        {
                            sb.Append($"<span class='ms-1'>{action.Text}</span>");
                        }
                        sb.Append("</a>");
                    }
                    else
                    {
                        sb.Append($@"<a href='{action.Url}' class='btn {buttonClass} {viewModel.ButtonSizeClass}' title='{action.Text}'>
                                    <i class='{action.IconClass}'></i>");
                        if (!action.IconOnly)
                        {
                            sb.Append($"<span class='ms-1'>{action.Text}</span>");
                        }
                        sb.Append("</a>");
                    }
                }
                sb.Append("</div>");
            }
            else
            {
                // Render dropdown
                sb.Append("<div class='dropdown'>");
                sb.Append($@"<button class='btn btn-soft-secondary {viewModel.ButtonSizeClass} dropdown-toggle' type='button'
                            data-bs-toggle='dropdown' aria-expanded='false'>");
                if (!string.IsNullOrEmpty(viewModel.DropdownIconClass))
                {
                    sb.Append($"<i class='{viewModel.DropdownIconClass} me-1'></i>");
                }
                sb.Append($"{viewModel.DropdownText}</button>");
                sb.Append("<ul class='dropdown-menu dropdown-menu-end'>");
                foreach (var action in viewModel.Actions)
                {
                    sb.Append("<li>");
                    if (action.RequiresConfirmation)
                    {
                        sb.Append($@"<a class='dropdown-item action-with-confirm' href='#'
                                    data-url='{action.Url}' data-message='{action.ConfirmationMessage}'>");
                    }
                    else
                    {
                        sb.Append($"<a class='dropdown-item' href='{action.Url}'>");
                    }
                    if (!string.IsNullOrEmpty(action.IconClass))
                    {
                        sb.Append($"<i class='{action.IconClass} me-2 text-{action.ColorClass}'></i>");
                    }
                    sb.Append($"{action.Text}</a></li>");
                }
                sb.Append("</ul></div>");
            }

            return sb.ToString();
        }

        // ========== PRIVATE HELPER METHODS ==========

        /// <summary>
        /// Transform a single action config to view model
        /// </summary>
        private static RowActionViewModel TransformAction(RowActionConfig action, string rowId)
        {
            return new RowActionViewModel
            {
                Text = action.Text,
                IconClass = action.IconClass,
                ColorClass = action.ColorClass,
                Url = action.UrlTemplate.Replace("{id}", rowId),
                RequiresConfirmation = action.RequiresConfirmation,
                ConfirmationMessage = action.ConfirmationMessage,
                IconOnly = action.IconOnly,
                IsVisible = true // TODO: Check permissions when permission system is implemented
            };
        }
    }
}
