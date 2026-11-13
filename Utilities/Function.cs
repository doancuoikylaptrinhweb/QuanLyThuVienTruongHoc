using System.Security.Cryptography;
using System.Text;
using SlugGenerator;

namespace LibraryManagement.Utilities
{
    public class Functions
    {
        // 1. Các biến tĩnh lưu phiên đăng nhập (Lab 10 trang 3)
        public static int _UserID = 0;
        public static string _UserName = string.Empty;
        public static string _Email = string.Empty;
        public static string _Role = string.Empty;      // "Admin", "Librarian", "Reader"
        public static string _Message = string.Empty;

        // 2. Hàm sinh URL thân thiện cho chi tiết sách (Lab 05 trang 2)
        public static string TitlesSlugGeneration(string type, string? title, long id)
        {
            return type + "-" + SlugGenerator.SlugGenerator.GenerateSlug(title) + "-" + id.ToString() + ".html";
        }

        // 3. Hàm lấy ngày giờ hiện tại chuẩn định dạng (Lab 08 trang 6)
        public static string GetCurrentDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        // 4. Các hàm mã hóa mật khẩu MD5 (Lab 10 trang 3)
        public static string MD5Hash(string text)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] result = md5.ComputeHash(Encoding.ASCII.GetBytes(text));
                StringBuilder strBuilder = new StringBuilder();
                for (int i = 0; i < result.Length; i++)
                {
                    strBuilder.Append(result[i].ToString("x2"));
                }
                return strBuilder.ToString();
            }
        }

        public static string MD5Password(string? text)
        {
            string str = MD5Hash(text ?? "");
            for (int i = 0; i <= 5; i++)
            {
                str = MD5Hash(str + text);
            }
            return str;
        }

        // 5. Hàm kiểm tra đã đăng nhập chưa (Lab 10 trang 11)
        public static bool IsLogin()
        {
            if (_UserID <= 0 || string.IsNullOrEmpty(_UserName))
            {
                return false;
            }
            return true;
        }
    }
}