using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    [Table("tblUser")]
    public class tblUser
    {
        [Key]
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; } // "Admin", "Librarian", "Reader"
        public bool? IsActive { get; set; }
    }
}