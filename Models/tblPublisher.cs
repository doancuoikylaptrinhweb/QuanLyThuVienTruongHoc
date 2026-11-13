using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    [Table("tblPublisher")]
    public class tblPublisher
    {
        [Key]
        public int PublisherID { get; set; }
        public string? PublisherName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
    }
}