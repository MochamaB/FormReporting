namespace FormReporting.Services.Notifications
{
    /// <summary>
    /// Centralized service for triggering notifications across the application
    /// All notification triggers are defined here for easy tracking and maintenance
    /// </summary>
    public interface INotificationTriggerService
    {
        // ========================================================================
        // FORM ASSIGNMENT TRIGGERS (Phase 1 - MVP)
        // ========================================================================

        /// <summary>
        /// Trigger notification when a form is assigned to a user
        /// Template: ASSIGNMENT_CREATED
        /// </summary>
        Task TriggerFormAssignmentCreatedAsync(
            int assignmentId,
            int assignedToUserId,
            int assignedByUserId);

        /// <summary>
        /// Trigger notification when assignment deadline is approaching
        /// Template: DEADLINE_REMINDER
        /// </summary>
        Task TriggerAssignmentDeadlineReminderAsync(int assignmentId);

        /// <summary>
        /// Trigger notification when assignment is overdue
        /// Template: OVERDUE_ALERT
        /// </summary>
        Task TriggerAssignmentOverdueAsync(int assignmentId);

        // TODO: Phase 2 - Add Assignment Updated, Completed, Bulk Assignment triggers

        // ========================================================================
        // FORM SUBMISSION TRIGGERS (Phase 1 - MVP)
        // ========================================================================

        /// <summary>
        /// Trigger notification when a form is submitted
        /// Template: FORM_SUBMITTED
        /// </summary>
        Task TriggerFormSubmittedAsync(int responseId);

        /// <summary>
        /// Trigger notification when form is approved
        /// Template: FORM_APPROVED
        /// </summary>
        Task TriggerFormApprovedAsync(
            int responseId,
            int approvedByUserId,
            string? comments = null);

        /// <summary>
        /// Trigger notification when form is rejected
        /// Template: FORM_REJECTED
        /// </summary>
        Task TriggerFormRejectedAsync(
            int responseId,
            int rejectedByUserId,
            string? reason = null);

        // TODO: Phase 2 - Add Draft Saved, Returned for Revision, Revision Submitted triggers

        // ========================================================================
        // WORKFLOW TRIGGERS (Phase 1 - MVP)
        // ========================================================================

        /// <summary>
        /// Trigger notification when workflow step is assigned
        /// Template: WORKFLOW_ASSIGNED
        /// </summary>
        Task TriggerWorkflowStepAssignedAsync(
            int stepInstanceId,
            int assignedToUserId);

        /// <summary>
        /// Trigger notification when workflow step is completed
        /// Template: STEP_COMPLETED
        /// </summary>
        Task TriggerWorkflowStepCompletedAsync(
            int stepInstanceId,
            int completedByUserId);

        /// <summary>
        /// Trigger notification for pending approval reminders
        /// Template: PENDING_APPROVAL_ALERT
        /// </summary>
        Task TriggerPendingApprovalReminderAsync(int stepInstanceId);

        // TODO: Phase 2 - Add Workflow Reassigned, Delegated, Cancelled triggers

        // ========================================================================
        // BATCH TRIGGERS (for scheduled jobs - Hangfire)
        // ========================================================================

        /// <summary>
        /// Send deadline reminders for all assignments due within specified hours
        /// Used by Hangfire/background jobs
        /// </summary>
        Task SendBatchDeadlineRemindersAsync(int hoursBeforeDue = 24);

        /// <summary>
        /// Send overdue alerts for all overdue assignments
        /// Used by Hangfire/background jobs
        /// </summary>
        Task SendBatchOverdueAlertsAsync();

        /// <summary>
        /// Send pending approval alerts for workflow steps pending > 48 hours
        /// Used by Hangfire/background jobs
        /// </summary>
        Task SendBatchPendingApprovalAlertsAsync(int hoursPending = 48);

        // ========================================================================
        // FUTURE MODULES (To be implemented in later phases)
        // ========================================================================

        // TODO: Phase 2 - IDENTITY & AUTHENTICATION TRIGGERS
        // - TriggerUserAccountCreatedAsync()
        // - TriggerPasswordResetRequestAsync()
        // - TriggerPasswordChangedAsync()
        // - TriggerEmailVerificationAsync()
        // - TriggerAccountLockedAsync()

        // TODO: Phase 3 - COLLABORATION TRIGGERS
        // - TriggerCommentAddedAsync()
        // - TriggerUserMentionedAsync()
        // - TriggerCommentReplyAsync()

        // TODO: Phase 4 - SYSTEM & ADMIN TRIGGERS
        // - TriggerSystemMaintenanceAsync()
        // - TriggerDataExportReadyAsync()
        // - TriggerReportGeneratedAsync()
        // - TriggerBulkOperationCompleteAsync()

        // TODO: Phase 5 - DIGEST & PREFERENCES
        // - TriggerDailyDigestAsync()
        // - TriggerWeeklyDigestAsync()
    }
}
