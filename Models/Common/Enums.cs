namespace FormReporting.Models.Common
{
    // ============================================================================
    // ORGANIZATIONAL ENUMS
    // ============================================================================

    /// <summary>
    /// Types of tenants in the system
    /// </summary>
    public enum TenantType
    {
        HeadOffice = 1,
        Regional = 2,
        Factory = 3,
        Subsidiary = 4,
        Warehouse = 5,
        Branch = 6
    }

    /// <summary>
    /// Status of a tenant
    /// </summary>
    public enum TenantStatus
    {
        Active = 1,
        Inactive = 2,
        Suspended = 3,
        Closed = 4
    }

    // ============================================================================
    // FORM ENUMS
    // ============================================================================

    /// <summary>
    /// Types of form fields
    /// Categories:
    /// - Input: Text, TextArea, Number, Decimal
    /// - Date/Time: Date, Time, DateTime
    /// - Selection: Dropdown, Radio, Checkbox, MultiSelect
    /// - Media: FileUpload, Image, Signature
    /// - Rating: Rating, Slider
    /// - Contact: Email, Phone, Url
    /// - Specialized: Currency, Percentage
    /// </summary>
    public enum FormFieldType
    {
        // Input fields
        Text = 1,
        TextArea = 2,
        Number = 3,
        Decimal = 4,

        // Date/Time fields
        Date = 5,
        Time = 6,
        DateTime = 7,

        // Selection fields
        Dropdown = 8,
        Radio = 9,
        Checkbox = 10,
        MultiSelect = 11,

        // Media fields
        FileUpload = 12,
        Image = 13,
        Signature = 14,

        // Rating fields
        Rating = 15,
        Slider = 16,

        // Contact fields
        Email = 17,
        Phone = 18,
        Url = 19,

        // Specialized fields
        Currency = 20,
        Percentage = 21
    }

    /// <summary>
    /// Validation types for form fields
    /// </summary>
    public enum ValidationRuleType
    {
        Required = 1,
        MinLength = 2,
        MaxLength = 3,
        MinValue = 4,
        MaxValue = 5,
        Regex = 6,
        Email = 7,
        Phone = 8,
        Url = 9,
        Custom = 10
    }

    /// <summary>
    /// Status of a form submission
    /// </summary>
    public enum SubmissionStatus
    {
        Draft = 1,
        Submitted = 2,
        UnderReview = 3,
        Approved = 4,
        Rejected = 5,
        Revised = 6,
        Cancelled = 7
    }

    /// <summary>
    /// Frequency for recurring form schedules
    /// </summary>
    public enum RecurrenceFrequency
    {
        Daily = 1,
        Weekly = 2,
        BiWeekly = 3,
        Monthly = 4,
        Quarterly = 5,
        SemiAnnually = 6,
        Annually = 7,
        Custom = 8
    }

    /// <summary>
    /// Render mode for form display
    /// </summary>
    public enum FormRenderMode
    {
        /// <summary>
        /// All sections displayed on single page (collapsible)
        /// </summary>
        SinglePage = 1,

        /// <summary>
        /// Sections displayed as wizard steps (one at a time with navigation)
        /// </summary>
        Wizard = 2
    }

    /// <summary>
    /// Layout type for form fields
    /// Defines how fields are displayed within a section
    /// </summary>
    public enum FieldLayoutType
    {
        /// <summary>
        /// Regular single field layout (default)
        /// </summary>
        Single = 1,

        /// <summary>
        /// Fields arranged in matrix/table layout (rows and columns)
        /// Used for rating scales, comparison grids, etc.
        /// </summary>
        Matrix = 2,

        /// <summary>
        /// Fields arranged in responsive grid layout
        /// </summary>
        Grid = 3,

        /// <summary>
        /// Fields displayed inline (horizontal)
        /// </summary>
        Inline = 4
    }

    /// <summary>
    /// Submission mode for form templates
    /// Defines HOW the form is filled (single user vs multi-user workflow)
    /// </summary>
    public enum SubmissionMode
    {
        /// <summary>
        /// Individual mode: One user fills entire form, then workflow happens (approval/review)
        /// FormTemplateAssignment controls who can access and submit
        /// Workflow steps (Approve/Reject/Sign/Review/Verify) execute AFTER submission
        /// </summary>
        Individual = 1,

        /// <summary>
        /// Collaborative mode: Multiple users fill different sections AS PART OF workflow
        /// Workflow Fill steps assign sections/fields to different users
        /// System initiates submission, users fill their assigned sections
        /// Approval steps happen after all Fill steps are complete
        /// </summary>
        Collaborative = 2
    }

    // ============================================================================
    // TICKET ENUMS
    // ============================================================================

    /// <summary>
    /// Status of a support ticket
    /// </summary>
    public enum TicketStatus
    {
        Open = 1,
        Assigned = 2,
        InProgress = 3,
        Pending = 4,
        Resolved = 5,
        Closed = 6,
        Reopened = 7,
        Cancelled = 8
    }

    /// <summary>
    /// Priority levels for tickets
    /// </summary>
    public enum TicketPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4,
        Critical = 5
    }

    /// <summary>
    /// Types of support tickets
    /// </summary>
    public enum TicketType
    {
        Incident = 1,
        Request = 2,
        Problem = 3,
        Change = 4,
        Question = 5
    }

    // ============================================================================
    // ASSET ENUMS
    // ============================================================================

    /// <summary>
    /// Status of hardware assets
    /// </summary>
    public enum AssetStatus
    {
        Available = 1,
        Assigned = 2,
        InUse = 3,
        InStorage = 4,
        UnderMaintenance = 5,
        Damaged = 6,
        Lost = 7,
        Stolen = 8,
        Disposed = 9,
        Retired = 10
    }

    /// <summary>
    /// Condition of hardware assets
    /// </summary>
    public enum AssetCondition
    {
        Excellent = 1,
        Good = 2,
        Fair = 3,
        Poor = 4,
        Faulty = 5
    }

    /// <summary>
    /// Types of asset maintenance
    /// </summary>
    public enum MaintenanceType
    {
        Preventive = 1,
        Corrective = 2,
        Emergency = 3,
        Routine = 4,
        Upgrade = 5
    }

    // ============================================================================
    // LICENSE ENUMS
    // ============================================================================

    /// <summary>
    /// Types of software licenses
    /// </summary>
    public enum LicenseType
    {
        Perpetual = 1,
        Subscription = 2,
        Trial = 3,
        OpenSource = 4,
        Freeware = 5,
        Enterprise = 6,
        Academic = 7
    }

    /// <summary>
    /// Status of software licenses
    /// </summary>
    public enum LicenseStatus
    {
        Active = 1,
        Expired = 2,
        Expiring = 3,
        Suspended = 4,
        Cancelled = 5,
        Revoked = 6
    }

    // ============================================================================
    // NOTIFICATION ENUMS
    // ============================================================================

    /// <summary>
    /// Notification delivery channels
    /// </summary>
    public enum NotificationChannel
    {
        Email = 1,
        SMS = 2,
        InApp = 3,
        WebPush = 4,
        Slack = 5,
        Teams = 6
    }

    /// <summary>
    /// Status of a notification
    /// </summary>
    public enum NotificationStatus
    {
        Pending = 1,
        Sent = 2,
        Delivered = 3,
        Read = 4,
        Failed = 5,
        Cancelled = 6
    }

    /// <summary>
    /// Priority levels for notifications
    /// </summary>
    public enum NotificationPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4
    }

    // ============================================================================
    // PERMISSION ENUMS
    // ============================================================================

    /// <summary>
    /// Types of permissions
    /// </summary>
    public enum PermissionType
    {
        View = 1,
        Create = 2,
        Edit = 3,
        Delete = 4,
        Approve = 5,
        Export = 6,
        Custom = 7
    }

    /// <summary>
    /// Role levels in the hierarchy
    /// </summary>
    public enum RoleLevel
    {
        HeadOffice = 1,
        Regional = 2,
        Factory = 3
    }

    // ============================================================================
    // REPORT ENUMS
    // ============================================================================

    /// <summary>
    /// Format types for report exports
    /// </summary>
    public enum ReportFormat
    {
        PDF = 1,
        Excel = 2,
        CSV = 3,
        HTML = 4,
        JSON = 5,
        XML = 6
    }

    /// <summary>
    /// Status of report execution
    /// </summary>
    public enum ReportExecutionStatus
    {
        Pending = 1,
        Running = 2,
        Completed = 3,
        Failed = 4,
        Cancelled = 5
    }

    // ============================================================================
    // FINANCIAL ENUMS
    // ============================================================================

    /// <summary>
    /// Types of expenditures
    /// </summary>
    public enum ExpenditureType
    {
        Hardware = 1,
        Software = 2,
        License = 3,
        Maintenance = 4,
        Training = 5,
        Consulting = 6,
        Infrastructure = 7,
        Other = 8
    }

    /// <summary>
    /// Budget period types
    /// </summary>
    public enum BudgetPeriod
    {
        Monthly = 1,
        Quarterly = 2,
        SemiAnnual = 3,
        Annual = 4,
        Custom = 5
    }

    /// <summary>
    /// Status of a budget
    /// </summary>
    public enum BudgetStatus
    {
        Draft = 1,
        Submitted = 2,
        Approved = 3,
        Active = 4,
        Closed = 5,
        Cancelled = 6
    }

    // ============================================================================
    // AUDIT ENUMS
    // ============================================================================

    /// <summary>
    /// Types of audit actions
    /// </summary>
    public enum AuditAction
    {
        Create = 1,
        Read = 2,
        Update = 3,
        Delete = 4,
        Login = 5,
        Logout = 6,
        PermissionGranted = 7,
        PermissionRevoked = 8,
        Export = 9,
        Import = 10,
        Approve = 11,
        Reject = 12,
        Custom = 13
    }

    // ============================================================================
    // METRIC ENUMS
    // ============================================================================

    /// <summary>
    /// Data types for metrics
    /// </summary>
    public enum MetricDataType
    {
        Integer = 1,
        Decimal = 2,
        Percentage = 3,
        Currency = 4,
        Duration = 5,
        Count = 6
    }

    /// <summary>
    /// Aggregation types for metrics
    /// </summary>
    public enum MetricAggregation
    {
        Sum = 1,
        Average = 2,
        Min = 3,
        Max = 4,
        Count = 5,
        Latest = 6
    }

}
