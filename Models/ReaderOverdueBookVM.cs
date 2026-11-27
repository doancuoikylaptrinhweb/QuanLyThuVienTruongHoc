namespace LibraryManagement.Models
{
    public class ReaderOverdueBookVM
    {
        public int TicketID { get; set; }
        public string TicketCode { get; set; } = string.Empty;
        public string TicketStatus { get; set; } = string.Empty;
        public long BookID { get; set; }
        public string BookCode { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public int OverdueDays { get; set; }
        public string SeverityLevel { get; set; } = "Nhắc nhở";
        public bool CanRequestReturn { get; set; }
    }
}
