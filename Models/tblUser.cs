using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.Models
{
    [Table("tblUser")]
    public class tblUser
    {
        [Key]
        public int UserID { get; set; }
        public string? UserCode { get; set; } // Mã người dùng / Mã bạn đọc: DG0001, TT0001, AD0001
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; } // "Admin", "Librarian", "Reader"
        public bool? IsActive { get; set; }

        // Mã hiển thị chuẩn hóa
        [NotMapped]
        public string Code => !string.IsNullOrWhiteSpace(UserCode)
            ? UserCode
            : (Role == "Admin" ? $"AD{UserID:D4}" : (Role == "Librarian" ? $"TT{UserID:D4}" : $"DG{UserID:D4}"));

        // Tên hiển thị kèm mã
        [NotMapped]
        public string DisplayName => $"[{Code}] {FullName ?? UserName}";
    }
}