using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    [Table("tblBorrowDetail")]
    public class tblBorrowDetail
    {
        [Key]
        public int DetailID { get; set; }
        public int TicketID { get; set; }
        public long BookID { get; set; }
        public decimal FineAmount { get; set; }
        public bool IsReturned { get; set; } = false; // Trạng thái đã hoàn kho đúng một lần
        public DateTime? ReturnDate { get; set; } // Ngày hoàn kho của cuốn sách này
        public string? Note { get; set; } // Tình trạng sách khi trả hoặc ghi chú
    }
}