using FormReporting.Data;
using FormReporting.Models.ViewModels.Forms;
using FormReporting.Models.Entities.Forms;
using FormReporting.Services.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service for managing form template submission rules
    /// </summary>
    public class SubmissionRuleService : ISubmissionRuleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IClaimsService _claimsService;
        private readonly ILogger<SubmissionRuleService> _logger;

        public SubmissionRuleService(
            ApplicationDbContext context,
            IClaimsService claimsService,
            ILogger<SubmissionRuleService> logger)
        {
            _context = context;
            _claimsService = claimsService;
            _logger = logger;
        }

        // ===== CRUD OPERATIONS =====

        public async Task<SubmissionRuleDetailDto?> GetByIdAsync(int ruleId)
        {
            try
            {
                var rule = await _context.FormTemplateSubmissionRules
                    .Include(r => r.Template)
                    .Include(r => r.CreatedByUser)
                    .Include(r => r.ModifiedByUser)
                    .FirstOrDefaultAsync(r => r.SubmissionRuleId == ruleId);

                if (rule == null)
                    return null;

                return new SubmissionRuleDetailDto
                {
                    SubmissionRuleId = rule.SubmissionRuleId,
                    TemplateId = rule.TemplateId,
                    TemplateName = rule.Template.TemplateName,
                    RuleName = rule.RuleName,
                    Description = rule.Description,
                    Frequency = rule.Frequency,
                    FrequencyDisplay = GetFrequencyDisplayText(rule.Frequency, rule.DueDay, rule.DueMonth),
                    DueDay = rule.DueDay,
                    DueDayDisplay = GetDueDayDisplayText(rule.Frequency, rule.DueDay),
                    DueMonth = rule.DueMonth,
                    DueMonthDisplay = GetDueMonthDisplayText(rule.DueMonth),
                    DueTime = rule.DueTime,
                    DueTimeDisplay = rule.DueTime?.ToString(@"hh\:mm"),
                    SpecificDueDate = rule.SpecificDueDate,
                    SpecificDueDateDisplay = rule.SpecificDueDate?.ToString("yyyy-MM-dd HH:mm"),
                    NextDueDate = CalculateNextDueDate(rule),
                    NextDueDateDisplay = CalculateNextDueDate(rule)?.ToString("yyyy-MM-dd HH:mm"),
                    GracePeriodDays = rule.GracePeriodDays,
                    AllowLateSubmission = rule.AllowLateSubmission,
                    ReminderDaysBefore = rule.ReminderDaysBefore,
                    ReminderDays = ParseReminderDays(rule.ReminderDaysBefore),
                    Status = rule.Status,
                    CreatedBy = rule.CreatedBy,
                    CreatedByName = $"{rule.CreatedByUser.FirstName} {rule.CreatedByUser.LastName}",
                    CreatedDate = rule.CreatedDate,
                    ModifiedBy = rule.ModifiedBy,
                    ModifiedByName = rule.ModifiedByUser != null ? $"{rule.ModifiedByUser.FirstName} {rule.ModifiedByUser.LastName}" : null,
                    ModifiedDate = rule.ModifiedDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submission rule {RuleId}", ruleId);
                throw;
            }
        }

        public async Task<List<SubmissionRuleListDto>> GetByTemplateIdAsync(int templateId)
        {
            try
            {
                var rules = await _context.FormTemplateSubmissionRules
                    .Include(r => r.CreatedByUser)
                    .Where(r => r.TemplateId == templateId)
                    .OrderBy(r => r.RuleName)
                    .ToListAsync();

                return rules.Select(rule => new SubmissionRuleListDto
                {
                    SubmissionRuleId = rule.SubmissionRuleId,
                    TemplateId = rule.TemplateId,
                    RuleName = rule.RuleName,
                    Description = rule.Description,
                    Frequency = rule.Frequency,
                    FrequencyDisplay = GetFrequencyDisplayText(rule.Frequency, rule.DueDay, rule.DueMonth),
                    NextDueDate = CalculateNextDueDate(rule),
                    NextDueDateDisplay = CalculateNextDueDate(rule)?.ToString("yyyy-MM-dd HH:mm"),
                    GracePeriodDays = rule.GracePeriodDays,
                    AllowLateSubmission = rule.AllowLateSubmission,
                    ReminderDaysBefore = rule.ReminderDaysBefore,
                    Status = rule.Status,
                    CreatedByName = $"{rule.CreatedByUser.FirstName} {rule.CreatedByUser.LastName}",
                    CreatedDate = rule.CreatedDate
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submission rules for template {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<SubmissionRuleDetailDto> CreateAsync(SubmissionRuleCreateDto dto)
        {
            try
            {
                var userId = _claimsService.GetUserId();
                if (userId == 0)
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                // Validate the rule
                if (!await ValidateRuleAsync(dto))
                {
                    throw new ArgumentException("Invalid submission rule configuration");
                }

                var rule = new FormTemplateSubmissionRule
                {
                    TemplateId = dto.TemplateId,
                    RuleName = dto.RuleName,
                    Description = dto.Description,
                    Frequency = dto.Frequency,
                    DueDay = dto.DueDay,
                    DueMonth = dto.DueMonth,
                    DueTime = dto.DueTime,
                    SpecificDueDate = dto.SpecificDueDate,
                    GracePeriodDays = dto.GracePeriodDays,
                    AllowLateSubmission = dto.AllowLateSubmission,
                    ReminderDaysBefore = dto.ReminderDaysBefore,
                    Status = "Active",
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow
                };

                _context.FormTemplateSubmissionRules.Add(rule);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created submission rule {RuleId} for template {TemplateId}", rule.SubmissionRuleId, dto.TemplateId);

                return (await GetByIdAsync(rule.SubmissionRuleId))!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating submission rule for template {TemplateId}", dto.TemplateId);
                throw;
            }
        }

        public async Task<SubmissionRuleDetailDto> UpdateAsync(SubmissionRuleUpdateDto dto)
        {
            try
            {
                var userId = _claimsService.GetUserId();
                if (userId == 0)
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                var rule = await _context.FormTemplateSubmissionRules
                    .FirstOrDefaultAsync(r => r.SubmissionRuleId == dto.SubmissionRuleId);

                if (rule == null)
                {
                    throw new ArgumentException($"Submission rule {dto.SubmissionRuleId} not found");
                }

                // Update properties
                rule.RuleName = dto.RuleName;
                rule.Description = dto.Description;
                rule.Frequency = dto.Frequency;
                rule.DueDay = dto.DueDay;
                rule.DueMonth = dto.DueMonth;
                rule.DueTime = dto.DueTime;
                rule.SpecificDueDate = dto.SpecificDueDate;
                rule.GracePeriodDays = dto.GracePeriodDays;
                rule.AllowLateSubmission = dto.AllowLateSubmission;
                rule.ReminderDaysBefore = dto.ReminderDaysBefore;
                rule.Status = dto.Status;
                rule.ModifiedBy = userId;
                rule.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated submission rule {RuleId}", dto.SubmissionRuleId);

                return (await GetByIdAsync(rule.SubmissionRuleId))!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating submission rule {RuleId}", dto.SubmissionRuleId);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int ruleId)
        {
            try
            {
                var rule = await _context.FormTemplateSubmissionRules
                    .FirstOrDefaultAsync(r => r.SubmissionRuleId == ruleId);

                if (rule == null)
                {
                    return false;
                }

                _context.FormTemplateSubmissionRules.Remove(rule);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted submission rule {RuleId}", ruleId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting submission rule {RuleId}", ruleId);
                throw;
            }
        }

        // ===== DATE CALCULATION =====

        public DateTime? CalculateNextDueDate(FormTemplateSubmissionRule rule, DateTime? fromDate = null)
        {
            if (string.IsNullOrEmpty(rule.Frequency))
                return null;

            var baseDate = fromDate ?? DateTime.UtcNow;
            var dueTime = rule.DueTime ?? TimeSpan.Zero;

            try
            {
                return rule.Frequency.ToLower() switch
                {
                    "once" => rule.SpecificDueDate,
                    "daily" => CalculateDailyDueDate(baseDate, dueTime),
                    "weekly" => CalculateWeeklyDueDate(baseDate, rule.DueDay, dueTime),
                    "monthly" => CalculateMonthlyDueDate(baseDate, rule.DueDay, dueTime),
                    "quarterly" => CalculateQuarterlyDueDate(baseDate, rule.DueDay, dueTime),
                    "annually" => CalculateAnnuallyDueDate(baseDate, rule.DueDay, rule.DueMonth, dueTime),
                    _ => null
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating next due date for rule {RuleId}", rule.SubmissionRuleId);
                return null;
            }
        }

        private DateTime CalculateDailyDueDate(DateTime baseDate, TimeSpan dueTime)
        {
            var today = baseDate.Date.Add(dueTime);
            return today > baseDate ? today : today.AddDays(1);
        }

        private DateTime? CalculateWeeklyDueDate(DateTime baseDate, int? dueDay, TimeSpan dueTime)
        {
            if (!dueDay.HasValue || dueDay < 0 || dueDay > 6)
                return null;

            var targetDayOfWeek = (DayOfWeek)dueDay.Value;
            var daysUntilTarget = ((int)targetDayOfWeek - (int)baseDate.DayOfWeek + 7) % 7;
            
            if (daysUntilTarget == 0)
            {
                var todayDue = baseDate.Date.Add(dueTime);
                if (todayDue > baseDate)
                    return todayDue;
                daysUntilTarget = 7;
            }

            return baseDate.Date.AddDays(daysUntilTarget).Add(dueTime);
        }

        private DateTime? CalculateMonthlyDueDate(DateTime baseDate, int? dueDay, TimeSpan dueTime)
        {
            if (!dueDay.HasValue)
                return null;

            var targetDay = dueDay.Value;
            var currentMonth = baseDate.Month;
            var currentYear = baseDate.Year;

            // Handle last day of month (-1)
            if (targetDay == -1)
            {
                var lastDayThisMonth = DateTime.DaysInMonth(currentYear, currentMonth);
                var thisMonthDue = new DateTime(currentYear, currentMonth, lastDayThisMonth).Add(dueTime);
                
                if (thisMonthDue > baseDate)
                    return thisMonthDue;

                // Next month
                var nextMonth = currentMonth == 12 ? 1 : currentMonth + 1;
                var nextYear = currentMonth == 12 ? currentYear + 1 : currentYear;
                var lastDayNextMonth = DateTime.DaysInMonth(nextYear, nextMonth);
                return new DateTime(nextYear, nextMonth, lastDayNextMonth).Add(dueTime);
            }

            // Regular day of month
            if (targetDay < 1 || targetDay > 31)
                return null;

            // Try this month first
            var daysInCurrentMonth = DateTime.DaysInMonth(currentYear, currentMonth);
            if (targetDay <= daysInCurrentMonth)
            {
                var thisMonthDue = new DateTime(currentYear, currentMonth, targetDay).Add(dueTime);
                if (thisMonthDue > baseDate)
                    return thisMonthDue;
            }

            // Next month
            var nextMonthValue = currentMonth == 12 ? 1 : currentMonth + 1;
            var nextYearValue = currentMonth == 12 ? currentYear + 1 : currentYear;
            var daysInNextMonth = DateTime.DaysInMonth(nextYearValue, nextMonthValue);
            
            if (targetDay <= daysInNextMonth)
            {
                return new DateTime(nextYearValue, nextMonthValue, targetDay).Add(dueTime);
            }

            return null;
        }

        private DateTime? CalculateQuarterlyDueDate(DateTime baseDate, int? dueDay, TimeSpan dueTime)
        {
            if (!dueDay.HasValue)
                return null;

            var currentQuarter = (baseDate.Month - 1) / 3 + 1;
            var quarterStartMonth = (currentQuarter - 1) * 3 + 1;
            var quarterEndMonth = quarterStartMonth + 2;

            // Try current quarter first
            for (int month = baseDate.Month; month <= quarterEndMonth; month++)
            {
                var dueDate = CalculateMonthlyDueDate(new DateTime(baseDate.Year, month, 1), dueDay, dueTime);
                if (dueDate.HasValue && dueDate > baseDate)
                    return dueDate;
            }

            // Next quarter
            var nextQuarterStartMonth = quarterEndMonth + 1;
            if (nextQuarterStartMonth > 12)
            {
                nextQuarterStartMonth = 1;
                return CalculateMonthlyDueDate(new DateTime(baseDate.Year + 1, nextQuarterStartMonth, 1), dueDay, dueTime);
            }

            return CalculateMonthlyDueDate(new DateTime(baseDate.Year, nextQuarterStartMonth, 1), dueDay, dueTime);
        }

        private DateTime? CalculateAnnuallyDueDate(DateTime baseDate, int? dueDay, int? dueMonth, TimeSpan dueTime)
        {
            if (!dueDay.HasValue || !dueMonth.HasValue)
                return null;

            if (dueMonth < 1 || dueMonth > 12)
                return null;

            // Try this year first
            var thisYearDue = CalculateMonthlyDueDate(new DateTime(baseDate.Year, dueMonth.Value, 1), dueDay, dueTime);
            if (thisYearDue.HasValue && thisYearDue > baseDate)
                return thisYearDue;

            // Next year
            return CalculateMonthlyDueDate(new DateTime(baseDate.Year + 1, dueMonth.Value, 1), dueDay, dueTime);
        }

        // ===== DISPLAY TEXT HELPERS =====

        public string GetFrequencyDisplayText(string? frequency, int? dueDay, int? dueMonth)
        {
            if (string.IsNullOrEmpty(frequency))
                return "No schedule";

            return frequency.ToLower() switch
            {
                "once" => "One-time",
                "daily" => "Daily",
                "weekly" => GetWeeklyDisplayText(dueDay),
                "monthly" => GetMonthlyDisplayText(dueDay),
                "quarterly" => GetQuarterlyDisplayText(dueDay),
                "annually" => GetAnnuallyDisplayText(dueDay, dueMonth),
                _ => frequency
            };
        }

        private string GetWeeklyDisplayText(int? dueDay)
        {
            if (!dueDay.HasValue || dueDay < 0 || dueDay > 6)
                return "Weekly";

            var dayName = ((DayOfWeek)dueDay.Value).ToString();
            return $"Weekly on {dayName}";
        }

        private string GetMonthlyDisplayText(int? dueDay)
        {
            if (!dueDay.HasValue)
                return "Monthly";

            if (dueDay == -1)
                return "Monthly on last day";

            var suffix = GetOrdinalSuffix(dueDay.Value);
            return $"Monthly on {dueDay}{suffix}";
        }

        private string GetQuarterlyDisplayText(int? dueDay)
        {
            if (!dueDay.HasValue)
                return "Quarterly";

            if (dueDay == -1)
                return "Quarterly on last day";

            var suffix = GetOrdinalSuffix(dueDay.Value);
            return $"Quarterly on {dueDay}{suffix}";
        }

        private string GetAnnuallyDisplayText(int? dueDay, int? dueMonth)
        {
            if (!dueDay.HasValue || !dueMonth.HasValue)
                return "Annually";

            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dueMonth.Value);
            
            if (dueDay == -1)
                return $"Annually on last day of {monthName}";

            var suffix = GetOrdinalSuffix(dueDay.Value);
            return $"Annually on {monthName} {dueDay}{suffix}";
        }

        public string? GetDueDayDisplayText(string? frequency, int? dueDay)
        {
            if (!dueDay.HasValue || string.IsNullOrEmpty(frequency))
                return null;

            return frequency.ToLower() switch
            {
                "weekly" => ((DayOfWeek)dueDay.Value).ToString(),
                "monthly" or "quarterly" => dueDay == -1 ? "Last day" : $"{dueDay}{GetOrdinalSuffix(dueDay.Value)}",
                "annually" => dueDay == -1 ? "Last day" : $"{dueDay}{GetOrdinalSuffix(dueDay.Value)}",
                _ => dueDay.ToString()
            };
        }

        public string? GetDueMonthDisplayText(int? dueMonth)
        {
            if (!dueMonth.HasValue || dueMonth < 1 || dueMonth > 12)
                return null;

            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dueMonth.Value);
        }

        private string GetOrdinalSuffix(int number)
        {
            if (number % 100 >= 11 && number % 100 <= 13)
                return "th";

            return (number % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
        }

        private List<int>? ParseReminderDays(string? reminderDaysBefore)
        {
            if (string.IsNullOrEmpty(reminderDaysBefore))
                return null;

            try
            {
                return reminderDaysBefore
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.Parse(s.Trim()))
                    .Where(d => d > 0)
                    .OrderByDescending(d => d)
                    .ToList();
            }
            catch
            {
                return null;
            }
        }

        // ===== VALIDATION =====

        public async Task<bool> ValidateRuleAsync(SubmissionRuleCreateDto dto)
        {
            try
            {
                // Check if template exists
                var templateExists = await _context.FormTemplates
                    .AnyAsync(t => t.TemplateId == dto.TemplateId);

                if (!templateExists)
                    return false;

                // Validate frequency-specific rules
                if (!string.IsNullOrEmpty(dto.Frequency))
                {
                    switch (dto.Frequency.ToLower())
                    {
                        case "weekly":
                            if (!dto.DueDay.HasValue || dto.DueDay < 0 || dto.DueDay > 6)
                                return false;
                            break;

                        case "monthly":
                        case "quarterly":
                            if (!dto.DueDay.HasValue || (dto.DueDay < 1 || dto.DueDay > 31) && dto.DueDay != -1)
                                return false;
                            break;

                        case "annually":
                            if (!dto.DueDay.HasValue || !dto.DueMonth.HasValue)
                                return false;
                            if ((dto.DueDay < 1 || dto.DueDay > 31) && dto.DueDay != -1)
                                return false;
                            if (dto.DueMonth < 1 || dto.DueMonth > 12)
                                return false;
                            break;

                        case "once":
                            if (!dto.SpecificDueDate.HasValue)
                                return false;
                            break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating submission rule");
                return false;
            }
        }

        public async Task<bool> CanAddRuleToTemplateAsync(int templateId)
        {
            try
            {
                // Check if template exists and is not archived
                var template = await _context.FormTemplates
                    .FirstOrDefaultAsync(t => t.TemplateId == templateId);

                return template != null && template.PublishStatus != "Archived";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if rule can be added to template {TemplateId}", templateId);
                return false;
            }
        }

        // ===== SUBMISSION TIMING VALIDATION =====

        public async Task<SubmissionTimingValidationResult> ValidateSubmissionTimingAsync(int templateId, DateTime submissionDate)
        {
            try
            {
                var rules = await _context.FormTemplateSubmissionRules
                    .Where(r => r.TemplateId == templateId && r.Status == "Active")
                    .ToListAsync();

                if (!rules.Any())
                {
                    return new SubmissionTimingValidationResult
                    {
                        CanSubmit = true,
                        Message = "No submission rules defined"
                    };
                }

                // For now, use the first active rule (could be extended to handle multiple rules)
                var rule = rules.First();
                var dueDate = CalculateNextDueDate(rule, submissionDate.AddDays(-30)); // Look back to find relevant due date

                if (!dueDate.HasValue)
                {
                    return new SubmissionTimingValidationResult
                    {
                        CanSubmit = true,
                        Message = "No due date calculated"
                    };
                }

                var gracePeriodEnd = dueDate.Value.AddDays(rule.GracePeriodDays);
                var isLate = submissionDate > dueDate.Value;
                var isWithinGracePeriod = isLate && submissionDate <= gracePeriodEnd;
                var canSubmit = !isLate || isWithinGracePeriod || rule.AllowLateSubmission;

                return new SubmissionTimingValidationResult
                {
                    CanSubmit = canSubmit,
                    IsLate = isLate,
                    IsWithinGracePeriod = isWithinGracePeriod,
                    DueDate = dueDate,
                    GracePeriodEnd = gracePeriodEnd,
                    Message = GetTimingMessage(isLate, isWithinGracePeriod, canSubmit, dueDate.Value, rule.AllowLateSubmission)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating submission timing for template {TemplateId}", templateId);
                return new SubmissionTimingValidationResult
                {
                    CanSubmit = false,
                    ValidationErrors = { "Error validating submission timing" }
                };
            }
        }

        private string GetTimingMessage(bool isLate, bool isWithinGracePeriod, bool canSubmit, DateTime dueDate, bool allowLateSubmission)
        {
            if (!isLate)
                return "Submission is on time";

            if (isWithinGracePeriod)
                return "Submission is late but within grace period";

            if (allowLateSubmission)
                return "Submission is late but allowed";

            return "Submission is too late and not allowed";
        }

        public async Task<List<SubmissionRuleDetailDto>> GetRulesNeedingRemindersAsync(DateTime forDate)
        {
            try
            {
                var activeRules = await _context.FormTemplateSubmissionRules
                    .Include(r => r.Template)
                    .Include(r => r.CreatedByUser)
                    .Where(r => r.Status == "Active" && !string.IsNullOrEmpty(r.ReminderDaysBefore))
                    .ToListAsync();

                var rulesNeedingReminders = new List<SubmissionRuleDetailDto>();

                foreach (var rule in activeRules)
                {
                    var reminderDays = ParseReminderDays(rule.ReminderDaysBefore);
                    if (reminderDays == null || !reminderDays.Any())
                        continue;

                    var nextDueDate = CalculateNextDueDate(rule, forDate);
                    if (!nextDueDate.HasValue)
                        continue;

                    var daysUntilDue = (nextDueDate.Value.Date - forDate.Date).Days;
                    
                    if (reminderDays.Contains(daysUntilDue))
                    {
                        var dto = await GetByIdAsync(rule.SubmissionRuleId);
                        if (dto != null)
                            rulesNeedingReminders.Add(dto);
                    }
                }

                return rulesNeedingReminders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rules needing reminders for date {Date}", forDate);
                return new List<SubmissionRuleDetailDto>();
            }
        }
    }
}
