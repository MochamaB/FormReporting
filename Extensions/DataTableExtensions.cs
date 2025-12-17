using FormReporting.Models.ViewModels.Components;
using Microsoft.AspNetCore.Html;

namespace FormReporting.Extensions
{
    /// <summary>
    /// Extension methods for building data tables from configuration objects
    /// Transforms DataTableConfig → DataTableViewModel for rendering
    /// </summary>
    public static class DataTableExtensions
    {
        /// <summary>
        /// Main transformation method: builds complete DataTableViewModel from config
        /// </summary>
        public static DataTableViewModel BuildDataTable(this DataTableConfig config)
        {
            var viewModel = new DataTableViewModel
            {
                TableId = config.TableId,
                Columns = config.Columns,
                TableContent = config.TableContentRenderer ?? (_ => new HtmlString("")),

                // Pagination
                ShowPagination = config.ShowPagination,
                CurrentPage = config.CurrentPage,
                TotalPages = config.TotalPages,
                TotalRecords = config.TotalRecords,
                PageSize = config.PageSize,

                // Bulk Actions
                EnableBulkActions = config.EnableBulkActions,

                // Sorting
                EnableSorting = config.EnableSorting,
                NonSortableColumns = config.NonSortableColumns ?? new List<int>(),

                // Create button
                CreateButtonText = config.CreateButtonText,
                CreateButtonUrl = config.CreateButtonUrl
            };

            // Build search box
            if (config.EnableSearch && config.SearchBox != null)
            {
                viewModel.SearchBox = BuildSearchBox(config.SearchBox);
            }

            // Build filter dropdowns
            if (config.FilterDropdowns != null && config.FilterDropdowns.Any())
            {
                viewModel.FilterDropdowns = config.FilterDropdowns
                    .Select(BuildFilterDropdown)
                    .ToList();
            }

            // Build filter selects
            if (config.FilterSelects != null && config.FilterSelects.Any())
            {
                viewModel.FilterSelects = config.FilterSelects
                    .Select(BuildFilterSelect)
                    .ToList();
            }

            // Build header actions
            if (config.HeaderActions != null && config.HeaderActions.Any())
            {
                viewModel.HeaderActions = config.HeaderActions
                    .Select(BuildHeaderAction)
                    .ToList();
            }

            // Build bulk actions
            if (config.EnableBulkActions && config.BulkActions != null && config.BulkActions.Any())
            {
                viewModel.BulkActions = config.BulkActions
                    .Select(BuildBulkAction)
                    .ToList();
            }

            // Build table CSS classes
            viewModel.TableClasses = BuildTableClasses(config);

            // View toggle
            viewModel.EnableViewToggle = config.EnableViewToggle;
            viewModel.DefaultView = config.DefaultView;
            viewModel.CardContent = config.CardContentRenderer;

            // AJAX loading
            viewModel.EnableAjaxLoading = config.EnableAjaxLoading;
            viewModel.AjaxUrl = config.AjaxUrl;
            viewModel.SkeletonRowCount = config.SkeletonRowCount;
            viewModel.AjaxQueryParams = config.AjaxQueryParams;

            // Build AJAX columns
            if (config.EnableAjaxLoading && config.AjaxColumns != null && config.AjaxColumns.Any())
            {
                viewModel.AjaxColumns = config.AjaxColumns
                    .Select(BuildAjaxColumn)
                    .ToList();
            }

            // Add Actions column to NonSortableColumns if not already there
            if (config.EnableSorting && config.Columns.Any())
            {
                var actionsColumnIndex = config.Columns.Count - 1; // Assume last column is actions
                if (!viewModel.NonSortableColumns.Contains(actionsColumnIndex))
                {
                    viewModel.NonSortableColumns.Add(actionsColumnIndex);
                }
            }

            // For AJAX mode, also add checkbox column (index 0) to non-sortable
            if (config.EnableAjaxLoading && config.EnableBulkActions)
            {
                if (!viewModel.NonSortableColumns.Contains(0))
                {
                    viewModel.NonSortableColumns.Insert(0, 0);
                }
            }

            return viewModel;
        }

        // ========== FLUENT API METHODS ==========

        /// <summary>
        /// Fluent API: Add search box
        /// </summary>
        public static DataTableConfig WithSearch(this DataTableConfig config,
            string parameterName = "search",
            string placeholder = "Search...",
            string? currentValue = null)
        {
            config.EnableSearch = true;
            config.SearchBox = new SearchBoxConfig
            {
                ParameterName = parameterName,
                PlaceholderText = placeholder,
                CurrentValue = currentValue,
                ActionUrl = "", // Will use current URL
                ShowButton = true
            };
            return config;
        }

        /// <summary>
        /// Fluent API: Add simple filter dropdown
        /// </summary>
        public static DataTableConfig WithFilterDropdown(this DataTableConfig config,
            string label,
            List<FilterOption> options)
        {
            config.FilterDropdowns ??= new List<FilterDropdownConfig>();
            config.FilterDropdowns.Add(new FilterDropdownConfig
            {
                Label = label,
                Options = options
            });
            return config;
        }

        /// <summary>
        /// Fluent API: Add advanced filter select
        /// </summary>
        public static DataTableConfig WithFilterSelect(this DataTableConfig config,
            string parameterName,
            string placeholder,
            List<FilterOption> options)
        {
            config.FilterSelects ??= new List<FilterSelectConfig>();
            config.FilterSelects.Add(new FilterSelectConfig
            {
                ParameterName = parameterName,
                PlaceholderText = placeholder,
                Options = options,
                ActionUrl = ""
            });
            return config;
        }

        /// <summary>
        /// Fluent API: Add header action button
        /// </summary>
        public static DataTableConfig WithHeaderAction(this DataTableConfig config,
            string text,
            string url,
            string iconClass = "",
            string colorClass = "primary")
        {
            config.HeaderActions ??= new List<HeaderActionConfig>();
            config.HeaderActions.Add(new HeaderActionConfig
            {
                Text = text,
                Url = url,
                IconClass = iconClass,
                ColorClass = colorClass
            });
            return config;
        }

        /// <summary>
        /// Fluent API: Add create button (shorthand)
        /// </summary>
        public static DataTableConfig WithCreateButton(this DataTableConfig config,
            string text,
            string url)
        {
            config.CreateButtonText = text;
            config.CreateButtonUrl = url;
            return config;
        }

        /// <summary>
        /// Fluent API: Enable bulk actions
        /// </summary>
        public static DataTableConfig WithBulkActions(this DataTableConfig config,
            params BulkActionConfig[] actions)
        {
            config.EnableBulkActions = true;
            config.BulkActions = actions.ToList();
            return config;
        }

        /// <summary>
        /// Fluent API: Set pagination info
        /// </summary>
        public static DataTableConfig WithPagination(this DataTableConfig config,
            int currentPage,
            int totalPages,
            int totalRecords,
            int pageSize = 10)
        {
            config.ShowPagination = true;
            config.CurrentPage = currentPage;
            config.TotalPages = totalPages;
            config.TotalRecords = totalRecords;
            config.PageSize = pageSize;
            return config;
        }

        /// <summary>
        /// Fluent API: Set table styling
        /// </summary>
        public static DataTableConfig WithStyling(this DataTableConfig config,
            bool hover = true,
            bool striped = false,
            bool bordered = false,
            string? size = null)
        {
            config.EnableHover = hover;
            config.EnableStriped = striped;
            config.EnableBordered = bordered;
            config.TableSize = size;
            return config;
        }

        /// <summary>
        /// Fluent API: Enable view toggle (table/card)
        /// </summary>
        public static DataTableConfig WithViewToggle(this DataTableConfig config,
            Func<object?, IHtmlContent> cardContentRenderer,
            string defaultView = "table")
        {
            config.EnableViewToggle = true;
            config.DefaultView = defaultView;
            config.CardContentRenderer = cardContentRenderer;
            return config;
        }

        // ========== PRIVATE BUILDER METHODS ==========

        private static SearchBoxViewModel BuildSearchBox(SearchBoxConfig config)
        {
            return new SearchBoxViewModel
            {
                ParameterName = config.ParameterName,
                PlaceholderText = config.PlaceholderText,
                CurrentValue = config.CurrentValue,
                ActionUrl = config.ActionUrl,
                ShowButton = config.ShowButton,
                InputId = config.InputId,
                PreserveQueryParams = config.PreserveQueryParams
            };
        }

        private static FilterDropdownViewModel BuildFilterDropdown(FilterDropdownConfig config)
        {
            return new FilterDropdownViewModel
            {
                Label = config.Label,
                Options = config.Options.Select(o => new FilterOptionViewModel
                {
                    Text = o.Text,
                    Value = o.Value,
                    Url = o.Url,
                    IsActive = o.IsActive
                }).ToList()
            };
        }

        private static FilterSelectViewModel BuildFilterSelect(FilterSelectConfig config)
        {
            return new FilterSelectViewModel
            {
                ParameterName = config.ParameterName,
                PlaceholderText = config.PlaceholderText,
                ActionUrl = config.ActionUrl,
                FormId = config.FormId,
                Options = config.Options.Select(o => new FilterOptionViewModel
                {
                    Text = o.Text,
                    Value = o.Value,
                    IsSelected = o.IsSelected
                }).ToList(),
                PreserveQueryParams = config.PreserveQueryParams
            };
        }

        private static HeaderActionViewModel BuildHeaderAction(HeaderActionConfig config)
        {
            return new HeaderActionViewModel
            {
                Text = config.Text,
                Url = config.Url,
                IconClass = config.IconClass,
                ColorClass = config.ColorClass
            };
        }

        private static BulkActionViewModel BuildBulkAction(BulkActionConfig config)
        {
            return new BulkActionViewModel
            {
                Text = config.Text,
                ActionUrl = config.ActionUrl,
                IconClass = config.IconClass,
                ColorClass = config.ColorClass,
                RequiresConfirmation = config.RequiresConfirmation,
                ConfirmationMessage = config.ConfirmationMessage ?? $"Are you sure you want to {config.Text.ToLower()}?"
            };
        }

        private static string BuildTableClasses(DataTableConfig config)
        {
            var classes = new List<string> { "table", "mb-0" };

            if (config.EnableHover)
                classes.Add("table-hover");

            if (config.EnableStriped)
                classes.Add("table-striped");

            if (config.EnableBordered)
                classes.Add("table-bordered");

            if (!string.IsNullOrEmpty(config.TableSize))
                classes.Add($"table-{config.TableSize}");

            return string.Join(" ", classes);
        }

        private static AjaxColumnViewModel BuildAjaxColumn(AjaxColumnConfig config)
        {
            return new AjaxColumnViewModel
            {
                FieldName = config.FieldName,
                DisplayType = config.DisplayType,
                BadgeColorField = config.BadgeColorField,
                IconField = config.IconField,
                SecondaryField = config.SecondaryField,
                DateFormat = config.DateFormat,
                Actions = config.Actions?.Select(a => new AjaxRowActionViewModel
                {
                    Text = a.Text,
                    IconClass = a.IconClass,
                    Url = a.Url,
                    OnClick = a.OnClick,
                    ColorClass = a.ColorClass,
                    IsDivider = a.IsDivider
                }).ToList()
            };
        }

        // ========== AJAX MODE FLUENT API ==========

        /// <summary>
        /// Fluent API: Enable AJAX loading mode
        /// </summary>
        public static DataTableConfig WithAjaxLoading(this DataTableConfig config,
            string ajaxUrl,
            int skeletonRowCount = 5)
        {
            config.EnableAjaxLoading = true;
            config.AjaxUrl = ajaxUrl;
            config.SkeletonRowCount = skeletonRowCount;
            config.AjaxColumns = new List<AjaxColumnConfig>();
            return config;
        }

        /// <summary>
        /// Fluent API: Add AJAX query parameter
        /// </summary>
        public static DataTableConfig WithAjaxParam(this DataTableConfig config,
            string key, string value)
        {
            config.AjaxQueryParams ??= new Dictionary<string, string>();
            config.AjaxQueryParams[key] = value;
            return config;
        }

        /// <summary>
        /// Fluent API: Add checkbox column for AJAX mode
        /// </summary>
        public static DataTableConfig WithCheckboxColumn(this DataTableConfig config, string idField = "id")
        {
            config.AjaxColumns ??= new List<AjaxColumnConfig>();
            config.AjaxColumns.Add(new AjaxColumnConfig
            {
                FieldName = idField,
                DisplayType = "checkbox"
            });
            return config;
        }

        /// <summary>
        /// Fluent API: Add text column for AJAX mode
        /// </summary>
        public static DataTableConfig WithTextColumn(this DataTableConfig config,
            string fieldName,
            string? secondaryField = null)
        {
            config.AjaxColumns ??= new List<AjaxColumnConfig>();
            config.AjaxColumns.Add(new AjaxColumnConfig
            {
                FieldName = fieldName,
                DisplayType = "text",
                SecondaryField = secondaryField
            });
            return config;
        }

        /// <summary>
        /// Fluent API: Add avatar column for AJAX mode (icon + text + secondary)
        /// </summary>
        public static DataTableConfig WithAvatarColumn(this DataTableConfig config,
            string fieldName,
            string? iconField = null,
            string? secondaryField = null,
            string? colorField = null)
        {
            config.AjaxColumns ??= new List<AjaxColumnConfig>();
            config.AjaxColumns.Add(new AjaxColumnConfig
            {
                FieldName = fieldName,
                DisplayType = "avatar",
                IconField = iconField,
                SecondaryField = secondaryField,
                BadgeColorField = colorField
            });
            return config;
        }

        /// <summary>
        /// Fluent API: Add badge column for AJAX mode
        /// </summary>
        public static DataTableConfig WithBadgeColumn(this DataTableConfig config,
            string fieldName,
            string? colorField = null)
        {
            config.AjaxColumns ??= new List<AjaxColumnConfig>();
            config.AjaxColumns.Add(new AjaxColumnConfig
            {
                FieldName = fieldName,
                DisplayType = "badge",
                BadgeColorField = colorField
            });
            return config;
        }

        /// <summary>
        /// Fluent API: Add date column for AJAX mode
        /// </summary>
        public static DataTableConfig WithDateColumn(this DataTableConfig config,
            string fieldName,
            string format = "MMM dd, yyyy")
        {
            config.AjaxColumns ??= new List<AjaxColumnConfig>();
            config.AjaxColumns.Add(new AjaxColumnConfig
            {
                FieldName = fieldName,
                DisplayType = "date",
                DateFormat = format
            });
            return config;
        }

        /// <summary>
        /// Fluent API: Add actions column for AJAX mode
        /// </summary>
        public static DataTableConfig WithActionsColumn(this DataTableConfig config,
            params AjaxRowActionConfig[] actions)
        {
            config.AjaxColumns ??= new List<AjaxColumnConfig>();
            config.AjaxColumns.Add(new AjaxColumnConfig
            {
                FieldName = "",
                DisplayType = "actions",
                Actions = actions.ToList()
            });
            return config;
        }

        // ========== HELPER METHODS FOR CONTROLLERS ==========

        /// <summary>
        /// Helper: Calculate pagination values from query params
        /// </summary>
        public static (int currentPage, int totalPages, int skip, int take) CalculatePagination(
            int? page,
            int totalRecords,
            int pageSize = 10)
        {
            int currentPage = page ?? 1;
            if (currentPage < 1) currentPage = 1;

            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (totalPages < 1) totalPages = 1;

            if (currentPage > totalPages) currentPage = totalPages;

            int skip = (currentPage - 1) * pageSize;
            int take = pageSize;

            return (currentPage, totalPages, skip, take);
        }

        /// <summary>
        /// Helper: Build URL with query string parameter
        /// </summary>
        public static string BuildUrl(string baseUrl, string paramName, string paramValue, Dictionary<string, string>? existingParams = null)
        {
            var queryParams = new Dictionary<string, string>(existingParams ?? new Dictionary<string, string>())
            {
                [paramName] = paramValue
            };

            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            return $"{baseUrl}?{queryString}";
        }

        /// <summary>
        /// Helper: Create default test config for rapid prototyping
        /// </summary>
        public static DataTableConfig CreateDefaultTestConfig()
        {
            return new DataTableConfig
            {
                Columns = new List<string> { "ID", "Name", "Email", "Role", "Status", "Actions" },
                EnableSearch = true,
                SearchBox = new SearchBoxConfig
                {
                    PlaceholderText = "Search users...",
                    CurrentValue = null
                },
                FilterDropdowns = new List<FilterDropdownConfig>
                {
                    new FilterDropdownConfig
                    {
                        Label = "Status",
                        Options = new List<FilterOption>
                        {
                            new FilterOption { Text = "All", Value = "", IsActive = true, Url = "?status=" },
                            new FilterOption { Text = "Active", Value = "active", IsActive = false, Url = "?status=active" },
                            new FilterOption { Text = "Inactive", Value = "inactive", IsActive = false, Url = "?status=inactive" }
                        }
                    }
                },
                FilterSelects = new List<FilterSelectConfig>
                {
                    new FilterSelectConfig
                    {
                        ParameterName = "role",
                        PlaceholderText = "Filter by Role",
                        Options = new List<FilterOption>
                        {
                            new FilterOption { Text = "HeadOffice Admin", Value = "1", IsSelected = false },
                            new FilterOption { Text = "Regional Manager", Value = "2", IsSelected = false },
                            new FilterOption { Text = "Factory User", Value = "3", IsSelected = false }
                        }
                    }
                },
                CreateButtonText = "Create User",
                CreateButtonUrl = "/users/create",
                ShowPagination = true,
                CurrentPage = 1,
                TotalPages = 5,
                TotalRecords = 47,
                PageSize = 10,
                EnableSorting = true,
                EnableHover = true
            };
        }
    }
}
