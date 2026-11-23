using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    [Table("tblBook")]
    public class tblBook
    {
        [Key]
        public long BookID { get; set; }
        public string? BookCode { get; set; } // Mã sách: MS0001, MS0002...
        public string? Title { get; set; }
        public int CategoryID { get; set; }
        public int AuthorID { get; set; }
        public int PublisherID { get; set; }
        public int PublishYear { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }
        public string? Images { get; set; }
        public string? Summary { get; set; }
        public string? Contents { get; set; }
        public int PostOrder { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsActive { get; set; }

        // Mã sách hiển thị chuẩn hóa
        [NotMapped]
        public string Code => !string.IsNullOrWhiteSpace(BookCode)
            ? BookCode
            : $"MS{BookID:D4}";

        // Tiêu đề sách kèm mã sách
        [NotMapped]
        public string DisplayTitle => $"[{Code}] {Title}";
    }
}