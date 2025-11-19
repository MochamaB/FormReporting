namespace FormReporting.Services.Identity
{
    /// <summary>
    /// Represents a user's complete scope information
    /// </summary>
    public class UserScope
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Scope level ID
        /// </summary>
        public int ScopeLevelId { get; set; }

        /// <summary>
        /// Scope name (Global, Regional, Tenant, etc.)
        /// </summary>
        public string ScopeName { get; set; } = string.Empty;

        /// <summary>
        /// Scope code (GLOBAL, REGIONAL, TENANT, etc.)
        /// </summary>
        public string ScopeCode { get; set; } = string.Empty;

        /// <summary>
        /// Hierarchical level (1=Global, 2=Regional, 3=Tenant, etc.)
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Primary tenant ID
        /// </summary>
        public int? PrimaryTenantId { get; set; }

        /// <summary>
        /// Region ID
        /// </summary>
        public int? RegionId { get; set; }

        /// <summary>
        /// Department ID
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// List of tenant IDs the user can access (scope + exceptions)
        /// </summary>
        public List<int> AccessibleTenantIds { get; set; } = new List<int>();

        /// <summary>
        /// List of tenant IDs from UserTenantAccess exceptions
        /// </summary>
        public List<int> TenantAccessExceptions { get; set; } = new List<int>();

        /// <summary>
        /// List of role names
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// List of permission codes
        /// </summary>
        public List<string> Permissions { get; set; } = new List<string>();
    }
}
