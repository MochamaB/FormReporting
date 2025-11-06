namespace FormReporting.Models.Common
{
    /// <summary>
    /// Interface for entities that can be activated or deactivated
    /// </summary>
    public interface IActivatable
    {
        /// <summary>
        /// Indicates if the record is currently active
        /// </summary>
        bool IsActive { get; set; }
    }
}
