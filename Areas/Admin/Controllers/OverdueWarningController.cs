using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;
using LibraryManagement.Services;
using LibraryManagement.Utilities;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Librarian")]
    public class OverdueWarningController : AdminBaseController
    {
        private readonly DataContext _context;

        public OverdueWarningController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string? readerKeyword, string? bookKeyword, string? overdueRange, int? minDays, string? status)
        {
            // Tự động đồng bộ & phân loại cảnh báo quá hạn từ dữ liệu mượn trả thực tế
            OverdueWarningService.SyncOverdueWarnings(_context);

            var allWarnings = _context.OverdueWarnings.ToList();
            var ticketIds = allWarnings.Select(w => w.TicketID).Distinct().ToList();
            var userIds = allWarnings.Select(w => w.UserID).Distinct().ToList();

            var ticketDict = _context.BorrowTickets
                .Where(t => ticketIds.Contains(t.TicketID))
                .ToDictionary(t => t.TicketID, t => t);

            var userDict = _context.Users
                .Where(u => userIds.Contains(u.UserID))
                .ToDictionary(u => u.UserID, u => u);

            var detailsByTicket = _context.BorrowDetails
                .Where(d => ticketIds.Contains(d.TicketID))
                .ToList()
                .GroupBy(d => d.TicketID)
                .ToDictionary(g => g.Key, g => g.ToList());

            var bookIds = detailsByTicket.Values
                .SelectMany(list => list)
                .Select(d => d.BookID)
                .Distinct()
                .ToList();

            var bookDict = _context.Books
                .Where(b => bookIds.Contains(b.BookID))
                .ToDictionary(b => b.BookID, b => b);

            // Thống kê tổng quan (trước khi lọc)
            var activeWarnings = allWarnings.Where(w => w.WarningStatus != OverdueWarningService.StatusReturned).ToList();
            ViewBag.TotalActiveWarnings = activeWarnings.Count;
            ViewBag.TotalOverdueBooks = activeWarnings.Sum(w => w.OverdueBookCount);
            ViewBag.ReminderCount = activeWarnings.Count(w => w.SeverityLevel == OverdueWarningService.SeverityReminder);
            ViewBag.WarningCount = activeWarnings.Count(w => w.SeverityLevel == OverdueWarningService.SeverityWarning);
            ViewBag.CriticalCount = activeWarnings.Count(w => w.SeverityLevel == OverdueWarningService.SeverityCritical);
            ViewBag.PendingActionCount = activeWarnings.Count(w => w.WarningStatus == OverdueWarningService.StatusPending);
            ViewBag.RemindedActionCount = activeWarnings.Count(w => w.WarningStatus == OverdueWarningService.StatusReminded);

            // Áp dụng bộ lọc
            IEnumerable<tblOverdueWarning> filtered = allWarnings;

            if (!string.IsNullOrWhiteSpace(readerKeyword))
            {
                string rTerm = readerKeyword.Trim().ToLowerInvariant();
                filtered = filtered.Where(w =>
                {
                    if (!userDict.TryGetValue(w.UserID, out var u)) return false;
                    return (u.FullName != null && u.FullName.ToLowerInvariant().Contains(rTerm)) ||
                           (u.UserName != null && u.UserName.ToLowerInvariant().Contains(rTerm)) ||
                           (u.UserCode != null && u.UserCode.ToLowerInvariant().Contains(rTerm)) ||
                           u.Code.ToLowerInvariant().Contains(rTerm);
                });
            }

            if (!string.IsNullOrWhiteSpace(bookKeyword))
            {
                string bTerm = bookKeyword.Trim().ToLowerInvariant();
                filtered = filtered.Where(w =>
                {
                    if (w.BookTitles != null && w.BookTitles.ToLowerInvariant().Contains(bTerm))
                        return true;

                    if (detailsByTicket.TryGetValue(w.TicketID, out var details))
                    {
                        foreach (var d in details)
                        {
                            if (bookDict.TryGetValue(d.BookID, out var bk))
                            {
                                if ((bk.Title != null && bk.Title.ToLowerInvariant().Contains(bTerm)) ||
                                    (bk.BookCode != null && bk.BookCode.ToLowerInvariant().Contains(bTerm)) ||
                                    bk.Code.ToLowerInvariant().Contains(bTerm))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                });
            }

            if (!string.IsNullOrWhiteSpace(overdueRange))
            {
                if (overdueRange == "1-3")
                    filtered = filtered.Where(w => w.OverdueDays >= 1 && w.OverdueDays <= 3);
                else if (overdueRange == "4-7")
                    filtered = filtered.Where(w => w.OverdueDays >= 4 && w.OverdueDays <= 7);
                else if (overdueRange == "8+")
                    filtered = filtered.Where(w => w.OverdueDays > 7);
            }

            if (minDays.HasValue && minDays.Value > 0)
            {
                filtered = filtered.Where(w => w.OverdueDays >= minDays.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                filtered = filtered.Where(w => w.WarningStatus == status);
            }

            var resultList = filtered
                .OrderBy(w => w.WarningStatus == OverdueWarningService.StatusReturned ? 1 : 0)
                .ThenByDescending(w => w.OverdueDays)
                .ThenByDescending(w => w.WarningID)
                .ToList();

            ViewBag.ReaderKeyword = readerKeyword;
            ViewBag.BookKeyword = bookKeyword;
            ViewBag.OverdueRange = overdueRange;
            ViewBag.MinDays = minDays;
            ViewBag.Status = status;

            ViewBag.Tickets = ticketDict;
            ViewBag.Users = userDict;
            ViewBag.DetailsByTicket = detailsByTicket;
            ViewBag.Books = bookDict;

            return View(resultList);
        }

        [HttpPost]
        public IActionResult MarkReminded(int warningId, string? reminderNote)
        {
            var warning = _context.OverdueWarnings.Find(warningId);
            if (warning == null)
            {
                TempData["Error"] = "Không tìm thấy bản ghi cảnh báo quá hạn!";
                return RedirectToAction(nameof(Index));
            }

            if (warning.WarningStatus == OverdueWarningService.StatusReturned)
            {
                TempData["Error"] = "Phiếu mượn này đã hoàn tất trả sách!";
                return RedirectToAction(nameof(Index));
            }

            string adminName = User.Identity?.Name ?? Functions._UserName ?? "Quản trị viên";
            warning.WarningStatus = OverdueWarningService.StatusReminded;
            warning.ReminderCount += 1;
            warning.LastRemindedDate = DateTime.Now;
            warning.RemindedBy = adminName;
            if (!string.IsNullOrWhiteSpace(reminderNote))
            {
                warning.ReminderNote = reminderNote.Trim();
            }
            else if (string.IsNullOrWhiteSpace(warning.ReminderNote))
            {
                warning.ReminderNote = $"Đã gửi thông báo nhắc nhở lần {warning.ReminderCount}";
            }
            warning.UpdatedDate = DateTime.Now;

            _context.SaveChanges();

            TempData["Success"] = $"Đã đánh dấu nhắc nhở thành công cho cảnh báo #{warning.WarningID} (lần {warning.ReminderCount}).";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult SyncNow()
        {
            OverdueWarningService.SyncOverdueWarnings(_context);
            TempData["Success"] = "Đã đồng bộ và phân loại lại toàn bộ cảnh báo sách quá hạn từ SQL Server.";
            return RedirectToAction(nameof(Index));
        }
    }
}
