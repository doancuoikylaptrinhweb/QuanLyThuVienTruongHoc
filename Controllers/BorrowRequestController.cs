using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;
using LibraryManagement.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace LibraryManagement.Controllers
{
    public class BorrowRequestController : Controller
    {
        private readonly DataContext _context;

        public BorrowRequestController(DataContext context)
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

        // 1. ĐĂNG KÝ MƯỢN TRỰC TIẾP TỪ TRANG CHI TIẾT SÁCH (AJAX)
        // Có thông báo xác nhận trước khi mượn, kiểm tra tồn kho, số lượng cho phép & trạng thái tài khoản
        [HttpPost]
        public IActionResult QuickBorrow(long bookId)
        {
            int userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return Json(new
                {
                    success = false,
                    requireLogin = true,
                    message = "Vui lòng đăng nhập tài khoản Độc giả để đăng ký mượn sách trực tuyến!"
                });
            }

            var user = _context.Users.Find(userId);
            if (user == null || user.IsActive != true)
            {
                return Json(new
                {
                    success = false,
                    message = "Tài khoản độc giả không tồn tại hoặc hiện đang bị khóa!"
                });
            }

            var book = _context.Books.FirstOrDefault(b => b.BookID == bookId && b.IsActive == true);
            if (book == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy cuốn sách này hoặc sách đã ngừng lưu hành!"
                });
            }

            if (book.AvailableQuantity <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = $"Cuốn sách '{book.Title}' hiện đã hết trên kệ sách!"
                });
            }

            // Kiểm tra xem độc giả có phiếu mượn nào đã quá hạn chưa hoàn trả
            bool hasOverdue = _context.BorrowTickets.Any(t =>
                t.UserID == userId &&
                t.Status != "Đã trả" &&
                t.Status != "Từ chối" &&
                t.DueDate < DateTime.Now);

            if (hasOverdue)
            {
                return Json(new
                {
                    success = false,
                    message = "Bạn hiện có sách mượn đã quá hạn trả! Vui lòng hoàn trả sách tại thư viện trước khi đăng ký mượn cuốn mới."
                });
            }

            // Lấy danh sách các phiếu đang hoạt động (Đang mượn, Chờ duyệt, Trả một phần)
            var activeTickets = _context.BorrowTickets
                .Where(t => t.UserID == userId && (t.Status == "Pending" || t.Status == "Đang mượn" || t.Status == "Trả một phần"))
                .ToList();
            var activeTicketIds = activeTickets.Select(t => t.TicketID).ToList();

            // Đếm tổng số cuốn sách đang mượn hoặc chờ duyệt (quy chế tối đa 3 cuốn)
            int activeBooksCount = _context.BorrowDetails
                .Count(d => activeTicketIds.Contains(d.TicketID) && !d.IsReturned);

            if (activeBooksCount >= 3)
            {
                return Json(new
                {
                    success = false,
                    message = "Bạn đang mượn hoặc có yêu cầu chờ duyệt tối đa 3 cuốn sách theo quy định thư viện. Vui lòng hoàn trả bớt sách trước khi đăng ký thêm!"
                });
            }

            // Kiểm tra xem độc giả đã gửi yêu cầu mượn hoặc đang mượn đúng cuốn sách này chưa (tránh mượn trùng)
            bool alreadyRequested = _context.BorrowDetails
                .Any(d => activeTicketIds.Contains(d.TicketID) && d.BookID == bookId && !d.IsReturned);

            if (alreadyRequested)
            {
                return Json(new
                {
                    success = false,
                    message = $"Bạn đã gửi yêu cầu hoặc đang mượn cuốn sách '{book.Title}' rồi! Vui lòng kiểm tra trong mục 'Phiếu mượn của tôi'."
                });
            }

            // Sinh mã phiếu mượn tự động: PM0001, PM0002...
            int nextId = (_context.BorrowTickets.Max(t => (int?)t.TicketID) ?? 0) + 1;
            string ticketCode = $"PM{nextId:D4}";

            var ticket = new tblBorrowTicket
            {
                UserID = userId,
                TicketCode = ticketCode,
                Status = "Pending", // Chờ thủ thư phê duyệt
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                Note = "Đăng ký mượn trực tuyến qua trang chi tiết sách"
            };
            _context.BorrowTickets.Add(ticket);
            _context.SaveChanges();

            // Thêm chi tiết sách
            var detail = new tblBorrowDetail
            {
                TicketID = ticket.TicketID,
                BookID = bookId,
                FineAmount = 0,
                IsReturned = false,
                ReturnDate = null,
                Note = null
            };
            _context.BorrowDetails.Add(detail);
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                ticketCode = ticket.Code,
                bookTitle = book.Title,
                message = $"Đăng ký mượn thành công cuốn sách <strong>{book.Title}</strong>!<br/>Mã phiếu yêu cầu: <strong>{ticket.Code}</strong>.<br/>Yêu cầu của bạn đã được chuyển tới Thủ thư xét duyệt."
            });
        }

        // 2. FORM ĐẶT MƯỢN NHIỀU SÁCH CÙNG LÚC (DÀNH CHO ĐỘC GIẢ)
        [HttpGet]
        public IActionResult Create()
        {
            int userId = GetCurrentUserId();
            if (userId <= 0)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thực hiện đặt mượn sách!";
                return RedirectToAction("Index", "Home");
            }

            var availableBooks = _context.Books
                .Where(b => b.IsActive == true && b.AvailableQuantity > 0)
                .OrderBy(b => b.Title)
                .ToList();
            ViewBag.AvailableBooks = availableBooks;
            ViewBag.Authors = _context.Authors.ToDictionary(a => a.AuthorID, a => a.AuthorName ?? "Chưa rõ");
            return View();
        }

        // POST: Submit borrow request (creates ticket with status Pending, does NOT modify stock)
        [HttpPost]
        public IActionResult Create(long[] selectedBookIds)
        {
            int userId = GetCurrentUserId();
            if (userId <= 0)
            {
                TempData["Error"] = "Không xác định được người dùng đang đăng nhập.";
                return RedirectToAction("Index", "Home");
            }

            if (selectedBookIds == null || selectedBookIds.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một cuốn sách để yêu cầu mượn.";
                return RedirectToAction(nameof(Create));
            }

            if (selectedBookIds.Length > 3)
            {
                TempData["Error"] = "Theo quy định thư viện, bạn chỉ được mượn tối đa 3 cuốn sách cùng lúc.";
                return RedirectToAction(nameof(Create));
            }

            var books = _context.Books.Where(b => selectedBookIds.Contains(b.BookID) && b.IsActive == true && b.AvailableQuantity > 0).ToList();
            if (books.Count != selectedBookIds.Length)
            {
                TempData["Error"] = "Một số sách được lựa chọn hiện không còn khả dụng trên kệ.";
                return RedirectToAction(nameof(Create));
            }

            int nextId = (_context.BorrowTickets.Max(t => (int?)t.TicketID) ?? 0) + 1;
            var ticket = new tblBorrowTicket
            {
                UserID = userId,
                TicketCode = $"PM{nextId:D4}",
                Status = "Pending",
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                Note = "Đăng ký mượn trực tuyến theo danh sách"
            };
            _context.BorrowTickets.Add(ticket);
            _context.SaveChanges();

            foreach (var bookId in selectedBookIds)
            {
                var detail = new tblBorrowDetail
                {
                    TicketID = ticket.TicketID,
                    BookID = bookId,
                    FineAmount = 0,
                    IsReturned = false,
                    ReturnDate = null,
                    Note = null
                };
                _context.BorrowDetails.Add(detail);
            }
            _context.SaveChanges();

            TempData["Success"] = $"Đã gửi yêu cầu mượn thành công {selectedBookIds.Length} cuốn sách. Mã phiếu yêu cầu: [{ticket.Code}]. Vui lòng chờ Thủ thư phê duyệt.";
            return RedirectToAction(nameof(MyRequests));
        }

        // 3. XEM LỊCH SỬ VÀ DANH SÁCH PHIẾU MƯỢN CỦA ĐỘC GIẢ
        [HttpGet]
        public IActionResult MyRequests()
        {
            int userId = GetCurrentUserId();
            if (userId <= 0)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem phiếu mượn của bạn.";
                return RedirectToAction("Index", "Home");
            }

            var tickets = _context.BorrowTickets
                .Where(t => t.UserID == userId)
                .OrderByDescending(t => t.TicketID)
                .ToList();

            var ticketIds = tickets.Select(t => t.TicketID).ToList();
            var allDetails = _context.BorrowDetails.Where(d => ticketIds.Contains(d.TicketID)).ToList();
            ViewBag.TicketDetails = allDetails.GroupBy(d => d.TicketID).ToDictionary(g => g.Key, g => g.ToList());

            var bookIds = allDetails.Select(d => d.BookID).Distinct().ToList();
            ViewBag.Books = _context.Books.Where(b => bookIds.Contains(b.BookID)).ToDictionary(b => b.BookID, b => b);

            return View(tickets);
        }

        // 4. ĐỘC GIẢ GỬI YÊU CẦU TRẢ SÁCH (CHUYỂN SANG TRẠNG THÁI 'CHỜ DUYỆT TRẢ')
        [HttpPost]
        public IActionResult RequestReturn(int id, string? returnNote)
        {
            int userId = GetCurrentUserId();
            if (userId <= 0)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thực hiện trả sách.";
                return RedirectToAction("Index", "Home");
            }

            var ticket = _context.BorrowTickets.FirstOrDefault(t => t.TicketID == id && t.UserID == userId);
            if (ticket == null)
            {
                TempData["Error"] = "Không tìm thấy phiếu mượn tương ứng của bạn.";
                return RedirectToAction(nameof(MyRequests));
            }

            if (ticket.Status != "Đang mượn" && ticket.Status != "Trả một phần" && ticket.Status != "Quá hạn")
            {
                TempData["Error"] = $"Phiếu mượn đang ở trạng thái '{ticket.Status}', không thể gửi yêu cầu trả sách.";
                return RedirectToAction(nameof(MyRequests));
            }

            ticket.Status = "Chờ duyệt trả";
            if (!string.IsNullOrWhiteSpace(returnNote))
            {
                string noteText = returnNote.Trim();
                ticket.Note = string.IsNullOrWhiteSpace(ticket.Note) ? noteText : $"{ticket.Note} | Độc giả yêu cầu trả: {noteText}";
            }
            else
            {
                ticket.Note = string.IsNullOrWhiteSpace(ticket.Note) ? "Độc giả yêu cầu trả sách trực tuyến" : ticket.Note;
            }

            _context.BorrowTickets.Update(ticket);
            _context.SaveChanges();

            TempData["Success"] = $"Đã gửi yêu cầu trả sách cho phiếu [{ticket.Code}] thành công! Trạng thái: 'Chờ duyệt trả'. Vui lòng mang sách tới quầy để Thủ thư tiếp nhận và xác nhận hoàn tất.";
            return RedirectToAction(nameof(MyRequests));
        }
    }
}
