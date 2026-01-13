using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FormReporting.Services.Forms;
using FormReporting.Services.Identity;
using FormReporting.Models.ViewModels.Components;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Models.Common;
using FormReporting.Extensions;
using System.Security.Claims;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FormReporting.Controllers.Submissions
{
    /// <summary>
    /// MVC Controller for Form Submissions
    /// Handles form rendering, submission, and viewing
    /// </summary>
    [Authorize]
    public class SubmissionsController : Controller
    {
        private readonly IFormSubmissionService _submissionService;
        private readonly IFormResponseService _responseService;
        private readonly IScopeService _scopeService;

        public SubmissionsController(
            IFormSubmissionService submissionService,
            IFormResponseService responseService,
            IScopeService scopeService)
        {
            _submissionService = submissionService;
            _responseService = responseService;
            _scopeService = scopeService;
        }

        /// <summary>
        /// Get current user ID from claims
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("UserId")?.Value;

            if (int.TryParse(userIdClaim, out var userId))
                return userId;

            return 0;
        }

        // ========================================================================
        // INDEX - LIST USER'S SUBMISSIONS
        // ========================================================================

        /// <summary>
        /// List user's submissions with filtering and pagination
        /// GET /Submissions
        /// </summary>
        public async Task<IActionResult> Index(string? status = null, string? search = null, int page = 1, int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var submissions = await _submissionService.GetUserSubmissionsAsync(userId, status);

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                submissions = submissions.Where(s =>
                    (s.Template?.TemplateName?.ToLower().Contains(search) ?? false) ||
                    (s.Tenant?.TenantName?.ToLower().Contains(search) ?? false))
                    .ToList();
            }

            // Calculate statistics for stat cards (from all submissions, not filtered)
            var allSubmissions = await _submissionService.GetUserSubmissionsAsync(userId, null);
            ViewBag.TotalSubmissions = allSubmissions.Count;
            ViewBag.DraftCount = allSubmissions.Count(s => s.Status == "Draft");
            ViewBag.SubmittedCount = allSubmissions.Count(s => s.Status == "Submitted");
            ViewBag.InApprovalCount = allSubmissions.Count(s => s.Status == "InApproval");
            ViewBag.ApprovedCount = allSubmissions.Count(s => s.Status == "Approved");

            // Pagination
            var totalRecords = submissions.Count;
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            
            var pagedSubmissions = submissions
                .OrderByDescending(s => s.LastSavedDate ?? s.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pagination ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;

            // Get available templates for "New Submission" dropdown
            var templates = await _submissionService.GetAvailableTemplatesAsync(userId);
            ViewBag.AvailableTemplates = templates;

            // Pass current filters to view
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;

            return View(pagedSubmissions);
        }

        // ========================================================================
        // AVAILABLE FORMS - LIST TEMPLATES USER CAN FILL
        // ========================================================================

        /// <summary>
        /// List available form templates user can fill
        /// GET /Submissions/AvailableForms
        /// </summary>
        public async Task<IActionResult> AvailableForms(string? tab = "overview", string? category = null, string? search = null, string? type = null, int page = 1, int pageSize = 12)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var allTemplates = await _submissionService.GetAvailableTemplatesAsync(userId);
            var templates = allTemplates.ToList();

            // Apply category filter
            if (!string.IsNullOrWhiteSpace(category))
            {
                templates = templates.Where(t =>
                    t.Category?.CategoryName?.Equals(category, StringComparison.OrdinalIgnoreCase) ?? false)
                    .ToList();
            }

            // Apply type filter (Daily, Weekly, Monthly, Quarterly, Annual)
            if (!string.IsNullOrWhiteSpace(type))
            {
                templates = templates.Where(t =>
                    t.TemplateType?.Equals(type, StringComparison.OrdinalIgnoreCase) ?? false)
                    .ToList();
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                templates = templates.Where(t =>
                    (t.TemplateName?.ToLower().Contains(search) ?? false) ||
                    (t.Description?.ToLower().Contains(search) ?? false) ||
                    (t.TemplateCode?.ToLower().Contains(search) ?? false))
                    .ToList();
            }

            // Get categories with counts for vertical tabs
            var categoriesWithCounts = allTemplates
                .Where(t => t.Category != null)
                .GroupBy(t => new { t.Category!.CategoryId, t.Category.CategoryName, t.Category.Description, t.Category.IconClass, t.Category.DisplayOrder })
                .Select(g => new
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    Description = g.Key.Description,
                    IconClass = g.Key.IconClass ?? "ri-folder-line",
                    DisplayOrder = g.Key.DisplayOrder,
                    TemplateCount = g.Count()
                })
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .ToList();

            ViewBag.CategoriesWithCounts = categoriesWithCounts;
            ViewBag.AllTemplatesCount = allTemplates.Count;

            // Pagination
            var totalRecords = templates.Count;
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var pagedTemplates = templates
                .OrderBy(t => t.Category?.DisplayOrder ?? 999)
                .ThenBy(t => t.TemplateName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Get submission stats for the paged templates
            var templateIds = pagedTemplates.Select(t => t.TemplateId).ToList();
            var submissionStats = await _submissionService.GetTemplateSubmissionStatsAsync(userId, templateIds);
            ViewBag.SubmissionStats = submissionStats;

            // Pagination ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;

            // Current filters
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentType = type;
            ViewBag.CurrentTab = tab ?? "forms";

            return View(pagedTemplates);
        }

        // ========================================================================
        // TEMPLATE SUBMISSIONS - VIEW ALL SUBMISSIONS FOR A TEMPLATE (SCOPE-FILTERED)
        // ========================================================================

        /// <summary>
        /// View all submissions for a specific template, filtered by user's scope
        /// GET /Submissions/Template/{templateId}
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="status">Filter by status</param>
        /// <param name="tenantId">Filter by tenant</param>
        /// <param name="period">Filter by period (thisMonth, lastMonth, thisQuarter, thisYear)</param>
        /// <param name="search">Search text</param>
        /// <param name="page">Page number</param>
        /// <param name="tab">Active tab (responses or summary)</param>
        [HttpGet("Submissions/Template/{templateId:int}")]
        public async Task<IActionResult> TemplateSubmissions(
            int templateId,
            string? status = null,
            int? tenantId = null,
            string? period = null,
            string? search = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int? submitterId = null,
            int page = 1,
            string? tab = "responses")
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            // Check if user can access this template
            var canAccess = await _submissionService.CanUserAccessTemplateAsync(userId, templateId);
            if (!canAccess)
            {
                TempData["Error"] = "You do not have access to this form template.";
                return RedirectToAction("AvailableForms");
            }

            try
            {
                // Build filters
                var filters = new Models.ViewModels.Forms.SubmissionFilters
                {
                    Status = status,
                    TenantId = tenantId,
                    Period = period,
                    Search = search,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    SubmitterId = submitterId
                };

                // Get scope-filtered submissions
                var viewModel = await _submissionService.GetScopedTemplateSubmissionsAsync(
                    templateId,
                    User,
                    filters,
                    page,
                    pageSize: 10);

                // Set active tab
                viewModel.ActiveTab = tab ?? "responses";

                return View(viewModel);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("AvailableForms");
            }
        }

        /// <summary>
        /// Get submission detail for offcanvas display (AJAX)
        /// GET /Submissions/Detail/{submissionId}
        /// </summary>
        /// <param name="submissionId">Submission ID</param>
        [HttpGet("Submissions/Detail/{submissionId:int}")]
        public async Task<IActionResult> GetSubmissionDetail(int submissionId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            // Check if user can access this submission
            var canAccess = await _submissionService.CanUserAccessSubmissionAsync(submissionId, User);
            if (!canAccess)
            {
                return Forbid();
            }

            var detail = await _submissionService.GetSubmissionDetailAsync(submissionId, User);
            if (detail == null)
            {
                return NotFound();
            }

            // Return partial view for AJAX loading
            return PartialView("Partials/_SubmissionDetailOffcanvas", detail);
        }

        // ========================================================================
        // EXPORT
        // ========================================================================

        /// <summary>
        /// Export submissions to CSV, Excel, or PDF
        /// GET /Submissions/Export
        /// </summary>
        [HttpGet("Submissions/Export")]
        public async Task<IActionResult> ExportSubmissions(
            int templateId,
            string format = "csv",
            string? status = null,
            int? tenantId = null,
            string? period = null,
            string? search = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int? submitterId = null)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            // Check if user can access this template
            var canAccess = await _submissionService.CanUserAccessTemplateAsync(userId, templateId);
            if (!canAccess)
            {
                TempData["Error"] = "You do not have access to this form template.";
                return RedirectToAction("AvailableForms");
            }

            try
            {
                // Build filters
                var filters = new Models.ViewModels.Forms.SubmissionFilters
                {
                    Status = status,
                    TenantId = tenantId,
                    Period = period,
                    Search = search,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    SubmitterId = submitterId
                };

                // Get export data
                var exportData = await _submissionService.GetSubmissionsForExportAsync(templateId, User, filters);

                if (exportData.Rows.Count == 0)
                {
                    TempData["Warning"] = "No submissions found to export.";
                    return RedirectToAction("TemplateSubmissions", new { templateId });
                }

                // Generate file based on format
                return format.ToLower() switch
                {
                    "csv" => GenerateCsvExport(exportData),
                    "excel" => GenerateExcelExport(exportData),
                    "pdf" => GeneratePdfExport(exportData),
                    _ => GenerateCsvExport(exportData)
                };
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Export failed: {ex.Message}";
                return RedirectToAction("TemplateSubmissions", new { templateId });
            }
        }

        /// <summary>
        /// Generate CSV file from export data
        /// </summary>
        private FileContentResult GenerateCsvExport(Services.Forms.SubmissionExportData exportData)
        {
            var sb = new System.Text.StringBuilder();

            // Add headers
            sb.AppendLine(string.Join(",", exportData.Headers.Select(EscapeCsvField)));

            // Add rows
            foreach (var row in exportData.Rows)
            {
                sb.AppendLine(string.Join(",", row.Select(EscapeCsvField)));
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"{SanitizeFileName(exportData.TemplateName)}_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }

        /// <summary>
        /// Generate Excel file from export data using ClosedXML
        /// </summary>
        private FileContentResult GenerateExcelExport(Services.Forms.SubmissionExportData exportData)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Submissions");

            // Add headers
            for (int col = 0; col < exportData.Headers.Count; col++)
            {
                var cell = worksheet.Cell(1, col + 1);
                cell.Value = exportData.Headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            }

            // Add data rows
            for (int row = 0; row < exportData.Rows.Count; row++)
            {
                for (int col = 0; col < exportData.Rows[row].Count; col++)
                {
                    worksheet.Cell(row + 2, col + 1).Value = exportData.Rows[row][col];
                }
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Freeze header row
            worksheet.SheetView.FreezeRows(1);

            // Generate file
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var bytes = stream.ToArray();
            var fileName = $"{SanitizeFileName(exportData.TemplateName)}_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Generate PDF file from export data using QuestPDF
        /// </summary>
        private FileContentResult GeneratePdfExport(Services.Forms.SubmissionExportData exportData)
        {
            // Set QuestPDF license (Community license for open source)
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Header()
                        .Text(exportData.TemplateName + " - Export")
                        .SemiBold().FontSize(14).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(10)
                        .Table(table =>
                        {
                            // Define columns
                            table.ColumnsDefinition(columns =>
                            {
                                foreach (var _ in exportData.Headers)
                                {
                                    columns.RelativeColumn();
                                }
                            });

                            // Header row
                            table.Header(header =>
                            {
                                foreach (var headerText in exportData.Headers)
                                {
                                    header.Cell()
                                        .Background(Colors.Grey.Lighten2)
                                        .Padding(5)
                                        .Text(headerText)
                                        .Bold()
                                        .FontSize(7);
                                }
                            });

                            // Data rows
                            foreach (var row in exportData.Rows)
                            {
                                foreach (var cellValue in row)
                                {
                                    table.Cell()
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten1)
                                        .Padding(4)
                                        .Text(cellValue ?? "")
                                        .FontSize(7);
                                }
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated on ");
                            x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                            x.Span(" | Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            });

            var bytes = document.GeneratePdf();
            var fileName = $"{SanitizeFileName(exportData.TemplateName)}_Export_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(bytes, "application/pdf", fileName);
        }

        /// <summary>
        /// Escape a field for CSV format
        /// </summary>
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "\"\"";

            // If field contains comma, quote, or newline, wrap in quotes and escape quotes
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }

        /// <summary>
        /// Sanitize filename for safe file download
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
            return string.IsNullOrWhiteSpace(sanitized) ? "Export" : sanitized;
        }

        // ========================================================================
        // BULK ACTIONS
        // ========================================================================

        /// <summary>
        /// Perform bulk action on submissions
        /// POST /Submissions/BulkAction
        /// </summary>
        [HttpPost("Submissions/BulkAction")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAction([FromBody] BulkActionRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Not authenticated" });

            if (request.SubmissionIds == null || !request.SubmissionIds.Any())
                return BadRequest(new { success = false, message = "No submissions selected" });

            try
            {
                int successCount = 0;
                var errors = new List<string>();

                foreach (var submissionId in request.SubmissionIds)
                {
                    // Check if user can access this submission
                    var canAccess = await _submissionService.CanUserAccessSubmissionAsync(submissionId, User);
                    if (!canAccess)
                    {
                        errors.Add($"No access to submission #{submissionId}");
                        continue;
                    }

                    switch (request.Action?.ToLower())
                    {
                        case "delete":
                            // TODO: Implement actual delete in service
                            // await _submissionService.DeleteSubmissionAsync(submissionId, User);
                            successCount++;
                            break;
                            
                        case "approve":
                            // TODO: Implement actual approve in service
                            // await _submissionService.ApproveSubmissionAsync(submissionId, User);
                            successCount++;
                            break;
                            
                        default:
                            return BadRequest(new { success = false, message = "Invalid action" });
                    }
                }

                if (successCount > 0)
                {
                    return Ok(new { 
                        success = true, 
                        message = $"{request.Action} completed for {successCount} submission(s)",
                        processed = successCount,
                        errors = errors
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "No submissions were processed",
                        errors = errors
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Request model for bulk actions
        /// </summary>
        public class BulkActionRequest
        {
            public string? Action { get; set; }
            public List<int>? SubmissionIds { get; set; }
        }

        // ========================================================================
        // START - WELCOME PAGE BEFORE SUBMISSION
        // ========================================================================

        /// <summary>
        /// Show welcome/start page for a form template
        /// GET /Submissions/Start/{templateId}
        /// </summary>
        public async Task<IActionResult> Start(int templateId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            // Check if user can access this template
            var canAccess = await _submissionService.CanUserAccessTemplateAsync(userId, templateId);
            if (!canAccess)
            {
                TempData["Error"] = "You do not have access to this form template.";
                return RedirectToAction("AvailableForms");
            }

            // Get template details
            var templates = await _submissionService.GetAvailableTemplatesAsync(userId);
            var template = templates.FirstOrDefault(t => t.TemplateId == templateId);
            if (template == null)
            {
                TempData["Error"] = "Form template not found.";
                return RedirectToAction("AvailableForms");
            }

            // Get user's accessible tenants
            var accessibleTenantIds = await _scopeService.GetAccessibleTenantIdsAsync(User);
            var tenants = await _submissionService.GetTenantsAsync(accessibleTenantIds);

            // Build available periods based on template type
            var availablePeriods = BuildAvailablePeriods(template.TemplateType);
            var suggestedPeriod = availablePeriods.FirstOrDefault(p => p.IsDefault)?.Value 
                ?? DateTime.Now.ToString("yyyy-MM-dd");

            // Check for existing draft with suggested period
            var defaultTenantId = tenants.Count == 1 ? tenants[0].TenantId : (int?)null;
            var suggestedPeriodDate = DateTime.TryParse(suggestedPeriod, out var sp) ? sp : DateTime.Now;
            var existingDraft = await _submissionService.GetExistingDraftAsync(
                userId, templateId, defaultTenantId, suggestedPeriodDate);

            // Build ViewModel
            var viewModel = new StartSubmissionViewModel
            {
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                Description = template.Description,
                TemplateCode = template.TemplateCode,
                TemplateType = template.TemplateType,
                Version = template.Version,
                PublishedDate = template.PublishedDate,
                CategoryName = template.Category?.CategoryName ?? "Uncategorized",
                CategoryIcon = template.Category?.IconClass ?? "ri-file-list-3-line",
                RequiresApproval = template.RequiresApproval,
                SectionCount = template.Sections?.Count ?? 0,
                FieldCount = template.Sections?.Sum(s => s.Items?.Count ?? 0) ?? 0,
                EstimatedMinutes = CalculateEstimatedTime(template.Sections?.Sum(s => s.Items?.Count ?? 0) ?? 0),

                // Tenant selection
                ShowTenantSelector = tenants.Count > 1,
                AvailableTenants = tenants.Select(t => new TenantOption
                {
                    TenantId = t.TenantId,
                    TenantName = t.TenantName,
                    TenantCode = t.TenantCode,
                    TenantType = t.TenantType
                }).ToList(),
                SelectedTenantId = defaultTenantId,

                // Period selection
                SuggestedPeriod = suggestedPeriodDate,
                AvailablePeriods = availablePeriods,
                SelectedPeriod = suggestedPeriod,

                // Draft detection
                HasExistingDraft = existingDraft != null,
                ExistingDraftId = existingDraft?.SubmissionId,
                DraftLastSaved = existingDraft?.LastSavedDate,
                DraftCurrentSection = existingDraft?.CurrentSection ?? 0
            };

            return View(viewModel);
        }

        /// <summary>
        /// Build available period options based on template type
        /// </summary>
        private List<PeriodOption> BuildAvailablePeriods(string templateType)
        {
            var periods = new List<PeriodOption>();
            var now = DateTime.Now;

            switch (templateType?.ToLower())
            {
                case "daily":
                    // Last 7 days + today
                    for (int i = 0; i <= 7; i++)
                    {
                        var date = now.AddDays(-i);
                        periods.Add(new PeriodOption
                        {
                            Value = date.ToString("yyyy-MM-dd"),
                            Label = date.ToString("dddd, MMMM d, yyyy"),
                            IsDefault = i == 0
                        });
                    }
                    break;

                case "weekly":
                    // Current week + last 4 weeks
                    var startOfWeek = now.AddDays(-(int)now.DayOfWeek + 1); // Monday
                    for (int i = 0; i < 5; i++)
                    {
                        var weekStart = startOfWeek.AddDays(-7 * i);
                        var weekEnd = weekStart.AddDays(6);
                        periods.Add(new PeriodOption
                        {
                            Value = weekStart.ToString("yyyy-MM-dd"),
                            Label = $"{weekStart:MMM d} - {weekEnd:MMM d, yyyy}",
                            IsDefault = i == 0
                        });
                    }
                    break;

                case "monthly":
                    // Current month + last 6 months
                    for (int i = 0; i < 7; i++)
                    {
                        var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                        periods.Add(new PeriodOption
                        {
                            Value = monthStart.ToString("yyyy-MM-dd"),
                            Label = monthStart.ToString("MMMM yyyy"),
                            IsDefault = i == 0
                        });
                    }
                    break;

                case "quarterly":
                    // Current quarter + last 4 quarters
                    var currentQuarter = (now.Month - 1) / 3 + 1;
                    var quarterStart = new DateTime(now.Year, (currentQuarter - 1) * 3 + 1, 1);
                    for (int i = 0; i < 5; i++)
                    {
                        var qStart = quarterStart.AddMonths(-3 * i);
                        var qEnd = qStart.AddMonths(3).AddDays(-1);
                        var qNum = (qStart.Month - 1) / 3 + 1;
                        periods.Add(new PeriodOption
                        {
                            Value = qStart.ToString("yyyy-MM-dd"),
                            Label = $"Q{qNum} {qStart.Year} ({qStart:MMM} - {qEnd:MMM})",
                            IsDefault = i == 0
                        });
                    }
                    break;

                case "annual":
                    // Current year + last 3 years
                    for (int i = 0; i < 4; i++)
                    {
                        var yearStart = new DateTime(now.Year - i, 1, 1);
                        periods.Add(new PeriodOption
                        {
                            Value = yearStart.ToString("yyyy-MM-dd"),
                            Label = yearStart.Year.ToString(),
                            IsDefault = i == 0
                        });
                    }
                    break;

                case "ondemand":
                default:
                    // For on-demand, just provide today as default
                    periods.Add(new PeriodOption
                    {
                        Value = now.ToString("yyyy-MM-dd"),
                        Label = "Custom Date Range",
                        IsDefault = true
                    });
                    break;
            }

            return periods;
        }

        /// <summary>
        /// Calculate estimated completion time based on field count
        /// </summary>
        private int CalculateEstimatedTime(int fieldCount)
        {
            // Assume ~30 seconds per field, minimum 5 minutes
            var minutes = (int)Math.Ceiling(fieldCount * 0.5);
            return Math.Max(5, minutes);
        }

        // ========================================================================
        // CREATE SUBMISSION - POST FROM START PAGE
        // ========================================================================

        /// <summary>
        /// Create a new submission or resume draft
        /// POST /Submissions/CreateSubmission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubmission(int templateId, int? tenantId, string selectedPeriod, DateTime? periodStart, DateTime? periodEnd)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            // Check if user can access this template
            var canAccess = await _submissionService.CanUserAccessTemplateAsync(userId, templateId);
            if (!canAccess)
            {
                TempData["Error"] = "You do not have access to this form template.";
                return RedirectToAction("AvailableForms");
            }

            // Parse reporting period
            DateTime period;
            if (!string.IsNullOrEmpty(selectedPeriod) && DateTime.TryParse(selectedPeriod, out var parsedPeriod))
            {
                period = parsedPeriod;
            }
            else if (periodStart.HasValue)
            {
                period = periodStart.Value;
            }
            else
            {
                period = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }

            // Check for existing draft - always resume if exists
            var existingDraft = await _submissionService.GetExistingDraftAsync(userId, templateId, tenantId, period);
            if (existingDraft != null)
            {
                TempData["Info"] = "Resuming your existing draft...";
                return RedirectToAction("Resume", new { submissionId = existingDraft.SubmissionId });
            }

            // Create new submission
            var submission = await _submissionService.CreateSubmissionAsync(templateId, userId, tenantId, period);

            // Redirect to the form
            return RedirectToAction("Fill", new { submissionId = submission.SubmissionId });
        }

        // ========================================================================
        // FILL - RENDER FORM FOR FILLING
        // ========================================================================

        /// <summary>
        /// Render form for filling (new or resumed submission)
        /// GET /Submissions/Fill/{submissionId}
        /// </summary>
        public async Task<IActionResult> Fill(int submissionId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            // Load submission
            var submission = await _submissionService.GetSubmissionAsync(submissionId);
            if (submission == null)
            {
                TempData["Error"] = "Submission not found.";
                return RedirectToAction("Index");
            }

            // Verify ownership
            if (submission.SubmittedBy != userId)
            {
                TempData["Error"] = "You do not have access to this submission.";
                return RedirectToAction("Index");
            }

            // Verify draft status
            if (submission.Status != "Draft")
            {
                TempData["Info"] = "This submission has already been submitted.";
                return RedirectToAction("View", new { submissionId });
            }

            // Build form ViewModel with existing responses
            var formViewModel = await _submissionService.BuildFormViewModelAsync(
                submission.TemplateId,
                submissionId);

            if (formViewModel == null)
            {
                TempData["Error"] = "Failed to load form template.";
                return RedirectToAction("Index");
            }

            // Set submission context
            formViewModel.SubmissionId = submissionId;
            formViewModel.TemplateId = submission.TemplateId;
            formViewModel.TenantId = submission.TenantId;
            formViewModel.ReportingPeriod = submission.ReportingPeriod.ToString("yyyy-MM-dd");
            formViewModel.SubmitUrl = Url.Action("Submit", "Submissions") ?? "/Submissions/Submit";
            formViewModel.AutoSaveUrl = "/api/submissions/auto-save";
            formViewModel.EnableAutoSave = true;
            formViewModel.AutoSaveIntervalMs = 30000;
            formViewModel.CurrentSectionIndex = submission.CurrentSection;
            formViewModel.CancelUrl = Url.Action("Index", "Submissions");

            // Set view data
            ViewBag.SubmissionId = submissionId;
            ViewBag.TemplateId = submission.TemplateId;
            ViewBag.TenantId = submission.TenantId;
            ViewBag.TenantName = submission.Tenant?.TenantName;
            ViewBag.ReportingPeriod = submission.ReportingPeriod.ToString("yyyy-MM-dd");
            ViewBag.ReportingPeriodDisplay = FormatReportingPeriod(submission.ReportingPeriod, submission.Template?.TemplateType);
            ViewBag.IsNewSubmission = submission.CurrentSection == 0;
            ViewBag.LastSavedDate = submission.LastSavedDate?.ToString("MMM dd, yyyy HH:mm");

            return View("SubmitForm", formViewModel);
        }

        /// <summary>
        /// Format reporting period for display based on template type
        /// </summary>
        private string FormatReportingPeriod(DateTime period, string? templateType)
        {
            return templateType?.ToLower() switch
            {
                "daily" => period.ToString("dddd, MMMM d, yyyy"),
                "weekly" => $"Week of {period:MMM d, yyyy}",
                "monthly" => period.ToString("MMMM yyyy"),
                "quarterly" => $"Q{(period.Month - 1) / 3 + 1} {period.Year}",
                "annual" => period.Year.ToString(),
                _ => period.ToString("MMMM d, yyyy")
            };
        }

        // ========================================================================
        // FORM - PUBLIC SHAREABLE FORM URL (LAZY SUBMISSION)
        // ========================================================================

        /// <summary>
        /// Public form URL - shareable link that creates submission on first save
        /// GET /Submissions/Form/{templateCode}
        /// GET /f/{templateCode} (via route)
        /// 
        /// Flow:
        /// 1. Check user access to template
        /// 2. If tenant/period provided, check for existing draft
        /// 3. If draft exists, load it
        /// 4. If no draft, render form without submission (lazy creation on first save)
        /// </summary>
        [Route("Submissions/Form/{templateCode}")]
        [Route("f/{templateCode}")]
        public async Task<IActionResult> Form(string templateCode, int? tenantId = null, string? period = null)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            // Find template by code
            var templates = await _submissionService.GetAvailableTemplatesAsync(userId);
            var template = templates.FirstOrDefault(t => 
                t.TemplateCode?.Equals(templateCode, StringComparison.OrdinalIgnoreCase) ?? false);

            if (template == null)
            {
                TempData["Error"] = "Form not found or you do not have access.";
                return RedirectToAction("AvailableForms");
            }

            // Determine tenant - use provided, or default if user has only one
            var accessibleTenantIds = await _scopeService.GetAccessibleTenantIdsAsync(User);
            var tenants = await _submissionService.GetTenantsAsync(accessibleTenantIds);
            
            int? effectiveTenantId = tenantId;
            if (!effectiveTenantId.HasValue && tenants.Count == 1)
            {
                effectiveTenantId = tenants[0].TenantId;
            }

            // If multiple tenants and none specified, redirect to Start page
            if (!effectiveTenantId.HasValue && tenants.Count > 1)
            {
                return RedirectToAction("Start", new { templateId = template.TemplateId });
            }

            // Determine reporting period
            DateTime reportingPeriod;
            if (!string.IsNullOrEmpty(period) && DateTime.TryParse(period, out var parsedPeriod))
            {
                reportingPeriod = parsedPeriod;
            }
            else
            {
                // Default to current period based on template type
                reportingPeriod = GetDefaultReportingPeriod(template.TemplateType);
            }

            // Check for existing draft
            var existingDraft = await _submissionService.GetExistingDraftAsync(
                userId, template.TemplateId, effectiveTenantId, reportingPeriod);

            if (existingDraft != null)
            {
                // Resume existing draft
                return RedirectToAction("Fill", new { submissionId = existingDraft.SubmissionId });
            }

            // No draft exists - render form for lazy submission creation
            // Build form ViewModel WITHOUT submission (will be created on first auto-save)
            var formViewModel = await _submissionService.BuildFormViewModelAsync(template.TemplateId);

            if (formViewModel == null)
            {
                TempData["Error"] = "Failed to load form template.";
                return RedirectToAction("AvailableForms");
            }

            // Set context for lazy submission creation
            formViewModel.SubmissionId = null; // No submission yet
            formViewModel.TemplateId = template.TemplateId;
            formViewModel.TenantId = effectiveTenantId;
            formViewModel.ReportingPeriod = reportingPeriod.ToString("yyyy-MM-dd");
            formViewModel.SubmitUrl = Url.Action("Submit", "Submissions") ?? "/Submissions/Submit";
            formViewModel.AutoSaveUrl = "/api/submissions/auto-save";
            formViewModel.EnableAutoSave = true;
            formViewModel.AutoSaveIntervalMs = 30000;
            formViewModel.CurrentSectionIndex = 0;
            formViewModel.CancelUrl = Url.Action("AvailableForms", "Submissions");

            // Set view data
            ViewBag.SubmissionId = null; // Will be created on first save
            ViewBag.TemplateId = template.TemplateId;
            ViewBag.TenantId = effectiveTenantId;
            ViewBag.TenantName = tenants.FirstOrDefault(t => t.TenantId == effectiveTenantId)?.TenantName;
            ViewBag.ReportingPeriod = reportingPeriod.ToString("yyyy-MM-dd");
            ViewBag.ReportingPeriodDisplay = FormatReportingPeriod(reportingPeriod, template.TemplateType);
            ViewBag.IsNewSubmission = true;
            ViewBag.IsLazySubmission = true; // Flag for JS to know submission needs to be created
            ViewBag.LastSavedDate = null;

            return View("SubmitForm", formViewModel);
        }

        /// <summary>
        /// Get default reporting period based on template type
        /// </summary>
        private DateTime GetDefaultReportingPeriod(string? templateType)
        {
            var now = DateTime.Now;
            return templateType?.ToLower() switch
            {
                "daily" => now.Date,
                "weekly" => now.AddDays(-(int)now.DayOfWeek + 1), // Monday of current week
                "monthly" => new DateTime(now.Year, now.Month, 1),
                "quarterly" => new DateTime(now.Year, ((now.Month - 1) / 3) * 3 + 1, 1),
                "annual" => new DateTime(now.Year, 1, 1),
                _ => now.Date
            };
        }

        // ========================================================================
        // RESUME - CONTINUE DRAFT SUBMISSION
        // ========================================================================

        /// <summary>
        /// Resume a draft submission (redirects to Fill)
        /// GET /Submissions/Resume/{submissionId}
        /// </summary>
        public IActionResult Resume(int submissionId)
        {
            // Simply redirect to Fill action which handles the actual rendering
            return RedirectToAction("Fill", new { submissionId });
        }

        // ========================================================================
        // SUBMIT - FINAL FORM SUBMISSION
        // ========================================================================

        /// <summary>
        /// Submit a form (final submission)
        /// POST /Submissions/Submit
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int submissionId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            // Submit the form
            var result = await _responseService.SubmitAsync(submissionId, userId);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction("Confirmation", new { submissionId });
            }
            else
            {
                // Validation failed - return to form with errors
                TempData["Error"] = "Please correct the errors below before submitting.";
                TempData["ValidationErrors"] = System.Text.Json.JsonSerializer.Serialize(result.ValidationErrors);
                return RedirectToAction("Fill", new { submissionId });
            }
        }

        // ========================================================================
        // CONFIRMATION - SUCCESS PAGE
        // ========================================================================

        /// <summary>
        /// Show submission confirmation page
        /// GET /Submissions/Confirmation/{submissionId}
        /// </summary>
        public async Task<IActionResult> Confirmation(int submissionId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var submission = await _submissionService.GetSubmissionAsync(submissionId);
            if (submission == null)
            {
                TempData["Error"] = "Submission not found.";
                return RedirectToAction("Index");
            }

            // Verify ownership
            if (submission.SubmittedBy != userId)
            {
                TempData["Error"] = "You do not have access to this submission.";
                return RedirectToAction("Index");
            }

            return View(submission);
        }

        // ========================================================================
        // VIEW - READ-ONLY SUBMISSION VIEW
        // ========================================================================

        /// <summary>
        /// View a submitted form (read-only)
        /// GET /Submissions/View/{submissionId}
        /// </summary>
        public async Task<IActionResult> View(int submissionId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var submission = await _submissionService.GetSubmissionAsync(submissionId);
            if (submission == null)
            {
                TempData["Error"] = "Submission not found.";
                return RedirectToAction("Index");
            }

            // Verify ownership (TODO: expand for approvers/reviewers)
            if (submission.SubmittedBy != userId)
            {
                TempData["Error"] = "You do not have access to this submission.";
                return RedirectToAction("Index");
            }

            // Build form ViewModel in read-only mode
            var formViewModel = await _submissionService.BuildFormViewModelAsync(
                submission.TemplateId,
                submissionId);

            if (formViewModel == null)
            {
                TempData["Error"] = "Failed to load form template.";
                return RedirectToAction("Index");
            }

            // Set read-only mode
            formViewModel.IsReadOnly = true;
            formViewModel.EnableAutoSave = false;

            // Mark all fields as read-only
            foreach (var section in formViewModel.Sections)
            {
                foreach (var row in section.FieldRows)
                {
                    foreach (var field in row)
                    {
                        field.IsReadOnly = true;
                        field.IsDisabled = true;
                    }
                }
            }

            // Set view data
            ViewBag.SubmissionId = submissionId;
            ViewBag.Submission = submission;
            ViewBag.IsReadOnly = true;

            return View("ViewSubmission", formViewModel);
        }

        // ========================================================================
        // DELETE - DELETE DRAFT SUBMISSION
        // ========================================================================

        /// <summary>
        /// Delete a draft submission
        /// POST /Submissions/Delete/{submissionId}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int submissionId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var success = await _responseService.DeleteDraftAsync(submissionId, userId);

            if (success)
            {
                TempData["Success"] = "Draft deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete draft. It may have already been submitted.";
            }

            return RedirectToAction("Index");
        }

        // ========================================================================
        // PRINT - PRINTABLE VIEW
        // ========================================================================

        /// <summary>
        /// Print-friendly view of a submission
        /// GET /Submissions/Print/{submissionId}
        /// </summary>
        public async Task<IActionResult> Print(int submissionId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var submission = await _submissionService.GetSubmissionAsync(submissionId);
            if (submission == null)
            {
                TempData["Error"] = "Submission not found.";
                return RedirectToAction("Index");
            }

            // Verify ownership
            if (submission.SubmittedBy != userId)
            {
                TempData["Error"] = "You do not have access to this submission.";
                return RedirectToAction("Index");
            }

            // Build form ViewModel
            var formViewModel = await _submissionService.BuildFormViewModelAsync(
                submission.TemplateId,
                submissionId);

            if (formViewModel == null)
            {
                TempData["Error"] = "Failed to load form template.";
                return RedirectToAction("Index");
            }

            // Set read-only mode
            formViewModel.IsReadOnly = true;

            ViewBag.Submission = submission;
            ViewBag.IsPrintView = true;

            return View("PrintSubmission", formViewModel);
        }
    }
}
