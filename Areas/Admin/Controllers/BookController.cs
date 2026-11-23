using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PagedList.Core;
using LibraryManagement.Models;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BookController : AdminBaseController
    {
        private readonly DataContext _context;

        public BookController(DataContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ DANH SÁCH SÁCH CÓ TÌM KIẾM THEO MÃ & PHÂN TRANG (Lab 08)
        public IActionResult Index(string? keyword, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string term = keyword.Trim().ToLower();
                query = query.Where(b =>
                    (b.Title != null && b.Title.ToLower().Contains(term)) ||
                    (b.BookCode != null && b.BookCode.ToLower().Contains(term)) ||
                    (b.Summary != null && b.Summary.ToLower().Contains(term))
                );
                ViewBag.Keyword = keyword;
            }

            var books = query.OrderByDescending(b => b.BookID);
            PagedList<tblBook> models = new PagedList<tblBook>(books, page, pageSize);

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Authors = _context.Authors.ToList();

            return View(models);
        }

        // 2. THÊM MỚI SÁCH (GET)
        public IActionResult Create()
        {
            ViewBag.CategoryList = new SelectList(_context.Categories.Where(c => c.IsActive == true), "CategoryID", "CategoryName");
            ViewBag.AuthorList = new SelectList(_context.Authors.Where(a => a.IsActive == true), "AuthorID", "AuthorName");
            ViewBag.PublisherList = new SelectList(_context.Publishers, "PublisherID", "PublisherName");

            // Dự kiến mã sách
            long nextId = (_context.Books.Max(b => (long?)b.BookID) ?? 0) + 1;
            ViewBag.PreviewBookCode = $"MS{nextId:D4}";

            return View();
        }

        // 2. THÊM MỚI SÁCH (POST) - TỰ ĐỘNG GÁN MÃ SÁCH NẾU CHƯA NHẬP
        [HttpPost]
        public IActionResult Create(tblBook book)
        {
            if (ModelState.IsValid)
            {
                book.CreatedDate = DateTime.Now;
                book.AvailableQuantity = book.Quantity; // Ban đầu số sách khả dụng bằng tổng số sách
                _context.Books.Add(book);
                _context.SaveChanges();

                // Tự động gán mã sách chuẩn nếu để trống
                if (string.IsNullOrWhiteSpace(book.BookCode))
                {
                    book.BookCode = $"MS{book.BookID:D4}";
                    _context.Books.Update(book);
                    _context.SaveChanges();
                }

                TempData["Success"] = $"Thêm mới sách [{book.Code}] thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.CategoryList = new SelectList(_context.Categories.Where(c => c.IsActive == true), "CategoryID", "CategoryName");
            ViewBag.AuthorList = new SelectList(_context.Authors.Where(a => a.IsActive == true), "AuthorID", "AuthorName");
            ViewBag.PublisherList = new SelectList(_context.Publishers, "PublisherID", "PublisherName");
            return View(book);
        }

        // 3. XÓA SÁCH (POST)
        [HttpPost]
        public IActionResult Delete(long id)
        {
            var book = _context.Books.Find(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}