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
            if (user == null) return NotFound();

            // Kiểm tra trùng tên đăng nhập (Username)
            var check = _context.Users.FirstOrDefault(u => u.UserName == user.UserName);
            if (check != null)
            {
                Functions._Message = "Tên đăng nhập này đã tồn tại, vui lòng chọn tên khác!";
                return RedirectToAction("Index", "Register");
            }

            // Mã hóa mật khẩu MD5 (Lab 10 trang 4)
            Functions._Message = string.Empty;
            user.Password = Functions.MD5Password(user.Password);
            user.Role = "Reader"; // Mặc định là Bạn đọc (hoặc "Admin" nếu muốn cấp quyền quản trị)
            user.IsActive = true;

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Index", "Login");
        }
    }
}