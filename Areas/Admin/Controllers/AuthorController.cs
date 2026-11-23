using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Librarian")] // Phân quyền cấp CONTROLLER
    public class AuthorController : AdminBaseController
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
        [HttpGet]
        public IActionResult Create()
        {
            return View(new tblAuthor { IsActive = true });
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
                try
                {
                    string name = author.AuthorName!.Trim();
                    author.AuthorName = name;
                    author.IsActive = author.IsActive ?? true;

                    _context.Authors.Add(author);
                    _context.SaveChanges();

                    TempData["Success"] = $"Đã thêm thành công tác giả: {author.AuthorName}!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi khi lưu tác giả vào CSDL: " + ex.Message;
                    ModelState.AddModelError("", "Lỗi khi lưu tác giả: " + ex.Message);
                }
            }

            return View(author);
        }

        // 2b. THÊM NHANH TÁC GIẢ QUA AJAX / MODAL
        [HttpPost]
        public IActionResult QuickCreate(string authorName, string? bio)
        {
            if (string.IsNullOrWhiteSpace(authorName))
            {
                return Json(new { success = false, message = "Vui lòng nhập tên tác giả!" });
            }

            try
            {
                string name = authorName.Trim();
                var existing = _context.Authors.FirstOrDefault(a => a.AuthorName != null && a.AuthorName.ToLower() == name.ToLower());
                if (existing != null)
                {
                    return Json(new { success = false, message = $"Tác giả '{name}' đã tồn tại trong hệ thống (ID: #{existing.AuthorID})!" });
                }

                var author = new tblAuthor
                {
                    AuthorName = name,
                    Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim(),
                    IsActive = true
                };

                _context.Authors.Add(author);
                _context.SaveChanges();

                return Json(new { 
                    success = true, 
                    authorId = author.AuthorID, 
                    authorName = author.AuthorName, 
                    message = $"Đã thêm thành công tác giả '{author.AuthorName}'!" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi thêm tác giả: " + ex.Message });
            }
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

        // 4. XÓA TÁC GIẢ (DELETE - GET) - CHỈ ADMIN TỐI CAO
        [HttpGet]
        [Authorize(Roles = "Admin")] // Phân quyền cấp ENDPOINT
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var author = _context.Authors.Find(id);
            if (author == null) return NotFound();

            ViewBag.BookCount = _context.Books.Count(b => b.AuthorID == id);
            return View(author);
        }

        // 4. XÓA TÁC GIẢ (DELETE - POST) - CHỈ ADMIN TỐI CAO
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Phân quyền cấp ENDPOINT
        public IActionResult DeleteConfirmed(int id)
        {
            var author = _context.Authors.Find(id);
            if (author == null) return NotFound();

            int bookCount = _context.Books.Count(b => b.AuthorID == id);
            if (bookCount > 0)
            {
                // Áp dụng SOFT DELETE để bảo toàn khóa ngoại của các đầu sách
                author.IsActive = false;
                _context.SaveChanges();
                TempData["Success"] = $"Tác giả [{author.AuthorName}] đang có {bookCount} đầu sách liên kết. Để bảo toàn toàn vẹn dữ liệu, hệ thống đã chuyển sang chế độ XÓA MỀM (Ẩn tác giả)!";
                return RedirectToAction(nameof(Index));
            }

            _context.Authors.Remove(author);
            _context.SaveChanges();

            TempData["Success"] = $"Đã xóa vĩnh viễn tác giả [{author.AuthorName}] thành công!";
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
