using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BorrowController : Controller
    {
        private readonly DataContext _context;

        public BorrowController(DataContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH PHIẾU MƯỢN
        public IActionResult Index()
        {
            var listTickets = _context.BorrowTickets.OrderByDescending(t => t.TicketID).ToList();
            ViewBag.Users = _context.Users.ToDictionary(u => u.UserID, u => u.FullName ?? u.UserName);
            return View(listTickets);
        }

        // 2. LẬP PHIẾU MƯỢN (GET)
        public IActionResult Create()
        {
            // Danh sách độc giả
            ViewBag.UserList = new SelectList(_context.Users.Where(u => u.IsActive == true), "UserID", "FullName");
            // Danh sách sách CÒN TRÊN KỆ (AvailableQuantity > 0)
            ViewBag.BookList = new SelectList(_context.Books.Where(b => b.IsActive == true && b.AvailableQuantity > 0), "BookID", "Title");

            return View();
        }

        // 2. LẬP PHIẾU MƯỢN (POST) - TỰ ĐỘNG TRỪ SÁCH KHO
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
                return RedirectToAction("Index");
            }

            ViewBag.UserList = new SelectList(_context.Users.Where(u => u.IsActive == true), "UserID", "FullName");
            ViewBag.BookList = new SelectList(_context.Books.Where(b => b.IsActive == true && b.AvailableQuantity > 0), "BookID", "Title");
            return View(ticket);
        }

        // 3. XÁC NHẬN TRẢ SÁCH (POST) - TỰ ĐỘNG CỘNG LẠI SÁCH VÀO KHO
        [HttpPost]
        public IActionResult ReturnBook(int id)
        {
            var ticket = _context.BorrowTickets.Find(id);
            if (ticket != null && ticket.Status == "Đang mượn")
            {
                ticket.Status = "Đã trả";
                ticket.ReturnDate = DateTime.Now;

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
            }
            return RedirectToAction("Index");
        }
    }
}