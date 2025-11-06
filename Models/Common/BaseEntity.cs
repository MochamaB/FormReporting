namespace FormReporting.Models.Common
{
    /// <summary>
    /// Base entity class for all database entities
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Date and time when the record was created (UTC)
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date and time when the record was last modified (UTC)
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
    }
}
