using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BorrowController : AdminBaseController
    {
        private readonly DataContext _context;

        public BorrowController(DataContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH PHIẾU MƯỢN & TÌM KIẾM THEO MÃ
        public IActionResult Index(string? keyword)
        {
            var query = _context.BorrowTickets.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string term = keyword.Trim().ToLower();
                // Tìm User ID phù hợp
                var matchedUserIds = _context.Users
                    .Where(u => (u.FullName != null && u.FullName.ToLower().Contains(term)) ||
                                (u.UserName != null && u.UserName.ToLower().Contains(term)) ||
                                (u.UserCode != null && u.UserCode.ToLower().Contains(term)))
                    .Select(u => u.UserID).ToList();

                // Tìm Book ID phù hợp
                var matchedBookIds = _context.Books
                    .Where(b => (b.Title != null && b.Title.ToLower().Contains(term)) ||
                                (b.BookCode != null && b.BookCode.ToLower().Contains(term)))
                    .Select(b => b.BookID).ToList();

                var matchedTicketIdsFromBooks = _context.BorrowDetails
                    .Where(d => matchedBookIds.Contains(d.BookID))
                    .Select(d => d.TicketID).ToList();

                query = query.Where(t =>
                    (t.TicketCode != null && t.TicketCode.ToLower().Contains(term)) ||
                    (t.ReturnCode != null && t.ReturnCode.ToLower().Contains(term)) ||
                    (t.Status != null && t.Status.ToLower().Contains(term)) ||
                    matchedUserIds.Contains(t.UserID) ||
                    matchedTicketIdsFromBooks.Contains(t.TicketID)
                );

                ViewBag.Keyword = keyword;
            }

            var listTickets = query.OrderByDescending(t => t.TicketID).ToList();

            // Lấy thông tin người dùng
            ViewBag.Users = _context.Users.ToDictionary(u => u.UserID, u => u);

            // Lấy thông tin sách cho mỗi phiếu mượn
            var ticketIds = listTickets.Select(t => t.TicketID).ToList();
            var details = _context.BorrowDetails.Where(d => ticketIds.Contains(d.TicketID)).ToList();
            var bookIds = details.Select(d => d.BookID).Distinct().ToList();
            var books = _context.Books.Where(b => bookIds.Contains(b.BookID)).ToDictionary(b => b.BookID, b => b);

            var ticketBooks = new Dictionary<int, tblBook>();
            foreach (var detail in details)
            {
                if (books.ContainsKey(detail.BookID) && !ticketBooks.ContainsKey(detail.TicketID))
                {
                    ticketBooks[detail.TicketID] = books[detail.BookID];
                }
            }
            ViewBag.TicketBooks = ticketBooks;

            return View(listTickets);
        }

        // 2. LẬP PHIẾU MƯỢN (GET) - HIỂN THỊ MÃ ĐỘC GIẢ & MÃ SÁCH
        public IActionResult Create()
        {
            // Danh sách độc giả hiển thị kèm Mã độc giả: [DG0001] Nguyễn Văn An
            var users = _context.Users.Where(u => u.IsActive == true).ToList()
                .Select(u => new {
                    UserID = u.UserID,
                    DisplayText = $"[{u.Code}] {u.FullName ?? u.UserName} ({(u.Role == "Admin" ? "Quản trị" : (u.Role == "Librarian" ? "Thủ thư" : "Độc giả"))})"
                }).ToList();
            ViewBag.UserList = new SelectList(users, "UserID", "DisplayText");

            // Danh sách sách CÒN TRÊN KỆ hiển thị kèm Mã sách: [MS0001] Clean Code
            var books = _context.Books.Where(b => b.IsActive == true && b.AvailableQuantity > 0).ToList()
                .Select(b => new {
                    BookID = b.BookID,
                    DisplayText = $"[{b.Code}] {b.Title} (Còn: {b.AvailableQuantity} bản)"
                }).ToList();
            ViewBag.BookList = new SelectList(books, "BookID", "DisplayText");

            // Mã phiếu mượn dự kiến
            int nextId = (_context.BorrowTickets.Max(t => (int?)t.TicketID) ?? 0) + 1;
            ViewBag.PreviewTicketCode = $"PM{nextId:D4}";

            return View();
        }

        // 2. LẬP PHIẾU MƯỢN (POST) - TỰ ĐỘNG SINH MÃ PHIẾU MƯỢN & TRỪ SÁCH KHO
        [HttpPost]
        public IActionResult Create(tblBorrowTicket ticket, long bookId)
        {
            var book = _context.Books.Find(bookId);
            if (book == null || book.AvailableQuantity <= 0)
            {
                ModelState.AddModelError("", "Sách này hiện đã hết trên kệ, không thể mượn!");
            }

            if (ModelState.IsValid)
            {
                ticket.BorrowDate = DateTime.Now;
                ticket.Status = "Đang mượn";

                _context.BorrowTickets.Add(ticket);
                _context.SaveChanges();

                // Tự động sinh mã phiếu mượn nếu chưa nhập
                if (string.IsNullOrWhiteSpace(ticket.TicketCode))
                {
                    ticket.TicketCode = $"PM{ticket.TicketID:D4}";
                    _context.BorrowTickets.Update(ticket);
                }

                // Lưu chi tiết sách mượn
                var detail = new tblBorrowDetail
                {
                    TicketID = ticket.TicketID,
                    BookID = bookId,
                    FineAmount = 0
                };
                _context.BorrowDetails.Add(detail);

                // TỰ ĐỘNG TRỪ SỐ LƯỢNG KHẢ DỤNG CỦA SÁCH TRONG KHO
                book.AvailableQuantity = book.AvailableQuantity - 1;
                _context.Books.Update(book);

                _context.SaveChanges();
                TempData["Success"] = $"Lập phiếu mượn [{ticket.Code}] thành công cho độc giả!";
                return RedirectToAction("Index");
            }

            var users = _context.Users.Where(u => u.IsActive == true).ToList()
                .Select(u => new {
                    UserID = u.UserID,
                    DisplayText = $"[{u.Code}] {u.FullName ?? u.UserName}"
                }).ToList();
            ViewBag.UserList = new SelectList(users, "UserID", "DisplayText");

            var books = _context.Books.Where(b => b.IsActive == true && b.AvailableQuantity > 0).ToList()
                .Select(b => new {
                    BookID = b.BookID,
                    DisplayText = $"[{b.Code}] {b.Title} (Còn: {b.AvailableQuantity} bản)"
                }).ToList();
            ViewBag.BookList = new SelectList(books, "BookID", "DisplayText");

            return View(ticket);
        }

        // 3. XÁC NHẬN TRẢ SÁCH (POST) - TỰ ĐỘNG SINH MÃ TRẢ SÁCH & CỘNG LẠI KHO
        [HttpPost]
        public IActionResult ReturnBook(int id)
        {
            var ticket = _context.BorrowTickets.Find(id);
            if (ticket != null && ticket.Status == "Đang mượn")
            {
                ticket.Status = "Đã trả";
                ticket.ReturnDate = DateTime.Now;
                // Tự động sinh Mã trả sách: PT0001
                ticket.ReturnCode = $"PT{ticket.TicketID:D4}";

                // Tìm sách trong chi tiết mượn để cộng lại vào kho
                var detail = _context.BorrowDetails.FirstOrDefault(d => d.TicketID == id);
                if (detail != null)
                {
                    var book = _context.Books.Find(detail.BookID);
                    if (book != null)
                    {
                        book.AvailableQuantity = book.AvailableQuantity + 1;
                        _context.Books.Update(book);
                    }
                }

                _context.BorrowTickets.Update(ticket);
                _context.SaveChanges();
                TempData["Success"] = $"Xác nhận trả sách thành công! Đã tạo Mã trả sách: [{ticket.ReturnCode}].";
            }
            return RedirectToAction("Index");
        }
    }
}