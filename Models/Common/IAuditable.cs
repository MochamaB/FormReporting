namespace FormReporting.Models.Common
{
    /// <summary>
    /// Interface for entities that require audit trail (who created/modified)
    /// </summary>
    public interface IAuditable
    {
        /// <summary>
        /// User ID of the person who created this record
        /// </summary>
        int? CreatedBy { get; set; }

        /// <summary>
        /// User ID of the person who last modified this record
        /// </summary>
        int? ModifiedBy { get; set; }
    }
}
