using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormReporting.Models.Entities.Identity;

namespace FormReporting.Models.Entities.Tickets
{
    /// <summary>
    /// Comments and notes on support tickets
    /// </summary>
    [Table("TicketComments")]
    public class TicketComment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Comment { get; set; } = string.Empty;

        public bool IsInternal { get; set; } = false; // Internal notes vs customer-visible

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(TicketId))]
        public virtual Ticket Ticket { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}
