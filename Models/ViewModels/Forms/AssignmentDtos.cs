using System.ComponentModel.DataAnnotations;

namespace FormReporting.Models.ViewModels.Forms
{
    /// <summary>
    /// DTO for creating a new assignment - defines WHO can access/submit a form.
    /// Submission rules (WHEN/HOW) are managed separately via FormTemplateSubmissionRule.
    /// </summary>
    public class AssignmentCreateDto
    {
        [Required]
        public int TemplateId { get; set; }

        [Required]
        [StringLength(50)]
        public string AssignmentType { get; set; } = string.Empty;
        // 'All', 'TenantType', 'TenantGroup', 'SpecificTenant', 'Role', 'Department', 'UserGroup', 'SpecificUser'

        // Target IDs based on AssignmentType
        public string? TenantType { get; set; }
        public int? TenantGroupId { get; set; }
        public int? TenantId { get; set; }
        public int? RoleId { get; set; }
        public int? DepartmentId { get; set; }
        public int? UserGroupId { get; set; }
        public int? UserId { get; set; }

        // Access Period
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
        public DateTime? EffectiveUntil { get; set; }

        // Access Options
        public bool AllowAnonymous { get; set; } = false;

        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing assignment
    /// </summary>
    public class AssignmentUpdateDto
    {
        // Access Period
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveUntil { get; set; }

        // Access Options
        public bool? AllowAnonymous { get; set; }

        // Status
        public string? Status { get; set; } // Active, Suspended, Revoked

        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for filtering assignments
    /// </summary>
    public class AssignmentFilterDto
    {
        public int? TemplateId { get; set; }
        public string? AssignmentType { get; set; }
        public string? Status { get; set; } // Active, Suspended, Revoked
        public DateTime? EffectiveAsOf { get; set; } // Filter assignments effective at this date
        public bool? AllowAnonymous { get; set; }
        public bool? IsExpired { get; set; } // EffectiveUntil < now

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        public string SortBy { get; set; } = "EffectiveFrom";
        public bool SortDescending { get; set; } = true;
    }

    /// <summary>
    /// DTO for assignment list item
    /// </summary>
    public class AssignmentListDto
    {
        public int AssignmentId { get; set; }
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateCode { get; set; } = string.Empty;
        public string AssignmentType { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty; // Resolved name (e.g., "Marketing Department")
        public string? TargetDetail { get; set; } // Additional info (e.g., email, tenant code)
        public int TargetCount { get; set; } // Number of users/tenants affected
        
        // Access Period
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveUntil { get; set; }
        
        // Access Options
        public bool AllowAnonymous { get; set; }
        
        // Status & Metadata
        public string Status { get; set; } = string.Empty; // Active, Suspended, Revoked
        public DateTime AssignedDate { get; set; }
        public string AssignedByName { get; set; } = string.Empty;
        
        // Computed properties
        public bool IsExpired => EffectiveUntil.HasValue && EffectiveUntil.Value < DateTime.UtcNow;
        public bool IsEffective => EffectiveFrom <= DateTime.UtcNow && !IsExpired && Status == "Active";

        // Computed properties for UI rendering
        public string TargetIcon => AssignmentType switch
        {
            "All" => "ri-global-line",
            "TenantType" => "ri-building-2-line",
            "TenantGroup" => "ri-team-line",
            "SpecificTenant" => "ri-store-2-line",
            "Role" => "ri-shield-user-line",
            "Department" => "ri-building-line",
            "UserGroup" => "ri-group-line",
            "SpecificUser" => "ri-user-line",
            _ => "ri-user-line"
        };

        public string TargetColor => AssignmentType switch
        {
            "All" => "primary",
            "TenantType" => "info",
            "TenantGroup" => "success",
            "SpecificTenant" => "warning",
            "Role" => "danger",
            "Department" => "secondary",
            "UserGroup" => "dark",
            "SpecificUser" => "primary",
            _ => "secondary"
        };

        public string TypeColor => TargetColor; // Same as target color

        public string StatusColor => Status switch
        {
            "Active" => IsEffective ? "success" : "warning",
            "Suspended" => "warning",
            "Revoked" => "danger",
            _ => IsExpired ? "secondary" : "info"
        };

        public string EffectivePeriodDisplay => EffectiveUntil.HasValue
            ? $"{EffectiveFrom:MMM d, yyyy} - {EffectiveUntil:MMM d, yyyy}"
            : $"{EffectiveFrom:MMM d, yyyy} - Indefinite";
    }

    /// <summary>
    /// Simplified DTO for assignment panel table rows
    /// </summary>
    public class AssignmentListItemDto
    {
        public int AssignmentId { get; set; }
        public string AssignmentType { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty;
        public string? TargetDetail { get; set; } // Additional info (e.g., email, tenant name)
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveUntil { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool AllowAnonymous { get; set; }
    }

    /// <summary>
    /// DTO for assignment details
    /// </summary>
    public class AssignmentDetailDto : AssignmentListDto
    {
        // Target Details
        public string? TenantType { get; set; }
        public int? TenantGroupId { get; set; }
        public string? TenantGroupName { get; set; }
        public int? TenantId { get; set; }
        public string? TenantName { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int? UserGroupId { get; set; }
        public string? UserGroupName { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        
        // Additional Info
        public string? Notes { get; set; }
        public int? CancelledBy { get; set; }
        public string? CancelledByName { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string? CancellationReason { get; set; }

        // Submission statistics for this assignment
        public int TotalExpectedSubmissions { get; set; }
        public int CompletedSubmissions { get; set; }
        public int PendingSubmissions { get; set; }
        public decimal CompletionRate => TotalExpectedSubmissions > 0 
            ? Math.Round((decimal)CompletedSubmissions / TotalExpectedSubmissions * 100, 1) 
            : 0;
    }

    /// <summary>
    /// DTO for assignment dashboard statistics
    /// </summary>
    public class AssignmentStatisticsDto
    {
        public int TotalAssignments { get; set; }
        public int ActiveAssignments { get; set; } // Status = Active
        public int SuspendedAssignments { get; set; }
        public int RevokedAssignments { get; set; }
        public int ExpiredAssignments { get; set; } // EffectiveUntil < now
        public int EffectiveAssignments { get; set; } // Currently effective (within date range, active)
        public int AnonymousAssignments { get; set; }

        // By assignment type breakdown
        public Dictionary<string, int> ByAssignmentType { get; set; } = new();

        // By status breakdown
        public Dictionary<string, int> ByStatus { get; set; } = new();
    }

    /// <summary>
    /// DTO for target preview (before creating assignment)
    /// </summary>
    public class AssignmentTargetPreviewDto
    {
        public string AssignmentType { get; set; } = string.Empty;
        public int TotalTargets { get; set; }
        public List<TargetItemDto> Targets { get; set; } = new();
    }

    /// <summary>
    /// Individual target item in preview
    /// </summary>
    public class TargetItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // User, Tenant, etc.
        public string? Email { get; set; }
        public string? Department { get; set; }
    }

    /// <summary>
    /// DTO for bulk operations
    /// </summary>
    public class AssignmentBulkOperationDto
    {
        [Required]
        public List<int> AssignmentIds { get; set; } = new();

        public string? Operation { get; set; } // "remind", "extend", "cancel"
        public DateTime? NewDueDate { get; set; } // For extend operation
        public string? CancellationReason { get; set; } // For cancel operation
    }

    /// <summary>
    /// DTO for target options in assignment wizard (scope-filtered)
    /// </summary>
    public class TargetOptionDto
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Group { get; set; } // For grouped dropdowns (e.g., by region)
        public int? Count { get; set; } // Number of affected users/tenants
    }

    /// <summary>
    /// Result of submission validation against assignment rules
    /// </summary>
    public class SubmissionValidationResult
    {
        public bool IsValid { get; set; }
        public bool CanSubmit { get; set; }
        public bool IsLate { get; set; }
        public string? ErrorMessage { get; set; }
        public string? BlockReason { get; set; }
        public DateTime? AllowedSubmissionDate { get; set; }
    }

    /// <summary>
    /// DTO for assignment coverage validation results
    /// </summary>
    public class AssignmentCoverageValidationDto
    {
        public bool HasSufficientCoverage { get; set; }
        public string SubmissionMode { get; set; } = string.Empty;
        public int ActiveAssignmentCount { get; set; }
        public int PotentialUserCount { get; set; }
        public List<string> CoverageIssues { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<AssignmentCoverageDetailDto> AssignmentDetails { get; set; } = new();
    }

    /// <summary>
    /// DTO for individual assignment coverage details
    /// </summary>
    public class AssignmentCoverageDetailDto
    {
        public int AssignmentId { get; set; }
        public string AssignmentType { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty;
        public int EstimatedUserCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveUntil { get; set; }
    }

    /// <summary>
    /// DTO for user's assignment view
    /// </summary>
    public class UserAssignmentDto
    {
        public int AssignmentId { get; set; }
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        
        // Access Period
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveUntil { get; set; }
        
        // Access Options
        public bool AllowAnonymous { get; set; }
        
        // Status
        public string Status { get; set; } = string.Empty;
        public bool HasSubmission { get; set; }
        public int? SubmissionId { get; set; }
        public string? SubmissionStatus { get; set; }
        
        // Computed
        public bool IsExpired => EffectiveUntil.HasValue && EffectiveUntil.Value < DateTime.UtcNow;
        public bool IsEffective => EffectiveFrom <= DateTime.UtcNow && !IsExpired && Status == "Active";
    }
}
