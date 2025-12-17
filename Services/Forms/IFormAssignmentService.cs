using FormReporting.Models.ViewModels.Forms;

namespace FormReporting.Services.Forms
{
    /// <summary>
    /// Service interface for managing form template assignments
    /// </summary>
    public interface IFormAssignmentService
    {
        // ===== Assignment CRUD =====

        /// <summary>
        /// Get assignments with filtering and pagination
        /// </summary>
        Task<(List<AssignmentListDto> Items, int TotalCount)> GetAssignmentsAsync(AssignmentFilterDto filter);

        /// <summary>
        /// Get assignment by ID with full details
        /// </summary>
        Task<AssignmentDetailDto?> GetAssignmentByIdAsync(int assignmentId);

        /// <summary>
        /// Create a new assignment
        /// </summary>
        Task<AssignmentDetailDto> CreateAssignmentAsync(AssignmentCreateDto dto);

        /// <summary>
        /// Update an existing assignment
        /// </summary>
        Task<AssignmentDetailDto?> UpdateAssignmentAsync(int assignmentId, AssignmentUpdateDto dto);

        /// <summary>
        /// Cancel an assignment
        /// </summary>
        Task<bool> CancelAssignmentAsync(int assignmentId, string reason);

        /// <summary>
        /// Extend assignment effective period
        /// </summary>
        Task<bool> ExtendEffectivePeriodAsync(int assignmentId, DateTime newEffectiveUntil);

        /// <summary>
        /// Suspend an assignment temporarily
        /// </summary>
        Task<bool> SuspendAssignmentAsync(int assignmentId, string? reason = null);

        /// <summary>
        /// Reactivate a suspended assignment
        /// </summary>
        Task<bool> ReactivateAssignmentAsync(int assignmentId);

        // ===== Target Preview =====

        /// <summary>
        /// Preview targets for an assignment (before creating)
        /// </summary>
        Task<AssignmentTargetPreviewDto> GetTargetPreviewAsync(AssignmentCreateDto dto);

        /// <summary>
        /// Get count of targets for an assignment type
        /// </summary>
        Task<int> GetTargetCountAsync(string assignmentType, int? targetId, string? tenantType = null);

        /// <summary>
        /// Get target options for an assignment type (scope-filtered)
        /// </summary>
        Task<List<TargetOptionDto>> GetTargetOptionsAsync(System.Security.Claims.ClaimsPrincipal user, string assignmentType);

        // ===== Statistics & Reporting =====

        /// <summary>
        /// Get assignment statistics for dashboard
        /// </summary>
        Task<AssignmentStatisticsDto> GetAssignmentStatisticsAsync(int? templateId = null);

        /// <summary>
        /// Get compliance metrics for reporting
        /// </summary>
        Task<AssignmentStatisticsDto> GetComplianceMetricsAsync(AssignmentFilterDto filter);

        // ===== User Access =====

        /// <summary>
        /// Check if user has access to a template via assignments
        /// </summary>
        Task<bool> CheckUserAccessAsync(int userId, int templateId);

        /// <summary>
        /// Get all assignments for a specific user
        /// </summary>
        Task<List<UserAssignmentDto>> GetUserAssignmentsAsync(int userId, int? templateId = null);

        /// <summary>
        /// Get assignments due for a user
        /// </summary>
        Task<List<UserAssignmentDto>> GetUserPendingAssignmentsAsync(int userId);

        // ===== Bulk Operations =====

        /// <summary>
        /// Send reminders for assignments
        /// </summary>
        Task<int> SendRemindersAsync(List<int> assignmentIds);

        /// <summary>
        /// Bulk extend effective periods
        /// </summary>
        Task<int> BulkExtendEffectivePeriodsAsync(List<int> assignmentIds, DateTime newEffectiveUntil);

        /// <summary>
        /// Bulk cancel assignments
        /// </summary>
        Task<int> BulkCancelAsync(List<int> assignmentIds, string reason);

        // ===== Background Jobs =====

        /// <summary>
        /// Process expired assignments (mark as expired/revoked)
        /// </summary>
        Task<int> ProcessExpiredAssignmentsAsync();

        /// <summary>
        /// Send reminders for upcoming due dates (to be implemented with SubmissionRules)
        /// </summary>
        Task<int> SendDueDateRemindersAsync();

        // ===== Submission Validation =====

        /// <summary>
        /// Validate if a submission can be made based on assignment access period.
        /// Due date validation will be handled by FormTemplateSubmissionRule.
        /// </summary>
        Task<SubmissionValidationResult> ValidateSubmissionAsync(int assignmentId, DateTime submissionDate);
    }
}
