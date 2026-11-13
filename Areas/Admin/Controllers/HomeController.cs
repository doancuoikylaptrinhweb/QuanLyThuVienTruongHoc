using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;
using LibraryManagement.Utilities;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly DataContext _context;

        public HomeController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // 1. Kiểm tra trạng thái đăng nhập (Lab 10 trang 11)
            if (!Functions.IsLogin())
            {
                return RedirectToAction("Index", "Login");
            }

            // 2. Các số liệu thống kê tổng hợp (Mục 10 đồ án)
            ViewBag.TotalBooks = _context.Books.Count();
            ViewBag.TotalBorrowing = _context.BorrowTickets.Count(t => t.Status == "Đang mượn");
            ViewBag.TotalUsers = _context.Users.Count(u => u.Role == "Reader");

            // 3. Thống kê số lượng sách theo từng thể loại để vẽ biểu đồ
            var categoryStats = _context.Categories
                .Where(c => c.IsActive == true)
                .Select(c => new
                {
                    Name = c.CategoryName,
                    Count = _context.Books.Count(b => b.CategoryID == c.CategoryID)
                }).ToList();

            ViewBag.ChartLabels = string.Join(",", categoryStats.Select(c => $"'{c.Name}'"));
            ViewBag.ChartData = string.Join(",", categoryStats.Select(c => c.Count));

            // 4. Danh sách 5 phiếu mượn mới nhất
            var recentBorrows = _context.BorrowTickets
                .OrderByDescending(t => t.TicketID)
                .Take(5)
                .ToList();

            ViewBag.Users = _context.Users.ToDictionary(u => u.UserID, u => u.FullName ?? u.UserName);

            return View(recentBorrows);
        }

        public IActionResult Logout()
        {
            Functions._UserID = 0;
            Functions._UserName = string.Empty;
            Functions._Email = string.Empty;
            Functions._Role = string.Empty;
            Functions._Message = string.Empty;

            return RedirectToAction("Index", "Login");
        }
    }
}