using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Librarian")] // Phân quyền cấp CONTROLLER
    public class CategoryController : AdminBaseController
    {
        private readonly DataContext _context;

        public CategoryController(DataContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ DANH SÁCH (READ)
        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult Index()
        {
            var listCategory = _context.Categories.OrderByDescending(c => c.CategoryID).ToList();

            // Đếm số lượng đầu sách theo từng thể loại để hiển thị kiểm soát toàn vẹn
            var bookCounts = new Dictionary<int, int>();
            foreach (var c in listCategory)
            {
                bookCounts[c.CategoryID] = _context.Books.Count(b => b.CategoryID == c.CategoryID);
            }
            ViewBag.BookCounts = bookCounts;

            return View(listCategory);
        }

        // 2. THÊM MỚI DANH MỤC (CREATE - GET)
        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult Create()
        {
            return View();
        }

        // 2. THÊM MỚI DANH MỤC (CREATE - POST)
        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult Create(tblCategory category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError("CategoryName", "Tên thể loại không được để trống!");
            }

            if (ModelState.IsValid)
            {
                category.IsActive = true;
                _context.Categories.Add(category);
                _context.SaveChanges();
                TempData["Success"] = $"Thêm mới thể loại [{category.CategoryName}] thành công!";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // 3. CHỈNH SỬA (EDIT - GET)
        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var category = _context.Categories.Find(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // 3. CHỈNH SỬA (EDIT - POST)
        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult Edit(tblCategory category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(category);
                _context.SaveChanges();
                TempData["Success"] = $"Cập nhật thể loại [{category.CategoryName}] thành công!";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // 4. BẬT / TẮT TRẠNG THÁI (TOGGLE ACTIVE)
        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult ToggleActive(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null) return NotFound();

            category.IsActive = !category.IsActive;
            _context.SaveChanges();

            string statusText = category.IsActive ? "Kích hoạt" : "Ẩn (Xóa mềm)";
            TempData["Success"] = $"Đã chuyển trạng thái thể loại [{category.CategoryName}] thành: {statusText}!";
            return RedirectToAction("Index");
        }

        // 5. XÓA THỂ LOẠI (DELETE - POST) - KIỂM TRA KHÓA NGOẠI & ÁP DỤNG SOFT DELETE
        [HttpPost]
        [Authorize(Roles = "Admin")] // Giới hạn chỉ Admin tối cao
        public IActionResult Delete(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy thể loại cần xóa!";
                return RedirectToAction("Index");
            }

            // KIỂM TRA RÀNG BUỘC KHÓA NGOẠI: Có đầu sách nào thuộc thể loại này không
            int linkedBooks = _context.Books.Count(b => b.CategoryID == id);
            if (linkedBooks > 0)
            {
                // Áp dụng SOFT DELETE để không phá vỡ khóa ngoại tblBook.CategoryID
                category.IsActive = false;
                _context.SaveChanges();
                TempData["Success"] = $"Thể loại [{category.CategoryName}] đang có {linkedBooks} đầu sách liên kết. Để bảo toàn toàn vẹn dữ liệu sách, hệ thống đã chuyển sang chế độ XÓA MỀM (Ẩn danh mục)!";
                return RedirectToAction("Index");
            }

            // Không có sách liên kết -> Xóa cứng an toàn
            _context.Categories.Remove(category);
            _context.SaveChanges();
            TempData["Success"] = $"Đã xóa thành công thể loại [{category.CategoryName}]!";
            return RedirectToAction("Index");
        }
    }
}
