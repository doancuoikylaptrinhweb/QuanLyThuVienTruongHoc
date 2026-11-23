using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    [Table("tblBorrowTicket")]
    public class tblBorrowTicket
    {
        [Key]
        public int TicketID { get; set; }
        public string? TicketCode { get; set; } // Mã phiếu mượn: PM0001, PM-202611-001
        public string? ReturnCode { get; set; } // Mã trả sách / Mã phiếu trả: PT0001, PT-202611-001
        public int UserID { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string? Status { get; set; } // "Đang mượn", "Đã trả", "Quá hạn"
        public string? Note { get; set; }

        // Mã phiếu mượn hiển thị
        [NotMapped]
        public string Code => !string.IsNullOrWhiteSpace(TicketCode)
            ? TicketCode
            : $"PM{TicketID:D4}";

        // Mã trả sách hiển thị
        [NotMapped]
        public string DisplayReturnCode => !string.IsNullOrWhiteSpace(ReturnCode)
            ? ReturnCode
            : (Status == "Đã trả" ? $"PT{TicketID:D4}" : "---");
    }
}