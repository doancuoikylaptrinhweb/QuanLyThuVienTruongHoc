using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LibraryManagement.Models;
using LibraryManagement.Utilities;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoginController : Controller
    {
        private readonly DataContext _context;

        public LoginController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Nếu đã đăng nhập với quyền Admin hoặc Thủ thư, chuyển thẳng vào Dashboard
            if (User.Identity?.IsAuthenticated == true && (User.IsInRole("Admin") || User.IsInRole("Librarian")))
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(tblUser user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.UserName)) return NotFound();

            string inputUserName = user.UserName.Trim();
            string inputPassword = user.Password ?? string.Empty;

            // Tìm người dùng theo tên đăng nhập hoặc mã người dùng (AD..., TT...)
            var check = _context.Users.FirstOrDefault(u => 
                (u.UserName != null && u.UserName.ToLower() == inputUserName.ToLower()) ||
                (u.UserCode != null && u.UserCode.ToLower() == inputUserName.ToLower())
            );

            // Tự động khởi tạo tài khoản Admin mặc định nếu lần đầu đăng nhập với admin / 123456
            if (check == null && inputUserName.ToLower() == "admin" && inputPassword == "123456")
            {
                check = new tblUser
                {
                    UserName = "admin",
                    FullName = "Nguyễn Quản Trị (Admin)",
                    Email = "admin@thuvien.edu.vn",
                    Password = PasswordSecurity.HashPassword(new tblUser(), "123456"),
                    Role = "Admin",
                    IsActive = true
                };
                _context.Users.Add(check);
                _context.SaveChanges();
            }

            // Kiểm tra mật khẩu bằng PasswordHasher (PBKDF2) hoặc tương thích ngược MD5
            bool isValidPassword = false;
            bool needRehash = false;

            if (check != null)
            {
                isValidPassword = PasswordSecurity.VerifyPassword(check, check.Password, inputPassword, out needRehash);
            }

            if (check == null || !isValidPassword)
            {
                Functions._Message = "Tên đăng nhập hoặc mật khẩu không chính xác!";
                return RedirectToAction("Index", "Login");
            }

            if (check.IsActive == false)
            {
                Functions._Message = "Tài khoản này hiện đang bị khóa!";
                return RedirectToAction("Index", "Login");
            }

            // Đảm bảo tài khoản admin luôn có quyền Admin
            if (check.UserName != null && check.UserName.ToLower() == "admin" && check.Role != "Admin")
            {
                check.Role = "Admin";
                _context.SaveChanges();
            }

            // Tự động nâng cấp chuỗi băm mật khẩu cũ sang PasswordHasher mới
            if (needRehash)
            {
                try
                {
                    check.Password = PasswordSecurity.HashPassword(check, inputPassword);
                    _context.SaveChanges();
                }
                catch { }
            }

            // Thiết lập Cookie Claims cho Cookie Authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, check.UserID.ToString()),
                new Claim(ClaimTypes.Name, check.UserName ?? "Admin"),
                new Claim(ClaimTypes.GivenName, check.FullName ?? check.UserName ?? "Admin"),
                new Claim(ClaimTypes.Email, check.Email ?? "admin@thuvien.edu.vn"),
                new Claim(ClaimTypes.Role, check.Role ?? "Admin"),
                new Claim("UserCode", check.Code)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            // Cập nhật fallback để tương thích các chức năng cũ
            Functions._Message = string.Empty;
            Functions._UserID = check.UserID;
            Functions._UserName = check.FullName ?? check.UserName ?? "Admin";
            Functions._Email = check.Email ?? "admin@thuvien.edu.vn";
            Functions._Role = check.Role ?? "Admin";

            // Nếu là độc giả đăng nhập nhầm vào form admin, điều hướng ra trang chủ độc giả
            if (check.Role == "Reader")
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // Đăng nhập thành công, chuyển hướng vào Dashboard Admin
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        // Đăng xuất Cookie Authentication
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Functions.ClearSession();
            return RedirectToAction("Index", "Login", new { area = "Admin" });
        }

        // Trang thông báo từ chối quyền truy cập (Access Denied - 403)
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
