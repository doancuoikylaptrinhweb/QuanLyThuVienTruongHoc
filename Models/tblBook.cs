using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    [Table("tblBook")]
    public class tblBook
    {
        [Key]
        public long BookID { get; set; }
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
    }
}