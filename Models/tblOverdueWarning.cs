using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    [Table("tblOverdueWarning")]
    public class tblOverdueWarning
    {
        [Key]
        public int WarningID { get; set; }

        public int TicketID { get; set; }

        public int UserID { get; set; }

        public int OverdueDays { get; set; }

        /// <summary>
        /// Mức độ quá hạn: "Nhắc nhở" (1-3 ngày), "Cảnh báo" (4-7 ngày), "Nghiêm trọng" (> 7 ngày)
        /// </summary>
        public string SeverityLevel { get; set; } = "Nhắc nhở";

        public int OverdueBookCount { get; set; }

        public string? BookTitles { get; set; }

        /// <summary>
        /// Trạng thái xử lý: "Chưa nhắc nhở", "Đã nhắc nhở", "Đã trả sách"
        /// </summary>
        public string WarningStatus { get; set; } = "Chưa nhắc nhở";

        public int ReminderCount { get; set; } = 0;

        public DateTime? LastRemindedDate { get; set; }

        public string? RemindedBy { get; set; }

        public string? ReminderNote { get; set; }

        public DateTime? ResolvedDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
