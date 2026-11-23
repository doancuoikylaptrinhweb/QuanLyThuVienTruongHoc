using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;
using LibraryManagement.Utilities;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Librarian")] // Phân quyền cấp CONTROLLER
    public class HomeController : AdminBaseController
    {
        private readonly DataContext _context;

        public HomeController(DataContext context)
        {
            _context = context;
        }

        // BẢNG ĐIỀU KHIỂN QUẢN TRỊ VIÊN: DỮ LIỆU 100% TRUY VẤN THỰC TẾ TỪ CSDL
        public IActionResult Index()
        {
            // 1. Thống kê số liệu thời gian thực từ Database
            int bookCount = _context.Books.Count();
            int copyCount = _context.Books.Sum(b => (int?)b.Quantity) ?? 0;
            int borrowingCount = _context.BorrowTickets.Count(t => t.Status == "Đang mượn" || t.Status == "Trả một phần");
            int overdueCount = _context.BorrowTickets.Count(t => (t.Status == "Đang mượn" || t.Status == "Trả một phần") && t.DueDate < DateTime.Now);
            int pendingCount = _context.BorrowTickets.Count(t => t.Status == "Chờ duyệt");

            ViewBag.TotalBooks = bookCount;
            ViewBag.TotalCopies = copyCount;
            ViewBag.TotalBorrowing = borrowingCount;
            ViewBag.TotalOverdue = overdueCount;
            ViewBag.TotalPending = pendingCount;

            // 2. Dữ liệu biểu đồ Xu hướng Mượn & Trả (6 tháng gần nhất từ Database)
            var trendMonths = new List<string>();
            var borrowCounts = new List<int>();
            var returnCounts = new List<int>();

            DateTime now = DateTime.Now;
            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = now.AddMonths(-i);
                int year = targetMonth.Year;
                int month = targetMonth.Month;

                trendMonths.Add($"T{month}/{year}");

                int bCount = _context.BorrowTickets.Count(t => t.BorrowDate.Year == year && t.BorrowDate.Month == month);
                int rCount = _context.BorrowTickets.Count(t => t.ReturnDate.HasValue && t.ReturnDate.Value.Year == year && t.ReturnDate.Value.Month == month);

                borrowCounts.Add(bCount);
                returnCounts.Add(rCount);
            }

            ViewBag.TrendMonths = trendMonths.ToArray();
            ViewBag.BorrowTrend = borrowCounts.ToArray();
            ViewBag.ReturnTrend = returnCounts.ToArray();

            // 3. Dữ liệu biểu đồ Cơ cấu Thể loại Sách (100% từ Database)
            var categories = _context.Categories.Where(c => c.IsActive == true).ToList();
            var categoryLabels = new List<string>();
            var categoryData = new List<int>();

            foreach (var cat in categories)
            {
                int count = _context.Books.Count(b => b.CategoryID == cat.CategoryID);
                if (count > 0 || categories.Count <= 6)
                {
                    categoryLabels.Add(cat.CategoryName ?? "Khác");
                    categoryData.Add(count);
                }
            }

            // Nếu DB chưa có thể loại nào
            if (categoryLabels.Count == 0)
            {
                categoryLabels.Add("Chưa có danh mục");
                categoryData.Add(0);
            }

            ViewBag.CategoryLabels = categoryLabels.ToArray();
            ViewBag.CategoryData = categoryData.ToArray();

            // 4. Danh sách Phiếu mượn gần đây (100% từ Database)
            var recentBorrows = _context.BorrowTickets
                .OrderByDescending(t => t.TicketID)
                .Take(5)
                .ToList();

            ViewBag.Users = _context.Users.ToDictionary(
                u => u.UserID, 
                u => $"[{u.Code}] {u.FullName ?? u.UserName}"
            );

            // 5. Top Sách Mượn Nhiều (Thống kê thực tế từ tblBorrowDetail và tblBook)
            var borrowedBookGroup = _context.BorrowDetails
                .GroupBy(d => d.BookID)
                .Select(g => new { BookID = g.Key, BorrowCount = g.Count() })
                .OrderByDescending(g => g.BorrowCount)
                .Take(4)
                .ToList();

            var topBookIds = borrowedBookGroup.Select(g => g.BookID).ToList();
            var topBooksDict = _context.Books.Where(b => topBookIds.Contains(b.BookID)).ToDictionary(b => b.BookID, b => b);
            var authorsDict = _context.Authors.ToDictionary(a => a.AuthorID, a => a.AuthorName ?? "Chưa rõ");

            var topBooksList = new List<dynamic>();
            foreach (var item in borrowedBookGroup)
            {
                if (topBooksDict.ContainsKey(item.BookID))
                {
                    var bk = topBooksDict[item.BookID];
                    string authName = authorsDict.ContainsKey(bk.AuthorID) ? authorsDict[bk.AuthorID] : "Đang cập nhật";
                    topBooksList.Add(new {
                        BookID = bk.BookID,
                        Code = bk.Code,
                        Title = bk.Title,
                        Author = authName,
                        Image = bk.Images,
                        AvailableQuantity = bk.AvailableQuantity,
                        BorrowCount = item.BorrowCount
                    });
                }
            }

            // Nếu chưa có lượt mượn nào, lấy các sách có sẵn trong DB để hiển thị
            if (topBooksList.Count == 0)
            {
                var defaultBooks = _context.Books.Where(b => b.IsActive == true).Take(3).ToList();
                foreach (var bk in defaultBooks)
                {
                    string authName = authorsDict.ContainsKey(bk.AuthorID) ? authorsDict[bk.AuthorID] : "Đang cập nhật";
                    topBooksList.Add(new {
                        BookID = bk.BookID,
                        Code = bk.Code,
                        Title = bk.Title,
                        Author = authName,
                        Image = bk.Images,
                        AvailableQuantity = bk.AvailableQuantity,
                        BorrowCount = 0
                    });
                }
            }

            ViewBag.TopBooks = topBooksList;

            return View(recentBorrows);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Functions.ClearSession();
            return RedirectToAction("Index", "Login", new { area = "Admin" });
        }
    }
}
