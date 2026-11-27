using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using SlugGenerator;

namespace LibraryManagement.Utilities
{
    public class Functions
    {
        private static IHttpContextAccessor? _httpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Biến static lưu trữ fallback (nếu chưa dùng claims)
        private static int _fallbackUserID = 0;
        private static string _fallbackUserName = string.Empty;
        private static string _fallbackEmail = string.Empty;
        private static string _fallbackRole = string.Empty;
        public static string _Message = string.Empty;

        // 1. Thuộc tính động lấy trực tiếp từ Cookie Claims của request hiện tại (tránh race-condition giữa nhiều user)
        public static int _UserID
        {
            get
            {
                var user = _httpContextAccessor?.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(idClaim, out int uid)) return uid;
                }
                return _fallbackUserID;
            }
            set => _fallbackUserID = value;
        }

        public static string _UserName
        {
            get
            {
                var user = _httpContextAccessor?.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    var name = user.FindFirst(ClaimTypes.GivenName)?.Value 
                               ?? user.FindFirst(ClaimTypes.Name)?.Value;
                    if (!string.IsNullOrEmpty(name)) return name;
                }
                return _fallbackUserName;
            }
            set => _fallbackUserName = value;
        }

        public static string _Email
        {
            get
            {
                var user = _httpContextAccessor?.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    var email = user.FindFirst(ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(email)) return email;
                }
                return _fallbackEmail;
            }
            set => _fallbackEmail = value;
        }

        public static string _Role
        {
            get
            {
                var user = _httpContextAccessor?.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    var role = user.FindFirst(ClaimTypes.Role)?.Value;
                    if (!string.IsNullOrEmpty(role)) return role;
                }
                return _fallbackRole;
            }
            set => _fallbackRole = value;
        }

        // 2. Hàm sinh URL thân thiện cho chi tiết sách
        public static string TitlesSlugGeneration(string type, string? title, long id)
        {
            return type + "-" + SlugGenerator.SlugGenerator.GenerateSlug(title) + "-" + id.ToString() + ".html";
        }

        // 3. Hàm lấy ngày giờ hiện tại chuẩn định dạng
        public static string GetCurrentDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        // 4. Các hàm mã hóa MD5 (dùng cho tương thích ngược)
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

        // 5. Hàm kiểm tra đã đăng nhập chưa (ưu tiên kiểm tra Cookie Auth của request)
        public static bool IsLogin()
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                return true;
            }
            return _fallbackUserID > 0 && !string.IsNullOrEmpty(_fallbackUserName);
        }

        // 6. Hàm kiểm tra quyền Quản trị (Admin hoặc Thủ thư)
        public static bool IsAdmin()
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                return user.IsInRole("Admin") || user.IsInRole("Librarian");
            }
            return IsLogin() && (_fallbackRole == "Admin" || _fallbackRole == "Librarian");
        }

        // 7. Hàm xóa phiên đăng nhập (Đăng xuất)
        public static void ClearSession()
        {
            _fallbackUserID = 0;
            _fallbackUserName = string.Empty;
            _fallbackEmail = string.Empty;
            _fallbackRole = string.Empty;
            _Message = string.Empty;
        }
        // 8. Chuẩn hóa đường dẫn ảnh bìa sách để hoạt động đúng ở mọi route (/, /Lookup, /Book/Detail/{id}, /Admin/...)
        public static string ResolveBookImage(string? imagePath)
        {
            const string defaultCover = "/images/books/clean-code.svg";
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return defaultCover;
            }

            string trimmed = imagePath.Trim();
            if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("/"))
            {
                return trimmed;
            }

            if (trimmed.StartsWith("~/"))
            {
                return "/" + trimmed.Substring(2);
            }

            return "/" + trimmed.TrimStart('/');
        }
    }
}
