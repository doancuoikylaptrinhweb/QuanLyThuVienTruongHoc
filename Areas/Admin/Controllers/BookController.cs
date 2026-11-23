using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PagedList.Core;
using LibraryManagement.Models;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Librarian")] // Phân quyền cấp CONTROLLER: Admin và Thủ thư
    public class BookController : AdminBaseController
    {
        private readonly DataContext _context;

        public BookController(DataContext context)
        {
            _context = context;
        }

        // 1. HIỂN THỊ DANH SÁCH SÁCH CÓ TÌM KIẾM THEO MÃ & PHÂN TRANG (Lab 08)
        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
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

            ViewBag.Categories = _context.Categories.ToDictionary(c => c.CategoryID, c => c.CategoryName ?? "");
            ViewBag.Authors = _context.Authors.ToDictionary(a => a.AuthorID, a => a.AuthorName ?? "");
            ViewBag.Publishers = _context.Publishers.ToDictionary(p => p.PublisherID, p => p.PublisherName ?? "");

            // Thống kê tồn kho
            ViewBag.TotalTitles = _context.Books.Count();
            ViewBag.TotalCopies = _context.Books.Sum(b => (int?)b.Quantity) ?? 0;
            ViewBag.TotalAvailable = _context.Books.Sum(b => (int?)b.AvailableQuantity) ?? 0;
            ViewBag.TotalBorrowed = _context.BorrowDetails.Count(d => !d.IsReturned);

            return View(models);
        }

        // 2. THÊM MỚI SÁCH (GET)
        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult Create()
        {
            ViewBag.CategoryList = new SelectList(_context.Categories.Where(c => c.IsActive == true), "CategoryID", "CategoryName");
            ViewBag.AuthorList = new SelectList(_context.Authors.Where(a => a.IsActive == true), "AuthorID", "AuthorName");
            ViewBag.PublisherList = new SelectList(_context.Publishers.Where(p => p.IsActive == true), "PublisherID", "PublisherName");

            // Dự kiến mã sách
            long nextId = (_context.Books.Max(b => (long?)b.BookID) ?? 0) + 1;
            ViewBag.PreviewBookCode = $"MS{nextId:D4}";

            return View();
        }

        // 2. THÊM MỚI SÁCH (POST) - KIỂM TRA KHÓA NGOẠI & KHỞI TẠO TỒN KHO
        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult Create(tblBook book)
        {
            // Kiểm tra ràng buộc khóa ngoại (Foreign Key Integrity)
            if (!_context.Categories.Any(c => c.CategoryID == book.CategoryID))
            {
                ModelState.AddModelError("CategoryID", "Thể loại sách được chọn không tồn tại hoặc đã bị xóa!");
            }

            if (!_context.Authors.Any(a => a.AuthorID == book.AuthorID))
            {
                ModelState.AddModelError("AuthorID", "Tác giả được chọn không tồn tại hoặc đã bị xóa!");
            }

            if (!_context.Publishers.Any(p => p.PublisherID == book.PublisherID))
            {
                ModelState.AddModelError("PublisherID", "Nhà xuất bản được chọn không tồn tại hoặc đã bị xóa!");
            }

            if (book.Quantity < 0)
            {
                ModelState.AddModelError("Quantity", "Số lượng nhập kho không thể là số âm!");
            }

            if (ModelState.IsValid)
            {
                book.CreatedDate = DateTime.Now;
                book.IsActive = true;
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

                TempData["Success"] = $"Thêm mới thành công sách [{book.Code}] '{book.Title}' (Tổng kho: {book.Quantity})!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryList = new SelectList(_context.Categories.Where(c => c.IsActive == true), "CategoryID", "CategoryName");
            ViewBag.AuthorList = new SelectList(_context.Authors.Where(a => a.IsActive == true), "AuthorID", "AuthorName");
            ViewBag.PublisherList = new SelectList(_context.Publishers.Where(p => p.IsActive == true), "PublisherID", "PublisherName");
            return View(book);
        }

        // 3. CHỈNH SỬA SÁCH (GET)
        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult Edit(long id)
        {
            var book = _context.Books.Find(id);
            if (book == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin cuốn sách yêu cầu!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryList = new SelectList(_context.Categories, "CategoryID", "CategoryName", book.CategoryID);
            ViewBag.AuthorList = new SelectList(_context.Authors, "AuthorID", "AuthorName", book.AuthorID);
            ViewBag.PublisherList = new SelectList(_context.Publishers, "PublisherID", "PublisherName", book.PublisherID);

            int currentlyBorrowed = _context.BorrowDetails.Count(d => d.BookID == id && !d.IsReturned);
            ViewBag.CurrentlyBorrowed = currentlyBorrowed;

            return View(book);
        }

        // 3. CHỈNH SỬA SÁCH (POST) - ĐỒNG BỘ TỒN KHO & BẢO TOÀN DỮ LIỆU
        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult Edit(tblBook book)
        {
            var existingBook = _context.Books.Find(book.BookID);
            if (existingBook == null) return NotFound();

            // Kiểm tra số sách đang được mượn
            int currentlyBorrowed = _context.BorrowDetails.Count(d => d.BookID == book.BookID && !d.IsReturned);

            // Quy tắc bảo toàn dữ liệu: Tổng kho không thể nhỏ hơn số sách đang cho mượn
            if (book.Quantity < currentlyBorrowed)
            {
                ModelState.AddModelError("Quantity", $"Không thể giảm tổng số lượng xuống {book.Quantity} vì đang có {currentlyBorrowed} cuốn đang được bạn đọc mượn!");
            }

            // Kiểm tra khóa ngoại
            if (!_context.Categories.Any(c => c.CategoryID == book.CategoryID))
            {
                ModelState.AddModelError("CategoryID", "Thể loại được chọn không hợp lệ!");
            }
            if (!_context.Authors.Any(a => a.AuthorID == book.AuthorID))
            {
                ModelState.AddModelError("AuthorID", "Tác giả được chọn không hợp lệ!");
            }
            if (!_context.Publishers.Any(p => p.PublisherID == book.PublisherID))
            {
                ModelState.AddModelError("PublisherID", "Nhà xuất bản được chọn không hợp lệ!");
            }

            if (ModelState.IsValid)
            {
                existingBook.Title = book.Title;
                existingBook.CategoryID = book.CategoryID;
                existingBook.AuthorID = book.AuthorID;
                existingBook.PublisherID = book.PublisherID;
                existingBook.PublishYear = book.PublishYear;
                existingBook.Summary = book.Summary;
                existingBook.Contents = book.Contents;
                existingBook.Images = book.Images;
                existingBook.PostOrder = book.PostOrder;
                existingBook.IsActive = book.IsActive ?? true;

                // Đồng bộ số lượng tồn kho: AvailableQuantity = Quantity mới - Số sách đang cho mượn
                existingBook.Quantity = book.Quantity;
                existingBook.AvailableQuantity = Math.Max(0, book.Quantity - currentlyBorrowed);

                _context.SaveChanges();
                TempData["Success"] = $"Cập nhật thành công sách [{existingBook.Code}]! Tồn kho khả dụng: {existingBook.AvailableQuantity}/{existingBook.Quantity}.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryList = new SelectList(_context.Categories, "CategoryID", "CategoryName", book.CategoryID);
            ViewBag.AuthorList = new SelectList(_context.Authors, "AuthorID", "AuthorName", book.AuthorID);
            ViewBag.PublisherList = new SelectList(_context.Publishers, "PublisherID", "PublisherName", book.PublisherID);
            ViewBag.CurrentlyBorrowed = currentlyBorrowed;

            return View(book);
        }

        // 4. BẬT / TẮT TRẠNG THÁI LƯU HÀNH SÁCH (ACTIVE / INACTIVE)
        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult ToggleActive(long id)
        {
            var book = _context.Books.Find(id);
            if (book == null) return NotFound();

            book.IsActive = !(book.IsActive ?? false);
            _context.SaveChanges();

            string statusText = (book.IsActive == true) ? "Hiển thị lưu hành" : "Ẩn khỏi danh mục";
            TempData["Success"] = $"Đã chuyển trạng thái sách [{book.Code}] thành: {statusText}!";
            return RedirectToAction(nameof(Index));
        }

        // 5. XÓA SÁCH: BẢO TOÀN DỮ LIỆU & ÁP DỤNG SOFT DELETE KHI ĐÃ CÓ LỊCH SỬ
        [HttpPost]
        [Authorize(Roles = "Admin")] // Giới hạn chỉ Admin tối cao
        public IActionResult Delete(long id)
        {
            var book = _context.Books.Find(id);
            if (book == null)
            {
                TempData["Error"] = "Không tìm thấy cuốn sách cần xóa!";
                return RedirectToAction(nameof(Index));
            }

            // QUY TẮC 1: Nếu sách đang có người mượn chưa trả -> TUYỆT ĐỐI CHẶN XÓA
            int unreturnedCount = _context.BorrowDetails.Count(d => d.BookID == id && !d.IsReturned);
            if (unreturnedCount > 0)
            {
                TempData["Error"] = $"Không thể xóa sách [{book.Code}] vì đang có {unreturnedCount} cuốn đang được mượn chưa hoàn kho! Vui lòng thu hồi sách trước.";
                return RedirectToAction(nameof(Index));
            }

            // QUY TẮC 2: Nếu sách đã từng có lịch sử mượn trả -> ÁP DỤNG SOFT DELETE ĐỂ BẢO TOÀN DỮ LIỆU
            bool hasBorrowHistory = _context.BorrowDetails.Any(d => d.BookID == id);
            if (hasBorrowHistory)
            {
                book.IsActive = false; // Xóa mềm: Ngừng lưu hành
                _context.SaveChanges();
                TempData["Success"] = $"Sách [{book.Code}] '{book.Title}' đã có lịch sử mượn trả trong hệ thống. Để bảo toàn toàn vẹn dữ liệu tham chiếu, hệ thống đã chuyển sang chế độ XÓA MỀM (Ngừng lưu hành)!";
                return RedirectToAction(nameof(Index));
            }

            // QUY TẮC 3: Sách mới chưa từng phát sinh bất kỳ liên kết phiếu mượn nào -> Xóa cứng an toàn
            _context.Books.Remove(book);
            _context.SaveChanges();
            TempData["Success"] = $"Đã xóa vĩnh viễn sách [{book.Code}] khỏi thư viện!";
            return RedirectToAction(nameof(Index));
        }

        // 6. KIỂM ĐỊNH & ĐỒNG BỘ TỒN KHO NGUYÊN TỬ (Inventory Reconciliation & Self-Healing)
        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult SyncInventory()
        {
            var allBooks = _context.Books.ToList();
            int correctedCount = 0;

            foreach (var b in allBooks)
            {
                // Đếm chính xác số cuốn đang được mượn thực tế
                int currentlyBorrowed = _context.BorrowDetails.Count(d => d.BookID == b.BookID && !d.IsReturned);
                int realAvailable = Math.Max(0, b.Quantity - currentlyBorrowed);

                if (b.AvailableQuantity != realAvailable)
                {
                    b.AvailableQuantity = realAvailable;
                    correctedCount++;
                }
            }

            if (correctedCount > 0)
            {
                _context.SaveChanges();
                TempData["Success"] = $"Đồng bộ tồn kho hoàn tất! Đã hiệu chỉnh lại số lượng khả dụng cho {correctedCount} đầu sách có sai lệch.";
            }
            else
            {
                TempData["Success"] = $"Kiểm định kho hoàn tất: Toàn bộ {allBooks.Count} đầu sách đều đồng bộ chính xác 100% với dữ liệu mượn trả!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
