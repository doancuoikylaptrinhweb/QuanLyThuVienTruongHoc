using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthorController : Controller
    {
        private readonly DataContext _context;

        public AuthorController(DataContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH TÁC GIẢ (READ)
        public IActionResult Index(string? keyword)
        {
            var query = _context.Authors.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                string term = keyword.Trim().ToLower();
                query = query.Where(a => 
                    (a.AuthorName != null && a.AuthorName.ToLower().Contains(term)) ||
                    (a.Bio != null && a.Bio.ToLower().Contains(term))
                );
                ViewBag.Keyword = keyword;
            }

            var authors = query.OrderByDescending(a => a.AuthorID).ToList();

            // Tính số lượng đầu sách cho từng tác giả
            var bookCounts = new Dictionary<int, int>();
            foreach (var a in authors)
            {
                bookCounts[a.AuthorID] = _context.Books.Count(b => b.AuthorID == a.AuthorID);
            }
            ViewBag.BookCounts = bookCounts;

            // Thống kê
            ViewBag.TotalAuthors = authors.Count;
            ViewBag.ActiveAuthors = authors.Count(a => a.IsActive == true);

            return View(authors);
        }

        // 2. THÊM MỚI TÁC GIẢ (CREATE - GET)
        public IActionResult Create()
        {
            return View();
        }

        // 2. THÊM MỚI TÁC GIẢ (CREATE - POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(tblAuthor author)
        {
            if (string.IsNullOrWhiteSpace(author.AuthorName))
            {
                ModelState.AddModelError("AuthorName", "Tên tác giả không được để trống!");
            }

            if (ModelState.IsValid)
            {
                author.AuthorName = author.AuthorName.Trim();
                author.IsActive = author.IsActive ?? true;
                _context.Authors.Add(author);
                _context.SaveChanges();

                TempData["Success"] = $"Đã thêm thành công tác giả: {author.AuthorName}!";
                return RedirectToAction(nameof(Index));
            }

            return View(author);
        }

        // 3. CHỈNH SỬA TÁC GIẢ (EDIT - GET)
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var author = _context.Authors.Find(id);
            if (author == null) return NotFound();

            return View(author);
        }

        // 3. CHỈNH SỬA TÁC GIẢ (EDIT - POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(tblAuthor author)
        {
            if (string.IsNullOrWhiteSpace(author.AuthorName))
            {
                ModelState.AddModelError("AuthorName", "Tên tác giả không được để trống!");
            }

            if (ModelState.IsValid)
            {
                author.AuthorName = author.AuthorName.Trim();
                _context.Authors.Update(author);
                _context.SaveChanges();

                TempData["Success"] = $"Đã cập nhật thông tin tác giả: {author.AuthorName}!";
                return RedirectToAction(nameof(Index));
            }

            return View(author);
        }

        // 4. XÓA TÁC GIẢ (DELETE - GET)
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var author = _context.Authors.Find(id);
            if (author == null) return NotFound();

            ViewBag.BookCount = _context.Books.Count(b => b.AuthorID == id);
            return View(author);
        }

        // 4. XÓA TÁC GIẢ (DELETE - POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var author = _context.Authors.Find(id);
            if (author == null) return NotFound();

            int bookCount = _context.Books.Count(b => b.AuthorID == id);
            if (bookCount > 0)
            {
                TempData["Error"] = $"Không thể xóa tác giả [{author.AuthorName}] vì đang có {bookCount} đầu sách liên kết!";
                return RedirectToAction(nameof(Index));
            }

            _context.Authors.Remove(author);
            _context.SaveChanges();

            TempData["Success"] = $"Đã xóa tác giả [{author.AuthorName}] thành công!";
            return RedirectToAction(nameof(Index));
        }

        // 5. KHÓA / KÍCH HOẠT TÁC GIẢ
        [HttpPost]
        public IActionResult ToggleActive(int id)
        {
            var author = _context.Authors.Find(id);
            if (author == null) return NotFound();

            author.IsActive = !(author.IsActive ?? false);
            _context.SaveChanges();

            string status = (author.IsActive == true) ? "Hiển thị" : "Ẩn";
            TempData["Success"] = $"Đã chuyển trạng thái tác giả [{author.AuthorName}] sang: {status}!";
            return RedirectToAction(nameof(Index));
        }
    }
}
