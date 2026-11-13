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
        public UserController(DataContext context) => _context = context;

        // Chỉ Admin được vào
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (Functions._Role != "Admin")
                context.Result = RedirectToAction("Index", "Login");
            base.OnActionExecuting(context);
        }

        public IActionResult Index(string? keyword)
        {
            var users = _context.Users.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
                users = users.Where(u => u.FullName!.Contains(keyword)
                                    || u.UserName!.Contains(keyword)
                                    || u.Email!.Contains(keyword));
            ViewBag.Keyword = keyword;
            return View(users.OrderByDescending(u => u.UserID).ToList());
        }

        [HttpPost]
        public IActionResult ToggleActive(int id)
        {
            if (id == Functions._UserID)
            {
                TempData["Error"] = "Không thể tự khóa tài khoản của mình!";
                return RedirectToAction(nameof(Index));
            }
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();
            user.IsActive = !(user.IsActive ?? false);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ChangeRole(int id, string role)
        {
            if (id == Functions._UserID)
            {
                TempData["Error"] = "Không thể tự đổi quyền của mình!";
                return RedirectToAction(nameof(Index));
            }
            if (role != "Admin" && role != "Librarian" && role != "Reader")
                return BadRequest();
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();
            user.Role = role;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}