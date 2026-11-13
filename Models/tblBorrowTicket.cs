using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    [Table("tblBorrowTicket")]
    public class tblBorrowTicket
    {
        [Key]
        public int TicketID { get; set; }
        public int UserID { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
    }
}