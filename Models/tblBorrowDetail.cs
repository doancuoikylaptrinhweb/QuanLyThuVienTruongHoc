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
    }
}