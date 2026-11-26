using Microsoft.AspNetCore.Identity;
using LibraryManagement.Models;

namespace LibraryManagement.Utilities
{
    /// <summary>
    /// Cơ chế băm và xác thực mật khẩu an toàn chuẩn Microsoft ASP.NET Core Identity (PBKDF2 HMAC-SHA256, Per-User Salt, 10,000+ iterations).
    /// Hỗ trợ kiểm tra tương thích ngược với mật khẩu MD5 cũ và tự động re-hash khi người dùng đăng nhập.
    /// </summary>
    public static class PasswordSecurity
    {
        public const string GoogleOAuthPlaceholder = "__GOOGLE_OAUTH_NO_PASSWORD__";
        private static readonly PasswordHasher<tblUser> _hasher = new PasswordHasher<tblUser>();

        /// <summary>
        /// Băm mật khẩu bằng PBKDF2 chuẩn an toàn cấp công nghiệp.
        /// </summary>
        public static string HashPassword(tblUser user, string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;
            return _hasher.HashPassword(user, password);
        }

        /// <summary>
        /// Xác thực mật khẩu:
        /// 1. Kiểm tra với chuẩn băm PasswordHasher mới.
        /// 2. Nếu là chuỗi băm MD5 cũ hoặc plain-text cũ, xác nhận thành công và bật cờ needRehash = true để tự động nâng cấp mật khẩu.
        /// </summary>
        public static bool VerifyPassword(tblUser user, string? storedHash, string providedPassword, out bool needRehash)
        {
            needRehash = false;

            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(providedPassword) || storedHash == GoogleOAuthPlaceholder)
            {
                return false;
            }

            // 1. Kiểm tra với chuẩn PasswordHasher (PBKDF2)
            try
            {
                var result = _hasher.VerifyHashedPassword(user, storedHash, providedPassword);
                if (result == PasswordVerificationResult.Success)
                {
                    return true;
                }
                if (result == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    needRehash = true;
                    return true;
                }
            }
            catch
            {
                // Chuỗi hash không theo định dạng Identity PasswordHasher (có thể là MD5 cũ)
            }

            // 2. Kiểm tra tương thích ngược với mật khẩu MD5 cũ hoặc chuỗi thường
            string oldMd5 = Functions.MD5Password(providedPassword);
            string rawMd5 = Functions.MD5Hash(providedPassword);

            if (storedHash.Equals(oldMd5, StringComparison.OrdinalIgnoreCase) ||
                storedHash.Equals(rawMd5, StringComparison.OrdinalIgnoreCase) ||
                storedHash.Equals(providedPassword))
            {
                // Mật khẩu đúng theo chuẩn cũ -> kích hoạt tự động nâng cấp sang PasswordHasher
                needRehash = true;
                return true;
            }

            return false;
        }
    }
}
