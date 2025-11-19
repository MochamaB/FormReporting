using FormReporting.Models.ViewModels.Components;

namespace FormReporting.Extensions
{
    /// <summary>
    /// Extension methods for transforming AssignmentManagerConfig → AssignmentManagerViewModel
    /// Follows the three-layer component pattern
    /// </summary>
    public static class AssignmentManagerExtensions
    {
        /// <summary>
        /// Build AssignmentManagerViewModel from Config
        /// Main transformation method
        /// </summary>
        public static AssignmentManagerViewModel BuildAssignmentManager(this AssignmentManagerConfig config)
        {
            return new AssignmentManagerViewModel
            {
                ManagerId = config.ManagerId,
                ContextLabel = config.ContextLabel,
                SupportedTypes = config.SupportedTypes.Select(TransformAssignmentType).ToList(),
                CurrentAssignments = config.CurrentAssignments
                    .OrderBy(a => a.Level ?? 0)
                    .ThenBy(a => a.TargetName)
                    .Select(TransformAssignmentItem)
                    .ToList(),
                ShowLevels = config.ShowLevels,
                AllowMultiplePerLevel = config.AllowMultiplePerLevel,
                ShowAddButton = config.ShowAddButton,
                ShowRemoveButton = config.ShowRemoveButton,
                IsCollapsible = config.IsCollapsible,
                InitiallyCollapsed = config.InitiallyCollapsed,
                CssClasses = config.CssClasses,
                HelpText = config.HelpText,
                SearchEndpoint = config.SearchEndpoint
            };
        }

        /// <summary>
        /// Fluent API: Add an assignment type
        /// </summary>
        public static AssignmentManagerConfig WithAssignmentType(
            this AssignmentManagerConfig config,
            string typeCode,
            string typeLabel,
            string icon = "ri-user-line",
            bool isDefault = false)
        {
            config.SupportedTypes.Add(new AssignmentType
            {
                TypeCode = typeCode,
                TypeLabel = typeLabel,
                Icon = icon,
                IsDefault = isDefault
            });
            return config;
        }

        /// <summary>
        /// Fluent API: Add current assignment
        /// </summary>
        public static AssignmentManagerConfig WithAssignment(
            this AssignmentManagerConfig config,
            string assignmentType,
            int targetId,
            string targetName,
            string? targetDetails = null,
            int? level = null,
            bool isMandatory = true)
        {
            config.CurrentAssignments.Add(new AssignmentItem
            {
                AssignmentType = assignmentType,
                TargetId = targetId,
                TargetName = targetName,
                TargetDetails = targetDetails,
                Level = level,
                IsMandatory = isMandatory
            });
            return config;
        }

        /// <summary>
        /// Fluent API: Configure for approval workflow context
        /// </summary>
        public static AssignmentManagerConfig ForApprovalWorkflow(this AssignmentManagerConfig config)
        {
            config.ContextLabel = "Approval Levels";
            config.ShowLevels = true;
            config.AllowMultiplePerLevel = false;
            config.SupportedTypes = new List<AssignmentType>
            {
                new AssignmentType { TypeCode = "User", TypeLabel = "Specific User", Icon = "ri-user-line", IsDefault = true },
                new AssignmentType { TypeCode = "Role", TypeLabel = "By Role", Icon = "ri-shield-user-line" }
            };
            return config;
        }

        /// <summary>
        /// Fluent API: Configure for form template assignments
        /// </summary>
        public static AssignmentManagerConfig ForFormAssignments(this AssignmentManagerConfig config)
        {
            config.ContextLabel = "Form Assignments";
            config.ShowLevels = false;
            config.AllowMultiplePerLevel = true;
            config.SupportedTypes = new List<AssignmentType>
            {
                new AssignmentType { TypeCode = "User", TypeLabel = "Specific User", Icon = "ri-user-line" },
                new AssignmentType { TypeCode = "Role", TypeLabel = "By Role", Icon = "ri-shield-user-line", IsDefault = true },
                new AssignmentType { TypeCode = "Department", TypeLabel = "By Department", Icon = "ri-building-line" },
                new AssignmentType { TypeCode = "UserGroup", TypeLabel = "User Group", Icon = "ri-group-line" },
                new AssignmentType { TypeCode = "Tenant", TypeLabel = "Factory/Branch", Icon = "ri-community-line" }
            };
            return config;
        }

        /// <summary>
        /// Fluent API: Configure for user group members
        /// </summary>
        public static AssignmentManagerConfig ForUserGroupMembers(this AssignmentManagerConfig config)
        {
            config.ContextLabel = "Group Members";
            config.ShowLevels = false;
            config.AllowMultiplePerLevel = true;
            config.SupportedTypes = new List<AssignmentType>
            {
                new AssignmentType { TypeCode = "User", TypeLabel = "Add User", Icon = "ri-user-add-line", IsDefault = true }
            };
            return config;
        }

        /// <summary>
        /// Fluent API: Make collapsible
        /// </summary>
        public static AssignmentManagerConfig AsCollapsible(
            this AssignmentManagerConfig config,
            bool initiallyCollapsed = true)
        {
            config.IsCollapsible = true;
            config.InitiallyCollapsed = initiallyCollapsed;
            return config;
        }

        // ========================================================================
        // PRIVATE TRANSFORMATION HELPERS
        // ========================================================================

        private static AssignmentTypeViewModel TransformAssignmentType(AssignmentType type)
        {
            return new AssignmentTypeViewModel
            {
                TypeCode = type.TypeCode,
                TypeLabel = type.TypeLabel,
                Icon = type.Icon,
                IsDefault = type.IsDefault
            };
        }

        private static AssignmentItemViewModel TransformAssignmentItem(AssignmentItem item)
        {
            return new AssignmentItemViewModel
            {
                AssignmentId = item.AssignmentId,
                AssignmentType = item.AssignmentType,
                TargetId = item.TargetId,
                TargetName = item.TargetName,
                TargetDetails = item.TargetDetails,
                Level = item.Level,
                LevelLabel = item.LevelLabel ?? (item.Level.HasValue ? $"Level {item.Level}" : null),
                IsMandatory = item.IsMandatory,
                AssignedDate = item.AssignedDate,
                AssignedBy = item.AssignedBy
            };
        }
    }
}
