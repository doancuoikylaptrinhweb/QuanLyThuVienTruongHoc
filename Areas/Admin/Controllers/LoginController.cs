using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(tblUser user)
        {
            if (user == null) return NotFound();

            // Mã hóa mật khẩu người dùng vừa nhập để so sánh với CSDL
            string pw = Functions.MD5Password(user.Password);

            // Kiểm tra tài khoản và mật khẩu
            var check = _context.Users.FirstOrDefault(u => u.UserName == user.UserName && u.Password == pw);

            if (check == null)
            {
                Functions._Message = "Tên đăng nhập hoặc mật khẩu không chính xác!";
                return RedirectToAction("Index", "Login");
            }
            // nghĩa thêm vào
            if (check.IsActive == false)
            {
                Functions._Message = "Tài khoản đã bị khóa!";
                return RedirectToAction("Index", "Login");
            }

            // Lưu thông tin đăng nhập vào các biến tĩnh (Lab 10 trang 8)
            Functions._Message = string.Empty;
            Functions._UserID = check.UserID;
            Functions._UserName = check.FullName ?? check.UserName ?? string.Empty;
            Functions._Email = check.Email ?? string.Empty;
            Functions._Role = check.Role ?? "Reader";

            // Đăng nhập thành công, chuyển hướng vào Dashboard Admin
            return RedirectToAction("Index", "Home");
        }
    }
}