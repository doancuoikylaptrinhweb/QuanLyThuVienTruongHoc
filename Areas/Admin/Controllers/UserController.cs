using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using LibraryManagement.Models;
using LibraryManagement.Utilities;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly DataContext _context;

        public UserController(DataContext context)
        {
            _context = context;
        }

        // Kiểm tra phân quyền: Chỉ tài khoản Admin mới có quyền truy cập quản lý người dùng
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (string.IsNullOrEmpty(Functions._Role) || Functions._Role != "Admin")
            {
                // Nếu chưa đăng nhập hoặc không phải Admin, chuyển hướng về trang đăng nhập
                if (!Functions.IsLogin())
                {
                    context.Result = RedirectToAction("Index", "Login");
                }
                else
                {
                    TempData["Error"] = "Bạn không có quyền truy cập chức năng Phân quyền hệ thống!";
                    context.Result = RedirectToAction("Index", "Home");
                }
            }
            base.OnActionExecuting(context);
        }

        // Tự động kiểm tra và đảm bảo có tài khoản Admin trong CSDL
        private void EnsureAdminExists()
        {
            try
            {
                var adminUser = _context.Users.FirstOrDefault(u => u.UserName == "admin");
                if (adminUser != null)
                {
                    if (adminUser.Role != "Admin" || adminUser.IsActive != true)
                    {
                        adminUser.Role = "Admin";
                        adminUser.IsActive = true;
                        _context.SaveChanges();
                    }
                }
                else if (!_context.Users.Any(u => u.Role == "Admin"))
                {
                    _context.Users.Add(new tblUser
                    {
                        UserName = "admin",
                        FullName = "Nguyễn Quản Trị (Admin)",
                        Email = "admin@thuvien.edu.vn",
                        Password = Functions.MD5Password("123456"),
                        Role = "Admin",
                        IsActive = true
                    });
                    _context.SaveChanges();
                }
            }
            catch { }
        }

        // 1. HIỂN THỊ DANH SÁCH NGƯỜI DÙNG & PHÂN QUYỀN
        public IActionResult Index(string? keyword)
        {
            EnsureAdminExists();

            var usersQuery = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                string term = keyword.Trim().ToLower();
                usersQuery = usersQuery.Where(u =>
                    (u.FullName != null && u.FullName.ToLower().Contains(term)) ||
                    (u.UserName != null && u.UserName.ToLower().Contains(term)) ||
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.Role != null && u.Role.ToLower().Contains(term))
                );
                ViewBag.Keyword = keyword;
            }

            var list = usersQuery.OrderByDescending(u => u.UserID).ToList();

            // Thống kê phân quyền
            ViewBag.TotalUsers = list.Count;
            ViewBag.TotalAdmins = list.Count(u => u.Role == "Admin");
            ViewBag.TotalLibrarians = list.Count(u => u.Role == "Librarian");
            ViewBag.TotalReaders = list.Count(u => u.Role == "Reader");

            return View(list);
        }

        // 2. TẠO TÀI KHOẢN MỚI (Admin / Thủ thư / Độc giả)
        [HttpPost]
        public IActionResult Create(tblUser user)
        {
            if (string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Password))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra trùng tên tài khoản
            if (_context.Users.Any(u => u.UserName.ToLower() == user.UserName.Trim().ToLower()))
            {
                TempData["Error"] = $"Tên tài khoản '{user.UserName}' đã tồn tại trong hệ thống!";
                return RedirectToAction(nameof(Index));
            }

            // Đặt role mặc định nếu trống
            if (string.IsNullOrEmpty(user.Role))
            {
                user.Role = "Reader";
            }

            // Mã hóa mật khẩu MD5 chuẩn
            user.Password = Functions.MD5Password(user.Password);
            user.IsActive = true;
            user.UserName = user.UserName.Trim();

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["Success"] = $"Đã tạo thành công tài khoản '{user.UserName}' với quyền '{user.Role}'!";
            return RedirectToAction(nameof(Index));
        }

        // 3. ĐỔI PHÂN QUYỀN (Role: Admin / Librarian / Reader)
        [HttpPost]
        public IActionResult ChangeRole(int id, string role)
        {
            if (id == Functions._UserID)
            {
                TempData["Error"] = "Bạn không thể tự thay đổi quyền hạn của chính tài khoản đang đăng nhập!";
                return RedirectToAction(nameof(Index));
            }

            if (role != "Admin" && role != "Librarian" && role != "Reader")
            {
                TempData["Error"] = "Quyền hạn không hợp lệ!";
                return RedirectToAction(nameof(Index));
            }

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            user.Role = role;
            _context.SaveChanges();

            TempData["Success"] = $"Đã cập nhật quyền của tài khoản '{user.UserName}' thành: {role}!";
            return RedirectToAction(nameof(Index));
        }

        // 4. KHÓA / MỞ KHÓA TÀI KHOẢN
        [HttpPost]
        public IActionResult ToggleActive(int id)
        {
            if (id == Functions._UserID)
            {
                TempData["Error"] = "Bạn không thể tự khóa tài khoản của chính mình!";
                return RedirectToAction(nameof(Index));
            }

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            user.IsActive = !(user.IsActive ?? false);
            _context.SaveChanges();

            string statusText = (user.IsActive == true) ? "Mở khóa thành công" : "Đã khóa tài khoản";
            TempData["Success"] = $"{statusText} cho người dùng '{user.UserName}'!";
            return RedirectToAction(nameof(Index));
        }

        // 5. ĐẶT LẠI MẬT KHẨU (Reset Password về mặc định 123456)
        [HttpPost]
        public IActionResult ResetPassword(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            user.Password = Functions.MD5Password("123456");
            _context.SaveChanges();

            TempData["Success"] = $"Đã đặt lại mật khẩu cho '{user.UserName}' về mặc định là: 123456";
            return RedirectToAction(nameof(Index));
        }

        // 6. XÓA TÀI KHOẢN
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (id == Functions._UserID)
            {
                TempData["Error"] = "Không thể xóa tài khoản của chính mình đang đăng nhập!";
                return RedirectToAction(nameof(Index));
            }

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            // Kiểm tra xem người dùng có phiếu mượn nào không
            if (_context.BorrowTickets.Any(t => t.UserID == id))
            {
                TempData["Error"] = $"Không thể xóa tài khoản '{user.UserName}' vì đang có lịch sử phiếu mượn sách. Hãy chọn chức năng Khóa tài khoản!";
                return RedirectToAction(nameof(Index));
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            TempData["Success"] = $"Đã xóa vĩnh viễn tài khoản '{user.UserName}' khỏi hệ thống!";
            return RedirectToAction(nameof(Index));
        }
    }
}
