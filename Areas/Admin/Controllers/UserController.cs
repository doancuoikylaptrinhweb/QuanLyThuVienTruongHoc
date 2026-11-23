using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using LibraryManagement.Models;
using LibraryManagement.Utilities;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Kiểm tra quyền ở cấp CONTROLLER: Chỉ Admin tối cao mới được vào Quản lý tài khoản
    public class UserController : AdminBaseController
    {
        private readonly DataContext _context;

        public UserController(DataContext context)
        {
            _context = context;
        }

        // Kiểm tra phân quyền bổ sung qua bộ lọc
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Result = RedirectToAction("Index", "Login", new { area = "Admin" });
            }
            else if (!user.IsInRole("Admin"))
            {
                TempData["Error"] = "Bạn không có quyền truy cập chức năng Phân quyền hệ thống (chỉ dành cho Admin)!";
                context.Result = RedirectToAction("AccessDenied", "Login", new { area = "Admin" });
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
                    var newAdmin = new tblUser
                    {
                        UserName = "admin",
                        FullName = "Nguyễn Quản Trị (Admin)",
                        Email = "admin@thuvien.edu.vn",
                        Role = "Admin",
                        IsActive = true
                    };
                    newAdmin.Password = PasswordSecurity.HashPassword(newAdmin, "123456");
                    _context.Users.Add(newAdmin);
                    _context.SaveChanges();
                }
            }
            catch { }
        }

        // 1. HIỂN THỊ DANH SÁCH NGƯỜI DÙNG & PHÂN QUYỀN
        [HttpGet]
        [Authorize(Roles = "Admin")] // Kiểm tra quyền ở cấp ENDPOINT
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
                    (u.Role != null && u.Role.ToLower().Contains(term)) ||
                    (u.UserCode != null && u.UserCode.ToLower().Contains(term))
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
        [Authorize(Roles = "Admin")] // Kiểm tra quyền ở cấp ENDPOINT
        public IActionResult Create(tblUser user)
        {
            if (string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Password))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra trùng tên tài khoản
            if (_context.Users.Any(u => u.UserName != null && u.UserName.ToLower() == user.UserName.Trim().ToLower()))
            {
                TempData["Error"] = $"Tên tài khoản '{user.UserName}' đã tồn tại trong hệ thống!";
                return RedirectToAction(nameof(Index));
            }

            // Đặt role mặc định nếu trống
            if (string.IsNullOrEmpty(user.Role))
            {
                user.Role = "Reader";
            }

            // Băm mật khẩu an toàn bằng PasswordHasher (PBKDF2 HMAC-SHA256 có Salt ngẫu nhiên)
            user.Password = PasswordSecurity.HashPassword(user, user.Password);
            user.IsActive = true;
            user.UserName = user.UserName.Trim();

            _context.Users.Add(user);
            _context.SaveChanges();

            // Tự động gán Mã người dùng chuẩn nếu chưa nhập
            if (string.IsNullOrWhiteSpace(user.UserCode))
            {
                user.UserCode = user.Role == "Admin" ? $"AD{user.UserID:D4}" : (user.Role == "Librarian" ? $"TT{user.UserID:D4}" : $"DG{user.UserID:D4}");
                _context.Users.Update(user);
                _context.SaveChanges();
            }

            TempData["Success"] = $"Đã tạo thành công tài khoản [{user.Code}] '{user.UserName}' với quyền '{user.Role}'!";
            return RedirectToAction(nameof(Index));
        }

        // 3. ĐỔI PHÂN QUYỀN (Role: Admin / Librarian / Reader)
        [HttpPost]
        [Authorize(Roles = "Admin")] // Kiểm tra quyền ở cấp ENDPOINT
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
        [Authorize(Roles = "Admin")] // Kiểm tra quyền ở cấp ENDPOINT
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

        // 5. ĐẶT LẠI MẬT KHẨU (Reset Password về mặc định 123456 bằng PasswordHasher)
        [HttpPost]
        [Authorize(Roles = "Admin")] // Kiểm tra quyền ở cấp ENDPOINT
        public IActionResult ResetPassword(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            user.Password = PasswordSecurity.HashPassword(user, "123456");
            _context.SaveChanges();

            TempData["Success"] = $"Đã đặt lại mật khẩu cho '{user.UserName}' về mặc định an toàn là: 123456";
            return RedirectToAction(nameof(Index));
        }

        // 6. XÓA TÀI KHOẢN
        [HttpPost]
        [Authorize(Roles = "Admin")] // Kiểm tra quyền ở cấp ENDPOINT
        public IActionResult Delete(int id)
        {
            if (id == Functions._UserID)
            {
                TempData["Error"] = "Không thể xóa tài khoản của chính mình đang đăng nhập!";
                return RedirectToAction(nameof(Index));
            }

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            // QUY TẮC BẢO TOÀN DỮ LIỆU:
            // 1. Nếu đang có phiếu mượn chưa trả -> TUYỆT ĐỐI CHẶN XÓA
            bool hasUnreturnedTickets = _context.BorrowTickets.Any(t => t.UserID == id && t.Status != "Đã trả");
            if (hasUnreturnedTickets)
            {
                TempData["Error"] = $"Không thể xóa tài khoản '{user.UserName}' vì đang có sách mượn chưa hoàn kho! Vui lòng thu hồi sách trước.";
                return RedirectToAction(nameof(Index));
            }

            // 2. Nếu có lịch sử phiếu mượn cũ -> ÁP DỤNG SOFT DELETE ĐỂ BẢO TOÀN DỮ LIỆU
            bool hasHistory = _context.BorrowTickets.Any(t => t.UserID == id);
            if (hasHistory)
            {
                user.IsActive = false; // Xóa mềm: Khóa tài khoản
                _context.SaveChanges();
                TempData["Success"] = $"Tài khoản '{user.UserName}' đã có lịch sử mượn trả trong thư viện. Để bảo toàn toàn vẹn dữ liệu phiếu mượn, hệ thống đã chuyển sang chế độ XÓA MỀM (Khóa tài khoản)!";
                return RedirectToAction(nameof(Index));
            }

            // 3. Nếu chưa từng phát sinh phiếu mượn -> Xóa cứng an toàn
            _context.Users.Remove(user);
            _context.SaveChanges();

            TempData["Success"] = $"Đã xóa vĩnh viễn tài khoản '{user.UserName}' khỏi hệ thống!";
            return RedirectToAction(nameof(Index));
        }
    }
}
