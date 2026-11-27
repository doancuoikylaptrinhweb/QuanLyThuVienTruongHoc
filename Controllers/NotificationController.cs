using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;
using LibraryManagement.Utilities;
using System.Security.Claims;

namespace LibraryManagement.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly DataContext _context;

        public NotificationController(DataContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            if (Functions.IsLogin() && Functions._UserID > 0)
            {
                return Functions._UserID;
            }

            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("UserID")?.Value;
            if (int.TryParse(idClaim, out int uid)) return uid;
            return 0;
        }

        [HttpGet]
        public IActionResult GetMyNotifications()
        {
            int userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var baseQuery = _context.ReaderNotifications
                .Where(n => n.UserID == userId);

            int unreadCount = baseQuery.Count(n => !n.IsRead);

            var items = baseQuery
                .OrderByDescending(n => n.CreatedDate)
                .Take(20)
                .Select(n => new
                {
                    notificationID = n.NotificationID,
                    ticketID = n.TicketID,
                    ticketCode = n.TicketCode,
                    bookTitles = n.BookTitles,
                    overdueDays = n.OverdueDays,
                    dueDate = n.DueDate.HasValue ? n.DueDate.Value.ToString("dd/MM/yyyy") : "N/A",
                    severityLevel = n.SeverityLevel,
                    title = n.Title,
                    message = n.Message,
                    isRead = n.IsRead,
                    createdDate = n.CreatedDate.ToString("dd/MM/yyyy HH:mm")
                })
                .ToList();

            return Json(new { success = true, unreadCount, items });
        }

        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            int userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập." });
            }

            // Chỉ cho phép độc giả đánh dấu đã đọc thông báo của chính mình
            var notification = _context.ReaderNotifications
                .FirstOrDefault(n => n.NotificationID == id && n.UserID == userId);

            if (notification == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy thông báo." });
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.Now;
                _context.SaveChanges();
            }

            int unreadCount = _context.ReaderNotifications
                .Count(n => n.UserID == userId && !n.IsRead);

            return Json(new { success = true, unreadCount });
        }

        [HttpPost]
        public IActionResult MarkAllAsRead()
        {
            int userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var unreadItems = _context.ReaderNotifications
                .Where(n => n.UserID == userId && !n.IsRead)
                .ToList();

            if (unreadItems.Count > 0)
            {
                DateTime now = DateTime.Now;
                foreach (var item in unreadItems)
                {
                    item.IsRead = true;
                    item.ReadDate = now;
                }
                _context.SaveChanges();
            }

            return Json(new { success = true, unreadCount = 0 });
        }
    }
}
