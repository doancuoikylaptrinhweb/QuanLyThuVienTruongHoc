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
            if (user == null || string.IsNullOrWhiteSpace(user.UserName)) return NotFound();

            // Mã hóa mật khẩu người dùng vừa nhập để so sánh với CSDL
            string pw = Functions.MD5Password(user.Password);

            // Kiểm tra tài khoản và mật khẩu (Hỗ trợ cả chuỗi băm MD5 và chuỗi thường)
            var check = _context.Users.FirstOrDefault(u => 
                u.UserName == user.UserName && 
                (u.Password == pw || u.Password == user.Password)
            );

            // Tự động tạo / cập nhật tài khoản Admin nếu đăng nhập với admin/123456
            if (check == null && user.UserName.Trim().ToLower() == "admin" && user.Password == "123456")
            {
                check = new tblUser
                {
                    UserName = "admin",
                    FullName = "Nguyễn Quản Trị (Admin)",
                    Email = "admin@thuvien.edu.vn",
                    Password = pw,
                    Role = "Admin",
                    IsActive = true
                };
                _context.Users.Add(check);
                _context.SaveChanges();
            }

            if (check == null)
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
            if (check.UserName.ToLower() == "admin" && check.Role != "Admin")
            {
                check.Role = "Admin";
                _context.SaveChanges();
            }

            // Lưu thông tin đăng nhập vào các biến tĩnh
            Functions._Message = string.Empty;
            Functions._UserID = check.UserID;
            Functions._UserName = check.FullName ?? check.UserName ?? "Admin";
            Functions._Email = check.Email ?? "admin@thuvien.edu.vn";
            Functions._Role = check.Role ?? "Admin";

            // Đăng nhập thành công, chuyển hướng vào Dashboard Admin
            return RedirectToAction("Index", "Home");
        }
    }
}
