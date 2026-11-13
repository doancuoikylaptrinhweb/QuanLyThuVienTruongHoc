using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly DataContext _context;

        public CategoryController(DataContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ DANH SÁCH (READ)
        public IActionResult Index()
        {
            var listCategory = _context.Categories.OrderByDescending(c => c.CategoryID).ToList();
            return View(listCategory);
        }

        // 2. THÊM MỚI DANH MỤC (CREATE - GET)
        public IActionResult Create()
        {
            return View();
        }

        // 2. THÊM MỚI DANH MỤC (CREATE - POST)
        [HttpPost]
        public IActionResult Create(tblCategory category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // 3. CHỈNH SỬA (EDIT - GET)
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // 3. CHỈNH SỬA (EDIT - POST)
        [HttpPost]
        public IActionResult Edit(tblCategory category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(category);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // 4. XÓA (DELETE - GET)
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // 4. XÓA (DELETE - POST)
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}