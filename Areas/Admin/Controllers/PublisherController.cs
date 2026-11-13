using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PublisherController : Controller
    {
        private readonly DataContext _context;

        public PublisherController(DataContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH NHÀ XUẤT BẢN (READ)
        public IActionResult Index(string? keyword)
        {
            var query = _context.Publishers.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                string term = keyword.Trim().ToLower();
                query = query.Where(p => 
                    (p.PublisherName != null && p.PublisherName.ToLower().Contains(term)) ||
                    (p.Address != null && p.Address.ToLower().Contains(term)) ||
                    (p.Phone != null && p.Phone.Contains(term))
                );
                ViewBag.Keyword = keyword;
            }

            var publishers = query.OrderByDescending(p => p.PublisherID).ToList();

            // Tính số lượng sách liên kết với mỗi NXB
            var bookCounts = new Dictionary<int, int>();
            foreach (var p in publishers)
            {
                bookCounts[p.PublisherID] = _context.Books.Count(b => b.PublisherID == p.PublisherID);
            }
            ViewBag.BookCounts = bookCounts;

            ViewBag.TotalPublishers = publishers.Count;

            return View(publishers);
        }

        // 2. THÊM MỚI NXB (CREATE - GET)
        public IActionResult Create()
        {
            return View();
        }

        // 2. THÊM MỚI NXB (CREATE - POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(tblPublisher publisher)
        {
            if (string.IsNullOrWhiteSpace(publisher.PublisherName))
            {
                ModelState.AddModelError("PublisherName", "Tên nhà xuất bản không được để trống!");
            }

            if (ModelState.IsValid)
            {
                publisher.PublisherName = publisher.PublisherName.Trim();
                _context.Publishers.Add(publisher);
                _context.SaveChanges();

                TempData["Success"] = $"Đã thêm thành công Nhà xuất bản: {publisher.PublisherName}!";
                return RedirectToAction(nameof(Index));
            }

            return View(publisher);
        }

        // 3. CHỈNH SỬA NXB (EDIT - GET)
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var publisher = _context.Publishers.Find(id);
            if (publisher == null) return NotFound();

            return View(publisher);
        }

        // 3. CHỈNH SỬA NXB (EDIT - POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(tblPublisher publisher)
        {
            if (string.IsNullOrWhiteSpace(publisher.PublisherName))
            {
                ModelState.AddModelError("PublisherName", "Tên nhà xuất bản không được để trống!");
            }

            if (ModelState.IsValid)
            {
                publisher.PublisherName = publisher.PublisherName.Trim();
                _context.Publishers.Update(publisher);
                _context.SaveChanges();

                TempData["Success"] = $"Đã cập nhật Nhà xuất bản: {publisher.PublisherName}!";
                return RedirectToAction(nameof(Index));
            }

            return View(publisher);
        }

        // 4. XÓA NXB (DELETE - GET)
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var publisher = _context.Publishers.Find(id);
            if (publisher == null) return NotFound();

            ViewBag.BookCount = _context.Books.Count(b => b.PublisherID == id);
            return View(publisher);
        }

        // 4. XÓA NXB (DELETE - POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var publisher = _context.Publishers.Find(id);
            if (publisher == null) return NotFound();

            int bookCount = _context.Books.Count(b => b.PublisherID == id);
            if (bookCount > 0)
            {
                TempData["Error"] = $"Không thể xóa Nhà xuất bản [{publisher.PublisherName}] vì đang có {bookCount} đầu sách liên kết!";
                return RedirectToAction(nameof(Index));
            }

            _context.Publishers.Remove(publisher);
            _context.SaveChanges();

            TempData["Success"] = $"Đã xóa Nhà xuất bản [{publisher.PublisherName}] thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
