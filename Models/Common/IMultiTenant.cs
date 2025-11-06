namespace FormReporting.Models.Common
{
    /// <summary>
    /// Interface for entities that belong to a specific tenant (multi-tenancy support)
    /// </summary>
    public interface IMultiTenant
    {
        /// <summary>
        /// The tenant (factory/subsidiary) this record belongs to
        /// </summary>
        int TenantId { get; set; }
    }
}
