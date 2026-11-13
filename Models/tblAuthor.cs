using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    [Table("tblAuthor")]
    public class tblAuthor
    {
        [Key]
        public int AuthorID { get; set; }
        public string? AuthorName { get; set; }
        public string? Bio { get; set; }
        public bool? IsActive { get; set; }
    }
}