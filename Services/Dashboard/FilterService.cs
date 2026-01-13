using FormReporting.Data;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.ViewModels.Dashboard.Components.Composite;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Services.Forms;
using FormReporting.Services.Organizational;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FormReporting.Services.Dashboard
{
    /// <summary>
    /// Implementation of dashboard filter operations
    /// Provides reusable filter components for all dashboards with scope-based security
    /// </summary>
    public class FilterService : IFilterService
    {
        private readonly IRegionService _regionService;
        private readonly ITenantService _tenantService;
        private readonly IFormSubmissionStatisticsService _statisticsService;
        private readonly ApplicationDbContext _context;

        public FilterService(
            IRegionService regionService,
            ITenantService tenantService,
            IFormSubmissionStatisticsService statisticsService,
            ApplicationDbContext context)
        {
            _regionService = regionService;
            _tenantService = tenantService;
            _statisticsService = statisticsService;
            _context = context;
        }

        /// <summary>
        /// Build standard filter panel configuration for dashboards
        /// </summary>
        public async Task<FilterPanelConfig> BuildStandardFiltersAsync(
            ClaimsPrincipal currentUser,
            int? templateId = null,
            string? mode = null)
        {
            var filters = new List<FilterInput>();
            var advancedFilters = new List<FilterInput>();

            // Get filter options
            var regionOptions = await GetRegionOptionsAsync(currentUser);
            var tenantOptions = await GetTenantOptionsAsync(currentUser);
            var statusOptions = await GetStatusOptionsAsync(currentUser, templateId);
            var submitterOptions = await GetSubmitterOptionsAsync(currentUser, templateId);
            var groupByOptions = GetGroupByOptions();

            // Template filter only for overall dashboard
            if (!templateId.HasValue && mode == "FullPage")
            {
                var templateOptions = await GetTemplateOptionsAsync(currentUser);
                filters.Add(FilterInput.Dropdown("Template", "templateId", templateOptions, ""));
            }

            // MAIN FILTERS (Always Visible)
            
            // 1. Region filter
            filters.Add(new FilterInput
            {
                Label = "Region",
                Name = "regionId",
                Type = FilterInputType.DropdownButton,
                Options = regionOptions,
                ColumnClass = ""
            });

            // 2. Tenant filter (will be updated via JavaScript when region changes)
            filters.Add(new FilterInput
            {
                Label = "Tenant",
                Name = "tenantId",
                Type = FilterInputType.DropdownButton,
                Options = tenantOptions,
                ColumnClass = ""
            });

            // 3. Status filter
            filters.Add(new FilterInput
            {
                Label = "Status",
                Name = "status",
                Type = FilterInputType.DropdownButton,
                Options = statusOptions,
                ColumnClass = ""
            });

            // ADVANCED FILTERS (Collapsible Section)
            
            // 4. Start Date
            advancedFilters.Add(new FilterInput
            {
                Label = "Start Date",
                Name = "startDate",
                Type = FilterInputType.Date,
                ColumnClass = ""
            });

            // 5. End Date
            advancedFilters.Add(new FilterInput
            {
                Label = "End Date", 
                Name = "endDate",
                Type = FilterInputType.Date,
                ColumnClass = ""
            });

            // 6. Submitter
            advancedFilters.Add(new FilterInput
            {
                Label = "Submitter",
                Name = "submitterId",
                Type = FilterInputType.Select,
                Options = submitterOptions,
                ColumnClass = ""
            });

            // 7. Group By (only for charts)
            if (templateId.HasValue)
            {
                advancedFilters.Add(new FilterInput
                {
                    Label = "Group By",
                    Name = "groupBy",
                    Type = FilterInputType.Select,
                    Options = groupByOptions,
                    ColumnClass = ""
                });
            }

            return new FilterPanelConfig
            {
                Id = "dashboard-filters",
                Title = null,
                ShowCard = false,
                Collapsible = false,
                ShowApplyButton = false,
                ShowClearButton = false,
                Method = "GET",
                Filters = filters,
                AdvancedFilters = advancedFilters.Any() ? advancedFilters : null
            };
        }

        /// <summary>
        /// Get region filter options based on user scope
        /// </summary>
        public async Task<List<SelectOption>> GetRegionOptionsAsync(ClaimsPrincipal currentUser)
        {
            var regions = await _regionService.GetAccessibleRegionsAsync(currentUser);

            var options = regions
                .Select(r => new SelectOption
                {
                    Value = r.RegionId.ToString(),
                    Text = r.RegionName,
                    Selected = false
                })
                .OrderBy(r => r.Text)
                .ToList();

            // Add "All Regions" option at the top if not already present
            if (!options.Any(o => string.IsNullOrEmpty(o.Value)))
            {
                options.Insert(0, new SelectOption
                {
                    Value = "",
                    Text = "All Regions",
                    Selected = true
                });
            }

            return options;
        }

        /// <summary>
        /// Get tenant filter options based on user scope and optional region filter
        /// </summary>
        public async Task<List<SelectOption>> GetTenantOptionsAsync(ClaimsPrincipal currentUser, int? regionId = null)
        {
            var tenants = await _tenantService.GetAccessibleTenantsAsync(currentUser);

            // Apply region filter if specified
            if (regionId.HasValue)
            {
                tenants = tenants.Where(t => t.RegionId == regionId.Value).ToList();
            }

            var options = tenants
                .Select(t => new SelectOption
                {
                    Value = t.TenantId.ToString(),
                    Text = t.TenantName,
                    Selected = false
                })
                .OrderBy(t => t.Text)
                .ToList();

            // Add "All Tenants" option at the top if not already present
            if (!options.Any(o => string.IsNullOrEmpty(o.Value)))
            {
                options.Insert(0, new SelectOption
                {
                    Value = "",
                    Text = "All Tenants",
                    Selected = true
                });
            }

            return options;
        }

        private List<int> GetAccessibleTemplateIdsAsync(ClaimsPrincipal currentUser)
        {
            // ✅ Get template IDs directly from claims (no database queries!)
            var templateAccessClaims = currentUser.FindAll("TemplateAccess");
            
            return templateAccessClaims
                .Where(c => int.TryParse(c.Value, out _))
                .Select(c => int.Parse(c.Value))
                .ToList();
        }

        /// <summary>
        /// Get status filter options based on template submissions
        /// </summary>
        public async Task<List<SelectOption>> GetStatusOptionsAsync(ClaimsPrincipal currentUser, int? templateId = null)
        {
            List<SubmissionSummaryViewModel> submissions;
            
            if (templateId.HasValue)
            {
                // Single template - use existing service method
                submissions = await _statisticsService.GetRecentSubmissionsAsync(templateId.Value, 1000, null, currentUser);
            }
            else
            {
                // Overall view - get accessible templates and use scope-aware method
                var accessibleTemplateIds = GetAccessibleTemplateIdsAsync(currentUser);
                
                if (accessibleTemplateIds.Any())
                {
                    submissions = await _statisticsService.GetRecentSubmissionsAsync(accessibleTemplateIds, 1000, null, currentUser);
                }
                else
                {
                    submissions = new List<SubmissionSummaryViewModel>();
                }
            }

            var statusOptions = submissions
                .Where(s => !string.IsNullOrEmpty(s.Status))
                .GroupBy(s => s.Status)
                .Select(g => new SelectOption
                {
                    Value = g.Key,
                    Text = $"{g.Key} ({g.Count()})",
                    Selected = false
                })
                .OrderBy(x => x.Text)
                .ToList();

            // Add "All Status" option at the top
            statusOptions.Insert(0, new SelectOption
            {
                Value = "",
                Text = "All Status",
                Selected = true
            });

            // If no submissions, add common status options
            if (statusOptions.Count <= 1)
            {
                statusOptions.AddRange(new List<SelectOption>
                {
                    new SelectOption { Value = "Draft", Text = "Draft", Selected = false },
                    new SelectOption { Value = "Submitted", Text = "Submitted", Selected = false },
                    new SelectOption { Value = "Approved", Text = "Approved", Selected = false },
                    new SelectOption { Value = "Rejected", Text = "Rejected", Selected = false }
                });
            }

            return statusOptions;
        }

        /// <summary>
        /// Get submitter filter options based on template submissions
        /// </summary>
        public async Task<List<SelectOption>> GetSubmitterOptionsAsync(ClaimsPrincipal currentUser, int? templateId = null)
        {
            List<SubmissionSummaryViewModel> submissions;
            
            if (templateId.HasValue)
            {
                // Single template - use existing service method
                submissions = await _statisticsService.GetRecentSubmissionsAsync(templateId.Value, 1000, null, currentUser);
            }
            else
            {
                // Overall view - get accessible templates and use scope-aware method
                var accessibleTemplateIds = GetAccessibleTemplateIdsAsync(currentUser);
                
                if (accessibleTemplateIds.Any())
                {
                    submissions = await _statisticsService.GetRecentSubmissionsAsync(accessibleTemplateIds, 1000, null, currentUser);
                }
                else
                {
                    submissions = new List<SubmissionSummaryViewModel>();
                }
            }

            var submitterOptions = submissions
                .Where(s => !string.IsNullOrEmpty(s.SubmittedBy))
                .GroupBy(s => s.SubmittedBy)
                .Select(g => new SelectOption
                {
                    Value = g.Key,
                    Text = $"{g.Key} ({g.Count()})",
                    Selected = false
                })
                .OrderBy(x => x.Text)
                .ToList();

            // Add "All Submitters" option at the top
            submitterOptions.Insert(0, new SelectOption
            {
                Value = "",
                Text = "All Submitters",
                Selected = true
            });

            return submitterOptions;
        }

        /// <summary>
        /// Get template filter options for overall dashboard
        /// Now uses claims-based template access from assignments
        /// </summary>
        public async Task<List<SelectOption>> GetTemplateOptionsAsync(ClaimsPrincipal currentUser)
        {
            // Get template IDs from claims (built from assignments during login)
            var templateAccessClaims = currentUser.FindAll("TemplateAccess");
            var accessibleTemplateIds = templateAccessClaims
                .Where(c => int.TryParse(c.Value, out _))
                .Select(c => int.Parse(c.Value))
                .ToList();
            
            if (!accessibleTemplateIds.Any())
            {
                return new List<SelectOption>();
            }
            
            // Get template details for accessible templates
            var templates = await _context.FormTemplates
                .Where(t => accessibleTemplateIds.Contains(t.TemplateId) && t.PublishStatus == "Published")
                .OrderBy(t => t.TemplateName)
                .Select(t => new SelectOption
                {
                    Value = t.TemplateId.ToString(),
                    Text = t.TemplateName,
                    Selected = false
                })
                .ToListAsync();

            // TEMPORARY: Log template filtering for debugging
            System.Diagnostics.Debug.WriteLine($"=== FILTER SERVICE: TEMPLATES FROM CLAIMS ===");
            System.Diagnostics.Debug.WriteLine($"User: {currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value}");
            System.Diagnostics.Debug.WriteLine($"Template Count from Claims: {accessibleTemplateIds.Count}");
            System.Diagnostics.Debug.WriteLine($"Published Templates Found: {templates.Count}");
            System.Diagnostics.Debug.WriteLine("============================================");

            // Add "All Templates" option at the top
            templates.Insert(0, new SelectOption
            {
                Value = "",
                Text = "All Templates",
                Selected = true
            });

            return templates;
        }

        /// <summary>
        /// Get group by options for date aggregation
        /// </summary>
        public List<SelectOption> GetGroupByOptions()
        {
            return new List<SelectOption>
            {
                new SelectOption { Value = "Daily", Text = "Daily", Selected = true },
                new SelectOption { Value = "Weekly", Text = "Weekly" },
                new SelectOption { Value = "Monthly", Text = "Monthly" }
            };
        }
    }
}
