using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;
using LibraryManagement.Utilities;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RegisterController : Controller
    {
        private readonly DataContext _context;

        public RegisterController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(tblUser user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Password))
            {
                Functions._Message = "Vui lòng nhập đầy đủ thông tin!";
                return RedirectToAction("Index", "Register");
            }

            // Kiểm tra trùng tên đăng nhập (Username)
            var check = _context.Users.FirstOrDefault(u => u.UserName != null && u.UserName.ToLower() == user.UserName.Trim().ToLower());
            if (check != null)
            {
                Functions._Message = "Tên đăng nhập này đã tồn tại, vui lòng chọn tên khác!";
                return RedirectToAction("Index", "Register");
            }

            // Băm mật khẩu bằng PasswordHasher an toàn (PBKDF2 HMAC-SHA256 có Salt)
            Functions._Message = string.Empty;
            user.Password = PasswordSecurity.HashPassword(user, user.Password);
            user.Role = "Reader"; // Mặc định là Bạn đọc
            user.IsActive = true;
            user.UserName = user.UserName.Trim();

            _context.Users.Add(user);
            _context.SaveChanges();

            // Tự động gán Mã bạn đọc chuẩn: DG0001
            if (string.IsNullOrWhiteSpace(user.UserCode))
            {
                user.UserCode = $"DG{user.UserID:D4}";
                _context.Users.Update(user);
                _context.SaveChanges();
            }

            TempData["Success"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Index", "Login");
        }
    }
}
