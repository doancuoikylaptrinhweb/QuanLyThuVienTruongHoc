using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;
using LibraryManagement.Services;
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
            // Đồng bộ cảnh báo quá hạn tự động (gộp theo phiếu mượn, không trùng lặp)
            OverdueWarningService.SyncOverdueWarnings(_context);

            // 1. Thống kê số liệu thời gian thực từ Database (chỉ tính sách đang lưu hành IsActive == true)
            int bookCount = _context.Books.Count(b => b.IsActive == true);
            int copyCount = _context.Books.Where(b => b.IsActive == true).Sum(b => (int?)b.Quantity) ?? 0;

            var activeBorrowStatuses = new[] { "Đang mượn", "Trả một phần", "Quá hạn", "Chờ duyệt trả" };
            int borrowingTicketCount = _context.BorrowTickets
                .Count(t => t.Status != null && activeBorrowStatuses.Contains(t.Status));

            // Số sách thực tế đang được mượn và chưa trả (không tính sách đã trả IsReturned == true)
            int borrowingBookCount = (from d in _context.BorrowDetails
                                      join t in _context.BorrowTickets on d.TicketID equals t.TicketID
                                      where !d.IsReturned && t.Status != null && activeBorrowStatuses.Contains(t.Status)
                                      select d.DetailID).Count();

            var activeWarnings = _context.OverdueWarnings
                .Where(w => w.WarningStatus != OverdueWarningService.StatusReturned)
                .OrderByDescending(w => w.OverdueDays)
                .ToList();

            int overdueCount = activeWarnings.Count;
            int overdueBookCount = activeWarnings.Sum(w => w.OverdueBookCount);
            int pendingCount = _context.BorrowTickets.Count(t => t.Status == "Pending" || t.Status == "Chờ duyệt");

            ViewBag.TotalBooks = bookCount;
            ViewBag.TotalCopies = copyCount;
            ViewBag.TotalBorrowing = borrowingTicketCount;
            ViewBag.TotalBorrowingBooks = borrowingBookCount;
            ViewBag.TotalOverdue = overdueCount;
            ViewBag.TotalOverdueBooks = overdueBookCount;
            ViewBag.OverdueReminderCount = activeWarnings.Count(w => w.SeverityLevel == OverdueWarningService.SeverityReminder);
            ViewBag.OverdueWarningCount = activeWarnings.Count(w => w.SeverityLevel == OverdueWarningService.SeverityWarning);
            ViewBag.OverdueCriticalCount = activeWarnings.Count(w => w.SeverityLevel == OverdueWarningService.SeverityCritical);
            ViewBag.TotalPending = pendingCount;

            // 2. Dữ liệu biểu đồ Xu hướng Mượn & Trả (6 tháng gần nhất theo từng lượt sách trong SQL Server)
            var trendMonths = new List<string>();
            var borrowCounts = new List<int>();
            var returnCounts = new List<int>();

            var excludedBorrowStatuses = new[] { "Pending", "Chờ duyệt", "Từ chối", "Đã hủy" };
            var detailWithTickets = (from d in _context.BorrowDetails
                                     join t in _context.BorrowTickets on d.TicketID equals t.TicketID
                                     select new
                                     {
                                         t.BorrowDate,
                                         t.Status,
                                         d.IsReturned,
                                         EffectiveReturnDate = d.ReturnDate ?? t.ReturnDate
                                     }).ToList();

            DateTime now = DateTime.Today;
            for (int i = 5; i >= 0; i--)
            {
                var targetMonth = now.AddMonths(-i);
                int year = targetMonth.Year;
                int month = targetMonth.Month;

                trendMonths.Add($"T{month}/{year}");

                int bCount = detailWithTickets.Count(x =>
                    x.BorrowDate.Year == year &&
                    x.BorrowDate.Month == month &&
                    (x.Status == null || !excludedBorrowStatuses.Contains(x.Status)));

                int rCount = detailWithTickets.Count(x =>
                    x.IsReturned &&
                    x.EffectiveReturnDate.HasValue &&
                    x.EffectiveReturnDate.Value.Year == year &&
                    x.EffectiveReturnDate.Value.Month == month);

                borrowCounts.Add(bCount);
                returnCounts.Add(rCount);
            }

            ViewBag.TrendMonths = trendMonths.ToArray();
            ViewBag.BorrowTrend = borrowCounts.ToArray();
            ViewBag.ReturnTrend = returnCounts.ToArray();

            // 3. Dữ liệu biểu đồ Cơ cấu Thể loại Sách (100% từ Database, chỉ tính sách IsActive == true)
            var categories = _context.Categories.Where(c => c.IsActive == true).OrderBy(c => c.CategoryID).ToList();
            var activeBookCountsByCat = _context.Books
                .Where(b => b.IsActive == true)
                .GroupBy(b => b.CategoryID)
                .ToDictionary(g => g.Key, g => g.Count());

            var categoryLabels = new List<string>();
            var categoryData = new List<int>();

            foreach (var cat in categories)
            {
                int count = activeBookCountsByCat.ContainsKey(cat.CategoryID) ? activeBookCountsByCat[cat.CategoryID] : 0;
                if (count > 0)
                {
                    categoryLabels.Add(cat.CategoryName ?? "Khác");
                    categoryData.Add(count);
                }
            }

            bool hasCategoryData = categoryData.Sum() > 0;
            if (!hasCategoryData)
            {
                categoryLabels.Add("Chưa có sách trong kho");
                categoryData.Add(0);
            }

            ViewBag.HasCategoryData = hasCategoryData;
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

            // 6. Top 5 cảnh báo sách quá hạn nghiêm trọng nhất
            var top5Warnings = activeWarnings.Take(5).ToList();
            var topWarningTicketIds = top5Warnings.Select(w => w.TicketID).ToList();
            var topWarningTickets = _context.BorrowTickets
                .Where(t => topWarningTicketIds.Contains(t.TicketID))
                .ToDictionary(t => t.TicketID, t => t);
            var userDict = _context.Users.ToDictionary(u => u.UserID, u => u);

            var topOverdueList = top5Warnings.Select(w =>
            {
                topWarningTickets.TryGetValue(w.TicketID, out var ticket);
                userDict.TryGetValue(w.UserID, out var reader);
                return new
                {
                    WarningID = w.WarningID,
                    TicketID = w.TicketID,
                    TicketCode = ticket?.Code ?? $"PM{w.TicketID:D4}",
                    ReaderCode = reader?.Code ?? $"DG{w.UserID:D4}",
                    ReaderName = reader?.FullName ?? reader?.UserName ?? "Bạn đọc",
                    BookTitles = w.BookTitles ?? "Tài liệu thư viện",
                    OverdueBookCount = w.OverdueBookCount,
                    OverdueDays = w.OverdueDays,
                    DueDate = ticket?.DueDate ?? DateTime.Today.AddDays(-w.OverdueDays),
                    SeverityLevel = w.SeverityLevel,
                    WarningStatus = w.WarningStatus,
                    TicketStatus = ticket?.Status ?? "Đang mượn"
                };
            }).ToList();

            ViewBag.TopOverdueWarnings = topOverdueList;

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
