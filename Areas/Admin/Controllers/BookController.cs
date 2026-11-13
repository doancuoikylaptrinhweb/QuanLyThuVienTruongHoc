using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PagedList.Core;
using LibraryManagement.Models;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BookController : Controller
    {
        private readonly DataContext _context;

        public BookController(DataContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ DANH SÁCH SÁCH CÓ PHÂN TRANG (Lab 08)
        public IActionResult Index(int page = 1)
        {
            int pageSize = 6; // Hiển thị 6 cuốn sách trên 1 trang
            var books = _context.Books.OrderByDescending(b => b.BookID);
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

            return View();
        }

        // 2. THÊM MỚI SÁCH (POST)
        [HttpPost]
        public IActionResult Create(tblBook book)
        {
            if (ModelState.IsValid)
            {
                book.CreatedDate = DateTime.Now;
                book.AvailableQuantity = book.Quantity; // Ban đầu số sách khả dụng bằng tổng số sách
                _context.Books.Add(book);
                _context.SaveChanges();
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