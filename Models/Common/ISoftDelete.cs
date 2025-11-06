namespace FormReporting.Models.Common
{
    /// <summary>
    /// Interface for entities that support soft delete (logical deletion)
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>
        /// Indicates if the record has been soft deleted
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// Date and time when the record was soft deleted (UTC)
        /// </summary>
        DateTime? DeletedDate { get; set; }

        /// <summary>
        /// User ID of the person who deleted this record
        /// </summary>
        int? DeletedBy { get; set; }
    }
}
