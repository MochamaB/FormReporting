using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.Entities.Forms;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Services.Identity;
using System.Security.Claims;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service implementation for generating submission statistics and reporting
    /// Handles counts, status breakdowns, on-time tracking, and completion metrics
    /// </summary>
    public class FormSubmissionStatisticsService : IFormSubmissionStatisticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IScopeService _scopeService;

        public FormSubmissionStatisticsService(ApplicationDbContext context, IScopeService scopeService)
        {
            _context = context;
            _scopeService = scopeService;
        }

        public async Task<int> GetTotalSubmissionsAsync(int templateId, DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null)
        {
            var query = _context.FormTemplateSubmissions
                .Where(s => s.TemplateId == templateId);

            query = ApplyFilters(query, startDate, endDate, tenantId);

            return await query.CountAsync();
        }

        public async Task<Dictionary<string, int>> GetSubmissionsByStatusAsync(int templateId, DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null)
        {
            var query = _context.FormTemplateSubmissions
                .Where(s => s.TemplateId == templateId);

            query = ApplyFilters(query, startDate, endDate, tenantId);

            var statusCounts = await query
                .GroupBy(s => s.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return statusCounts.ToDictionary(x => x.Status, x => x.Count);
        }

        public async Task<OnTimeStatisticsViewModel> GetOnTimeStatisticsAsync(int templateId, DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null)
        {
            var query = _context.FormTemplateSubmissions
                .Where(s => s.TemplateId == templateId);

            query = ApplyFilters(query, startDate, endDate, tenantId);

            var submissions = await query
                .Select(s => new
                {
                    s.SubmittedDate,
                    s.CreatedDate,
                    s.ReportingPeriod,
                    s.Status
                })
                .ToListAsync();

            if (!submissions.Any())
            {
                return new OnTimeStatisticsViewModel
                {
                    TotalSubmissions = 0,
                    OnTimeCount = 0,
                    LateCount = 0,
                    OnTimePercentage = 0,
                    LatePercentage = 0
                };
            }

            // Use SubmittedDate if available, otherwise use CreatedDate for Draft submissions
            var onTimeCount = submissions.Count(s => 
            {
                var effectiveDate = s.SubmittedDate ?? s.CreatedDate;
                return effectiveDate.Date <= s.ReportingPeriod.Date;
            });
            
            var lateCount = submissions.Count - onTimeCount;
            var total = submissions.Count;

            return new OnTimeStatisticsViewModel
            {
                TotalSubmissions = total,
                OnTimeCount = onTimeCount,
                LateCount = lateCount,
                OnTimePercentage = total > 0 ? (decimal)onTimeCount / total * 100 : 0,
                LatePercentage = total > 0 ? (decimal)lateCount / total * 100 : 0
            };
        }

        public async Task<double?> GetAverageCompletionTimeAsync(int templateId, DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null)
        {
            var query = _context.FormTemplateSubmissions
                .Where(s => s.TemplateId == templateId && s.SubmittedDate.HasValue);

            query = ApplyFilters(query, startDate, endDate, tenantId);

            var submissions = await query
                .Select(s => new
                {
                    s.CreatedDate,
                    s.SubmittedDate
                })
                .ToListAsync();

            if (!submissions.Any())
                return null;

            var completionTimes = submissions
                .Select(s => (s.SubmittedDate!.Value - s.CreatedDate).TotalHours)
                .ToList();

            return completionTimes.Average();
        }

        public async Task<List<SubmissionTrendDataPoint>> GetSubmissionTrendsAsync(int templateId, DateTime startDate, DateTime endDate, string groupBy = "Daily", int? tenantId = null)
        {
            var query = _context.FormTemplateSubmissions
                .Where(s => s.TemplateId == templateId);

            if (tenantId.HasValue)
            {
                query = query.Where(s => s.TenantId == tenantId.Value);
            }

            var submissions = await query
                .Select(s => new
                {
                    EffectiveDate = s.SubmittedDate ?? s.CreatedDate,
                    s.Status
                })
                .Where(s => s.EffectiveDate >= startDate && s.EffectiveDate <= endDate)
                .Select(s => s.EffectiveDate)
                .ToListAsync();

            var trendData = new List<SubmissionTrendDataPoint>();

            if (groupBy == "Daily")
            {
                var dailyGroups = submissions.GroupBy(d => d.Date)
                    .OrderBy(g => g.Key);

                foreach (var group in dailyGroups)
                {
                    trendData.Add(new SubmissionTrendDataPoint
                    {
                        Date = group.Key,
                        Count = group.Count(),
                        Label = group.Key.ToString("MMM dd, yyyy")
                    });
                }
            }
            else if (groupBy == "Weekly")
            {
                var weeklyGroups = submissions
                    .GroupBy(d => GetWeekStartDate(d))
                    .OrderBy(g => g.Key);

                foreach (var group in weeklyGroups)
                {
                    trendData.Add(new SubmissionTrendDataPoint
                    {
                        Date = group.Key,
                        Count = group.Count(),
                        Label = $"Week of {group.Key:MMM dd, yyyy}"
                    });
                }
            }
            else if (groupBy == "Monthly")
            {
                var monthlyGroups = submissions
                    .GroupBy(d => new DateTime(d.Year, d.Month, 1))
                    .OrderBy(g => g.Key);

                foreach (var group in monthlyGroups)
                {
                    trendData.Add(new SubmissionTrendDataPoint
                    {
                        Date = group.Key,
                        Count = group.Count(),
                        Label = group.Key.ToString("MMM yyyy")
                    });
                }
            }

            return trendData;
        }

        public async Task<TemplateStatisticsDashboardViewModel> GetTemplateStatisticsDashboardAsync(int templateId, DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null)
        {
            var template = await _context.FormTemplates
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

            if (template == null)
                throw new ArgumentException($"Template {templateId} not found");

            var dashboard = new TemplateStatisticsDashboardViewModel
            {
                TemplateId = templateId,
                TemplateName = template.TemplateName,
                TotalSubmissions = await GetTotalSubmissionsAsync(templateId, startDate, endDate, tenantId),
                StatusBreakdown = await GetSubmissionsByStatusAsync(templateId, startDate, endDate, tenantId),
                OnTimeStatistics = await GetOnTimeStatisticsAsync(templateId, startDate, endDate, tenantId),
                AverageCompletionTimeHours = await GetAverageCompletionTimeAsync(templateId, startDate, endDate, tenantId),
                RecentSubmissions = await GetRecentSubmissionsAsync(templateId, 10, tenantId)
            };

            if (startDate.HasValue && endDate.HasValue)
            {
                dashboard.TrendData = await GetSubmissionTrendsAsync(templateId, startDate.Value, endDate.Value, "Daily", tenantId);
            }

            return dashboard;
        }

        public async Task<List<TenantSubmissionStatistics>> GetTenantComparisonAsync(int templateId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.FormTemplateSubmissions
                .Include(s => s.Tenant)
                .Where(s => s.TemplateId == templateId && s.TenantId.HasValue);

            if (startDate.HasValue)
            {
                query = query.Where(s => s.SubmittedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(s => s.SubmittedDate <= endDate.Value);
            }

            var tenantStats = await query
                .GroupBy(s => new { s.TenantId, s.Tenant!.TenantName })
                .Select(g => new TenantSubmissionStatistics
                {
                    TenantId = g.Key.TenantId!.Value,
                    TenantName = g.Key.TenantName,
                    TotalSubmissions = g.Count(),
                    SubmittedCount = g.Count(s => s.Status == "Submitted"),
                    DraftCount = g.Count(s => s.Status == "Draft"),
                    ApprovedCount = g.Count(s => s.Status == "Approved"),
                    RejectedCount = g.Count(s => s.Status == "Rejected")
                })
                .OrderByDescending(t => t.TotalSubmissions)
                .ToListAsync();

            return tenantStats;
        }

        public async Task<List<UserSubmissionStatistics>> GetUserSubmissionRatesAsync(int templateId, DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null)
        {
            var query = _context.FormTemplateSubmissions
                .Include(s => s.Submitter)
                .Where(s => s.TemplateId == templateId);

            query = ApplyFilters(query, startDate, endDate, tenantId);

            var userStats = await query
                .GroupBy(s => new { s.SubmittedBy, s.Submitter.FirstName, s.Submitter.LastName })
                .Select(g => new UserSubmissionStatistics
                {
                    UserId = g.Key.SubmittedBy,
                    UserName = $"{g.Key.FirstName} {g.Key.LastName}",
                    TotalSubmissions = g.Count(),
                    SubmittedCount = g.Count(s => s.Status == "Submitted"),
                    DraftCount = g.Count(s => s.Status == "Draft"),
                    ApprovedCount = g.Count(s => s.Status == "Approved")
                })
                .OrderByDescending(u => u.TotalSubmissions)
                .ToListAsync();

            return userStats;
        }

        public async Task<List<SubmissionSummaryViewModel>> GetRecentSubmissionsAsync(int templateId, int count = 10, int? tenantId = null, ClaimsPrincipal currentUser = null)
        {
            var query = _context.FormTemplateSubmissions
                .Include(s => s.Submitter)
                .Include(s => s.Tenant)
                .Where(s => s.TemplateId == templateId);

            // Apply scope filtering if user provided
            if (currentUser != null)
            {
                var accessibleTenantIds = await _scopeService.GetAccessibleTenantIdsAsync(currentUser);
                query = query.Where(s => s.TenantId.HasValue && accessibleTenantIds.Contains(s.TenantId.Value));
            }

            if (tenantId.HasValue)
            {
                query = query.Where(s => s.TenantId == tenantId.Value);
            }

            var submissions = await query
                .OrderByDescending(s => s.SubmittedDate ?? s.CreatedDate)
                .Take(count)
                .Select(s => new SubmissionSummaryViewModel
                {
                    SubmissionId = s.SubmissionId,
                    Status = s.Status,
                    SubmittedBy = $"{s.Submitter.FirstName} {s.Submitter.LastName}",
                    SubmittedDate = s.SubmittedDate,
                    TenantName = s.Tenant != null ? s.Tenant.TenantName : "N/A",
                    ReportingPeriod = s.ReportingPeriod
                })
                .ToListAsync();

            return submissions;
        }

        // Scope-aware methods for multiple templates
        public async Task<List<SubmissionSummaryViewModel>> GetRecentSubmissionsAsync(List<int> templateIds, int count = 10, int? tenantId = null, ClaimsPrincipal currentUser = null)
        {
            var query = _context.FormTemplateSubmissions
                .Include(s => s.Submitter)
                .Include(s => s.Tenant)
                .Where(s => templateIds.Contains(s.TemplateId));

            // Apply scope filtering if user provided
            if (currentUser != null)
            {
                var accessibleTenantIds = await _scopeService.GetAccessibleTenantIdsAsync(currentUser);
                query = query.Where(s => s.TenantId.HasValue && accessibleTenantIds.Contains(s.TenantId.Value));
            }

            if (tenantId.HasValue)
            {
                query = query.Where(s => s.TenantId == tenantId.Value);
            }

            var submissions = await query
                .OrderByDescending(s => s.SubmittedDate ?? s.CreatedDate)
                .Take(count)
                .Select(s => new SubmissionSummaryViewModel
                {
                    SubmissionId = s.SubmissionId,
                    Status = s.Status,
                    SubmittedBy = $"{s.Submitter.FirstName} {s.Submitter.LastName}",
                    SubmittedDate = s.SubmittedDate,
                    TenantName = s.Tenant != null ? s.Tenant.TenantName : "N/A",
                    ReportingPeriod = s.ReportingPeriod
                })
                .ToListAsync();

            return submissions;
        }

        public async Task<Dictionary<string, int>> GetSubmissionsByStatusAsync(List<int> templateIds, DateTime? startDate = null, DateTime? endDate = null, int? tenantId = null, ClaimsPrincipal currentUser = null)
        {
            var query = _context.FormTemplateSubmissions
                .Where(s => templateIds.Contains(s.TemplateId));

            // Apply scope filtering if user provided
            if (currentUser != null)
            {
                var accessibleTenantIds = await _scopeService.GetAccessibleTenantIdsAsync(currentUser);
                query = query.Where(s => s.TenantId.HasValue && accessibleTenantIds.Contains(s.TenantId.Value));
            }

            query = ApplyFilters(query, startDate, endDate, tenantId);

            var statusCounts = await query
                .GroupBy(s => s.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return statusCounts.ToDictionary(x => x.Status, x => x.Count);
        }

        public async Task<List<SubmissionTrendDataPoint>> GetSubmissionTrendsAsync(List<int> templateIds, DateTime startDate, DateTime endDate, string groupBy = "Daily", int? tenantId = null, ClaimsPrincipal currentUser = null)
        {
            var query = _context.FormTemplateSubmissions
                .Where(s => templateIds.Contains(s.TemplateId));

            // Apply scope filtering if user provided
            if (currentUser != null)
            {
                var accessibleTenantIds = await _scopeService.GetAccessibleTenantIdsAsync(currentUser);
                query = query.Where(s => s.TenantId.HasValue && accessibleTenantIds.Contains(s.TenantId.Value));
            }

            query = query.Where(s => s.CreatedDate >= startDate && s.CreatedDate <= endDate);

            if (tenantId.HasValue)
            {
                query = query.Where(s => s.TenantId == tenantId.Value);
            }

            // Group by specified period and project to trend data
            var trendData = await (groupBy.ToLower() switch
            {
                "weekly" => query
                    .GroupBy(s => EF.Functions.DateDiffDay(startDate, s.CreatedDate) / 7)
                    .Select(g => new { Week = g.Key, Count = g.Count() })
                    .Select(x => new SubmissionTrendDataPoint 
                    { 
                        Label = $"Week {x.Week + 1}", 
                        Count = x.Count,
                        Date = startDate.AddDays(x.Week * 7)
                    }),
                "monthly" => query
                    .GroupBy(s => new { s.CreatedDate.Year, s.CreatedDate.Month })
                    .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                    .Select(x => new SubmissionTrendDataPoint 
                    { 
                        Label = $"{x.Year}-{x.Month:D2}", 
                        Count = x.Count,
                        Date = new DateTime(x.Year, x.Month, 1)
                    }),
                "quarterly" => query
                    .GroupBy(s => (s.CreatedDate.Month - 1) / 3)
                    .Select(g => new { Quarter = g.Key, Year = g.First().CreatedDate.Year, Count = g.Count() })
                    .Select(x => new SubmissionTrendDataPoint 
                    { 
                        Label = $"Q{x.Quarter + 1}-{x.Year}", 
                        Count = x.Count,
                        Date = new DateTime(x.Year, (x.Quarter * 3) + 1, 1)
                    }),
                _ => query
                    .GroupBy(s => s.CreatedDate.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .Select(x => new SubmissionTrendDataPoint 
                    { 
                        Label = x.Date.ToString("yyyy-MM-dd"), 
                        Count = x.Count,
                        Date = x.Date
                    })
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

            return trendData;
        }

        private IQueryable<FormTemplateSubmission> ApplyFilters(IQueryable<FormTemplateSubmission> query, DateTime? startDate, DateTime? endDate, int? tenantId)
        {
            if (startDate.HasValue)
            {
                query = query.Where(s => s.SubmittedDate >= startDate.Value || (s.SubmittedDate == null && s.CreatedDate >= startDate.Value));
            }

            if (endDate.HasValue)
            {
                query = query.Where(s => s.SubmittedDate <= endDate.Value || (s.SubmittedDate == null && s.CreatedDate <= endDate.Value));
            }

            if (tenantId.HasValue)
            {
                query = query.Where(s => s.TenantId == tenantId.Value);
            }

            return query;
        }

        private DateTime GetWeekStartDate(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
    }
}
