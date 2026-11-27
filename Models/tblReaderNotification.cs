using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    [Table("tblReaderNotification")]
    public class tblReaderNotification
    {
        [Key]
        public int NotificationID { get; set; }

        public int UserID { get; set; }

        public int TicketID { get; set; }

        public int? WarningID { get; set; }

        [StringLength(50)]
        public string TicketCode { get; set; } = string.Empty;

        [StringLength(1000)]
        public string BookTitles { get; set; } = string.Empty;

        public int OverdueDays { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(30)]
        public string SeverityLevel { get; set; } = "Nhắc nhở";

        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1500)]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime? ReadDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? CreatedBy { get; set; }
    }
}
