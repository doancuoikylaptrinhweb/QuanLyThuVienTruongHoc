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

            DateTime cooldownThreshold = DateTime.Now.AddHours(-24);
            var recentlyRemindedTicketIds = _context.ReaderNotifications
                .Where(n => n.CreatedDate >= cooldownThreshold)
                .Select(n => n.TicketID)
                .Distinct()
                .ToHashSet();
            ViewBag.RecentlyRemindedTicketIds = recentlyRemindedTicketIds;

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

            var ticket = _context.BorrowTickets.FirstOrDefault(t => t.TicketID == warning.TicketID);
            if (ticket == null)
            {
                TempData["Error"] = "Không tìm thấy phiếu mượn tương ứng với cảnh báo này!";
                return RedirectToAction(nameof(Index));
            }

            int readerId = ticket.UserID > 0 ? ticket.UserID : warning.UserID;
            var reader = _context.Users.FirstOrDefault(u => u.UserID == readerId);
            if (reader == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản độc giả của phiếu mượn này!";
                return RedirectToAction(nameof(Index));
            }

            // Lấy chính xác các cuốn sách chưa trả thuộc phiếu mượn
            var unreturnedBookTitles = (from d in _context.BorrowDetails
                                        join b in _context.Books on d.BookID equals b.BookID
                                        where d.TicketID == ticket.TicketID && !d.IsReturned
                                        select b.Title).ToList();

            if (!unreturnedBookTitles.Any() || ticket.Status == "Đã trả" || ticket.Status == "Đã hủy")
            {
                OverdueWarningService.SyncOverdueWarnings(_context);
                TempData["Error"] = "Phiếu mượn này không còn sách quá hạn chưa trả!";
                return RedirectToAction(nameof(Index));
            }

            DateTime today = DateTime.Today;
            DateTime dueDate = ticket.DueDate.Date;
            int overdueDays = Math.Max(1, (today - dueDate).Days);
            if (dueDate >= today)
            {
                OverdueWarningService.SyncOverdueWarnings(_context);
                TempData["Error"] = "Phiếu mượn này hiện chưa quá hạn trả!";
                return RedirectToAction(nameof(Index));
            }

            string ticketCode = string.IsNullOrWhiteSpace(ticket.Code) ? $"PM{ticket.TicketID:D4}" : ticket.Code;

            // Cơ chế chống gửi nhắc nhở trùng lặp liên tiếp (cooldown 24 giờ dựa trên thông báo thực tế đã gửi cho độc giả)
            DateTime? latestNotifTime = _context.ReaderNotifications
                .Where(n => n.TicketID == ticket.TicketID && n.UserID == readerId)
                .OrderByDescending(n => n.CreatedDate)
                .Select(n => (DateTime?)n.CreatedDate)
                .FirstOrDefault();

            if (latestNotifTime.HasValue && (DateTime.Now - latestNotifTime.Value).TotalHours < 24)
            {
                DateTime nextAllowed = latestNotifTime.Value.AddHours(24);
                TempData["Error"] = $"Phiếu {ticketCode} đã được gửi thông báo nhắc nhở lúc {latestNotifTime.Value:dd/MM/yyyy HH:mm}. Vui lòng đợi đến {nextAllowed:dd/MM/yyyy HH:mm} để tránh gửi thông báo trùng lặp.";
                return RedirectToAction(nameof(Index));
            }

            string bookTitles = string.Join(", ", unreturnedBookTitles.Distinct());
            if (bookTitles.Length > 980) bookTitles = bookTitles.Substring(0, 977) + "...";

            string severity = OverdueWarningService.ClassifySeverity(overdueDays);
            string adminName = User.Identity?.Name ?? Functions._UserName ?? "Quản trị viên";
            string noteText = string.IsNullOrWhiteSpace(reminderNote) ? string.Empty : reminderNote.Trim();
            DateTime now = DateTime.Now;

            warning.UserID = readerId;
            warning.OverdueDays = overdueDays;
            warning.OverdueBookCount = unreturnedBookTitles.Count;
            warning.BookTitles = bookTitles;
            warning.SeverityLevel = severity;
            warning.WarningStatus = OverdueWarningService.StatusReminded;
            warning.ReminderCount += 1;
            warning.LastRemindedDate = now;
            warning.RemindedBy = adminName;
            if (!string.IsNullOrEmpty(noteText))
            {
                warning.ReminderNote = noteText;
            }
            else if (string.IsNullOrWhiteSpace(warning.ReminderNote))
            {
                warning.ReminderNote = $"Đã gửi thông báo nhắc nhở lần {warning.ReminderCount}";
            }
            warning.UpdatedDate = now;

            string dueDateStr = ticket.DueDate.ToString("dd/MM/yyyy");
            string message = $"Phiếu mượn {ticketCode} đã quá hạn {overdueDays} ngày (Hạn trả: {dueDateStr}). Sách chưa trả: {bookTitles}.";
            if (!string.IsNullOrEmpty(noteText))
            {
                message += $" Ghi chú từ thủ thư: {noteText}";
            }
            if (message.Length > 1490) message = message.Substring(0, 1487) + "...";

            var notification = new tblReaderNotification
            {
                UserID = readerId,
                TicketID = ticket.TicketID,
                WarningID = warning.WarningID,
                TicketCode = ticketCode,
                BookTitles = bookTitles,
                OverdueDays = overdueDays,
                DueDate = ticket.DueDate,
                SeverityLevel = severity,
                Title = $"Nhắc nhở quá hạn phiếu {ticketCode} ({overdueDays} ngày)",
                Message = message,
                IsRead = false,
                CreatedDate = now,
                CreatedBy = adminName
            };
            _context.ReaderNotifications.Add(notification);

            _context.SaveChanges();

            TempData["Success"] = $"Đã gửi thông báo nhắc nhở đến độc giả {reader.FullName ?? reader.UserName} cho phiếu {ticketCode} (lần {warning.ReminderCount}).";
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
