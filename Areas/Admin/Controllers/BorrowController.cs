using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Librarian")] // Phân quyền cấp CONTROLLER: Admin và Thủ thư
    public class BorrowController : AdminBaseController
    {
        private readonly DataContext _context;

        public BorrowController(DataContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH PHIẾU MƯỢN & TÌM KIẾM
        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
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
            var ticketIds = listTickets.Select(t => t.TicketID).ToList();

            // Lấy thông tin độc giả
            ViewBag.Users = _context.Users.ToDictionary(u => u.UserID, u => u);

            // Lấy toàn bộ danh sách chi tiết sách của các phiếu mượn
            var allDetails = _context.BorrowDetails.Where(d => ticketIds.Contains(d.TicketID)).ToList();
            ViewBag.TicketDetails = allDetails.GroupBy(d => d.TicketID).ToDictionary(g => g.Key, g => g.ToList());

            // Lấy thông tin sách
            var bookIds = allDetails.Select(d => d.BookID).Distinct().ToList();
            ViewBag.Books = _context.Books.Where(b => bookIds.Contains(b.BookID)).ToDictionary(b => b.BookID, b => b);

            return View(listTickets);
        }

        // 2. XEM CHI TIẾT PHIẾU MƯỢN & DANH SÁCH SÁCH CHI TIẾT
        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult Detail(int id)
        {
            var ticket = _context.BorrowTickets.Find(id);
            if (ticket == null)
            {
                TempData["Error"] = "Không tìm thấy phiếu mượn yêu cầu!";
                return RedirectToAction(nameof(Index));
            }

            var user = _context.Users.Find(ticket.UserID);
            ViewBag.User = user;

            var details = _context.BorrowDetails.Where(d => d.TicketID == id).ToList();
            var bookIds = details.Select(d => d.BookID).Distinct().ToList();
            var books = _context.Books.Where(b => bookIds.Contains(b.BookID)).ToDictionary(b => b.BookID, b => b);

            ViewBag.Details = details;
            ViewBag.Books = books;

            return View(ticket);
        }

        // API TÌM KIẾM ĐỘC GIẢ THEO MÃ HOẶC TÊN (DÙNG CHO Ô TÌM KIẾM KHI LẬP PHIẾU MƯỢN)
        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult SearchReaders(string? keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Json(new { success = true, data = Array.Empty<object>() });
            }

            string term = keyword.Trim().ToLowerInvariant();

            var matched = _context.Users
                .Where(u => u.IsActive == true)
                .ToList()
                .Where(u =>
                    u.Code.ToLowerInvariant().Contains(term) ||
                    (u.UserCode != null && u.UserCode.ToLowerInvariant().Contains(term)) ||
                    (u.FullName != null && u.FullName.ToLowerInvariant().Contains(term)) ||
                    (u.UserName != null && u.UserName.ToLowerInvariant().Contains(term)) ||
                    (u.Email != null && u.Email.ToLowerInvariant().Contains(term))
                )
                .OrderByDescending(u => u.Code.Equals(term, StringComparison.OrdinalIgnoreCase))
                .ThenBy(u => u.Role == "Reader" ? 0 : 1)
                .ThenBy(u => u.UserID)
                .Take(10)
                .Select(u => new
                {
                    userId = u.UserID,
                    code = u.Code,
                    fullName = u.FullName ?? u.UserName ?? "Bạn đọc",
                    userName = u.UserName ?? "",
                    email = u.Email ?? "",
                    roleLabel = u.Role == "Admin" ? "Quản trị" : (u.Role == "Librarian" ? "Thủ thư" : "Độc giả")
                })
                .ToList();

            return Json(new { success = true, data = matched });
        }

        // 3. LẬP PHIẾU MƯỢN MỚI (GET) - CHO PHÉP CHỌN NHIỀU ĐẦU SÁCH
        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult Create()
        {
            // Danh sách độc giả
            var users = _context.Users.Where(u => u.IsActive == true).ToList()
                .Select(u => new {
                    UserID = u.UserID,
                    DisplayText = $"[{u.Code}] {u.FullName ?? u.UserName} ({(u.Role == "Admin" ? "Quản trị" : (u.Role == "Librarian" ? "Thủ thư" : "Độc giả"))})"
                }).ToList();
            ViewBag.UserList = new SelectList(users, "UserID", "DisplayText");

            // Danh sách sách CÒN KHẢ DỤNG TRÊN KỆ
            var availableBooks = _context.Books
                .Where(b => b.IsActive == true && b.AvailableQuantity > 0)
                .OrderBy(b => b.BookID)
                .ToList();
            ViewBag.AvailableBooks = availableBooks;

            // Mã phiếu mượn dự kiến
            int nextId = (_context.BorrowTickets.Max(t => (int?)t.TicketID) ?? 0) + 1;
            ViewBag.PreviewTicketCode = $"PM{nextId:D4}";

            return View();
        }

        // 3. LẬP PHIẾU MƯỢN MỚI (POST) - CẬP NHẬT TỒN KHO NGUYÊN TỬ + TRANSACTION
        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult Create(tblBorrowTicket ticket, List<long>? selectedBookIds, long? bookId)
        {
            // Hỗ trợ cả danh sách nhiều sách hoặc 1 cuốn đơn lẻ
            var bookIds = new List<long>();
            if (selectedBookIds != null && selectedBookIds.Count > 0)
            {
                bookIds.AddRange(selectedBookIds);
            }
            else if (bookId.HasValue && bookId.Value > 0)
            {
                bookIds.Add(bookId.Value);
            }

            // Loại bỏ các ID trùng lặp nếu có
            bookIds = bookIds.Distinct().ToList();

            if (ticket.UserID <= 0)
            {
                ModelState.AddModelError("UserID", "Vui lòng chọn độc giả mượn sách!");
            }
            else
            {
                var borrower = _context.Users.Find(ticket.UserID);
                if (borrower == null || borrower.IsActive != true)
                {
                    ModelState.AddModelError("UserID", "Tài khoản độc giả không tồn tại hoặc hiện đang bị khóa!");
                }
            }

            if (bookIds.Count == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một cuốn sách để lập phiếu mượn!");
            }
            else
            {
                foreach (var bId in bookIds)
                {
                    var bk = _context.Books.Find(bId);
                    if (bk == null || bk.IsActive != true)
                    {
                        ModelState.AddModelError("", $"Cuốn sách ID #{bId} không tồn tại hoặc đã ngừng lưu hành (xóa mềm)!");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                // SỬ DỤNG DATABASE TRANSACTION BẢO ĐẢM TÍNH TOÀN VẸN ACID
                using var transaction = _context.Database.BeginTransaction();
                try
                {
                    ticket.BorrowDate = DateTime.Now;
                    ticket.Status = "Đang mượn";

                    if (string.IsNullOrWhiteSpace(ticket.TicketCode))
                    {
                        int nextId = (_context.BorrowTickets.Max(t => (int?)t.TicketID) ?? 0) + 1;
                        ticket.TicketCode = $"PM{nextId:D4}";
                    }

                    _context.BorrowTickets.Add(ticket);
                    _context.SaveChanges();

                    // CẬP NHẬT TỒN KHO NGUYÊN TỬ VÀ THÊM CHI TIẾT SÁCH
                    foreach (var bId in bookIds)
                    {
                        var bookInfo = _context.Books.Find(bId);
                        string bookTitle = bookInfo != null ? $"[{bookInfo.Code}] {bookInfo.Title}" : $"Sách #{bId}";

                        // CÂU LỆNH NGUYÊN TỬ: Kiểm tra và trừ kho trong 1 thao tác SQL duy nhất
                        // WHERE [BookID] = @bId AND [AvailableQuantity] > 0
                        int rowsAffected = _context.Database.ExecuteSqlInterpolated(
                            $"UPDATE [dbo].[tblBook] SET [AvailableQuantity] = [AvailableQuantity] - 1 WHERE [BookID] = {bId} AND [AvailableQuantity] > 0"
                        );

                        if (rowsAffected == 0)
                        {
                            // Nếu rowsAffected = 0 tức là sách đã hết ngay lúc thao tác -> Rollback!
                            throw new InvalidOperationException($"Cuốn sách '{bookTitle}' hiện không còn bản khả dụng trên kệ!");
                        }

                        // Lưu chi tiết sách mượn
                        var detail = new tblBorrowDetail
                        {
                            TicketID = ticket.TicketID,
                            BookID = bId,
                            FineAmount = 0,
                            IsReturned = false,
                            ReturnDate = null,
                            Note = null
                        };
                        _context.BorrowDetails.Add(detail);
                    }

                    _context.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = $"Lập thành công phiếu mượn [{ticket.Code}] gồm {bookIds.Count} cuốn sách với giao dịch nguyên tử an toàn!";
                    return RedirectToAction(nameof(Detail), new { id = ticket.TicketID });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = $"Không thể lập phiếu mượn: {ex.Message}";
                }
            }

            // Nạp lại dữ liệu nếu lỗi
            var users = _context.Users.Where(u => u.IsActive == true).ToList()
                .Select(u => new {
                    UserID = u.UserID,
                    DisplayText = $"[{u.Code}] {u.FullName ?? u.UserName}"
                }).ToList();
            ViewBag.UserList = new SelectList(users, "UserID", "DisplayText");
            ViewBag.AvailableBooks = _context.Books.Where(b => b.IsActive == true && b.AvailableQuantity > 0).OrderBy(b => b.BookID).ToList();

            return View(ticket);
        }

        // 4. HOÀN KHO TỪNG BẢN SÁCH RIÊNG LẺ - BẢO ĐẢM HOÀN KHO ĐÚNG 1 LẦN
        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult ReturnBookItem(int detailId, string? returnNote)
        {
            var detail = _context.BorrowDetails.Find(detailId);
            if (detail == null)
            {
                TempData["Error"] = "Không tìm thấy chi tiết sách cần hoàn kho!";
                return RedirectToAction(nameof(Index));
            }

            int ticketId = detail.TicketID;
            long bookId = detail.BookID;

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                DateTime now = DateTime.Now;

                // THAO TÁC NGUYÊN TỬ CÓ ĐIỀU KIỆN [IsReturned] = 0
                // Chỉ cập nhật nếu cuốn sách này CHƯA TỪNG được hoàn kho
                int updated = _context.Database.ExecuteSqlInterpolated(
                    $"UPDATE [dbo].[tblBorrowDetail] SET [IsReturned] = 1, [ReturnDate] = {now}, [Note] = {returnNote} WHERE [DetailID] = {detailId} AND [IsReturned] = 0"
                );

                if (updated == 0)
                {
                    // Đã được hoàn kho trước đó -> Rollback, không hoàn kho lần 2 (Idempotency)
                    transaction.Rollback();
                    TempData["Error"] = "Bản sách này đã được hoàn kho trước đó! Không thể hoàn kho lặp lại.";
                    return RedirectToAction(nameof(Detail), new { id = ticketId });
                }

                // CẬP NHẬT TĂNG TỒN KHO NGUYÊN TỬ (Chỉ chạy khi updated == 1)
                _context.Database.ExecuteSqlInterpolated(
                    $"UPDATE [dbo].[tblBook] SET [AvailableQuantity] = [AvailableQuantity] + 1 WHERE [BookID] = {bookId}"
                );

                // Kiểm tra trạng thái toàn bộ phiếu mượn
                var allDetails = _context.BorrowDetails.Where(d => d.TicketID == ticketId).ToList();
                // Đồng bộ trạng thái cuốn vừa trả
                detail.IsReturned = true;

                var ticket = _context.BorrowTickets.Find(ticketId);
                if (ticket != null)
                {
                    bool allReturned = allDetails.All(d => d.DetailID == detailId || d.IsReturned);
                    if (allReturned)
                    {
                        ticket.Status = "Đã trả";
                        ticket.ReturnDate = now;
                        if (string.IsNullOrWhiteSpace(ticket.ReturnCode))
                        {
                            ticket.ReturnCode = $"PT{ticket.TicketID:D4}";
                        }
                    }
                    else
                    {
                        ticket.Status = "Trả một phần";
                    }
                    _context.BorrowTickets.Update(ticket);
                    _context.SaveChanges();
                }

                transaction.Commit();
                var book = _context.Books.Find(bookId);
                TempData["Success"] = $"Đã hoàn kho thành công cuốn sách [{book?.Code ?? "MS"}] và tăng lại 1 tồn kho!";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                TempData["Error"] = $"Lỗi khi hoàn kho sách: {ex.Message}";
            }

            return RedirectToAction(nameof(Detail), new { id = ticketId });
        }

        // 5. HOÀN KHO TOÀN BỘ CÁC BẢN SÁCH CÒN LẠI TRONG PHIẾU MƯỢN
        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult ReturnTicket(int id, string? returnNote)
        {
            var ticket = _context.BorrowTickets.Find(id);
            if (ticket == null)
            {
                TempData["Error"] = "Không tìm thấy phiếu mượn yêu cầu!";
                return RedirectToAction(nameof(Index));
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                DateTime now = DateTime.Now;
                var unreturnedDetails = _context.BorrowDetails.Where(d => d.TicketID == id && !d.IsReturned).ToList();

                if (unreturnedDetails.Count == 0)
                {
                    TempData["Error"] = "Tất cả các bản sách trong phiếu này đã được hoàn kho trước đó!";
                    return RedirectToAction(nameof(Detail), new { id });
                }

                int countReturned = 0;
                foreach (var d in unreturnedDetails)
                {
                    // CẬP NHẬT NGUYÊN TỬ CÓ ĐIỀU KIỆN: Chỉ cập nhật khi IsReturned = 0
                    int updated = _context.Database.ExecuteSqlInterpolated(
                        $"UPDATE [dbo].[tblBorrowDetail] SET [IsReturned] = 1, [ReturnDate] = {now}, [Note] = {returnNote} WHERE [DetailID] = {d.DetailID} AND [IsReturned] = 0"
                    );

                    if (updated > 0)
                    {
                        // Hoàn kho đúng 1 lần cho từng cuốn
                        _context.Database.ExecuteSqlInterpolated(
                            $"UPDATE [dbo].[tblBook] SET [AvailableQuantity] = [AvailableQuantity] + 1 WHERE [BookID] = {d.BookID}"
                        );
                        countReturned++;
                    }
                }

                // Cập nhật trạng thái phiếu mượn sang hoàn tất
                ticket.Status = "Đã trả";
                ticket.ReturnDate = now;
                if (string.IsNullOrWhiteSpace(ticket.ReturnCode))
                {
                    ticket.ReturnCode = $"PT{ticket.TicketID:D4}";
                }
                _context.BorrowTickets.Update(ticket);
                _context.SaveChanges();

                transaction.Commit();
                TempData["Success"] = $"Hoàn kho thành công {countReturned} bản sách! Đã cấp Mã trả sách: [{ticket.ReturnCode}].";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                TempData["Error"] = $"Lỗi khi hoàn kho phiếu mượn: {ex.Message}";
            }

            return RedirectToAction(nameof(Detail), new { id });
        }

        // 6. ACTION COMPATIBILITY: Hỗ trợ nút trả nhanh từ trang Index
        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult ReturnBook(int id)
        {
            return ReturnTicket(id, "Hoàn kho toàn bộ qua nút thao tác nhanh");
        }

        // ==================== NEW: APPROVAL WORKFLOW ====================
        // 7. LIST PENDING BORROW & RETURN REQUESTS (Admin/Librarian)
        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult PendingRequests()
        {
            var pending = _context.BorrowTickets
                .Where(t => t.Status == "Pending" || t.Status == "Chờ duyệt trả")
                .OrderByDescending(t => t.TicketID)
                .ToList();

            // Load related details and books for display
            var ticketIds = pending.Select(t => t.TicketID).ToList();
            var allDetails = _context.BorrowDetails.Where(d => ticketIds.Contains(d.TicketID)).ToList();
            ViewBag.TicketDetails = allDetails.GroupBy(d => d.TicketID).ToDictionary(g => g.Key, g => g.ToList());
            var bookIds = allDetails.Select(d => d.BookID).Distinct().ToList();
            ViewBag.Books = _context.Books.Where(b => bookIds.Contains(b.BookID)).ToDictionary(b => b.BookID, b => b);
            ViewBag.Users = _context.Users.ToDictionary(u => u.UserID, u => u);
            return View(pending);
        }

        // 8. APPROVE BORROW REQUEST
        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult ApproveRequest(int id)
        {
            var ticket = _context.BorrowTickets.Find(id);
            if (ticket == null || ticket.Status != "Pending")
            {
                TempData["Error"] = "Phiếu không tồn tại hoặc không ở trạng thái chờ duyệt.";
                return RedirectToAction(nameof(PendingRequests));
            }

            var details = _context.BorrowDetails.Where(d => d.TicketID == id).ToList();
            if (!details.Any())
            {
                TempData["Error"] = "Phiếu không có chi tiết sách để duyệt.";
                return RedirectToAction(nameof(PendingRequests));
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // Update ticket info
                ticket.Status = "Đang mượn";
                ticket.BorrowDate = DateTime.Now;
                if (ticket.DueDate <= ticket.BorrowDate)
                {
                    ticket.DueDate = DateTime.Now.AddDays(14);
                }
                // Ensure TicketCode exists
                if (string.IsNullOrWhiteSpace(ticket.TicketCode))
                {
                    int nextId = (_context.BorrowTickets.Max(t => (int?)t.TicketID) ?? 0) + 1;
                    ticket.TicketCode = $"PM{nextId:D4}";
                }
                _context.BorrowTickets.Update(ticket);

                // Decrement stock for each book atomically
                foreach (var d in details)
                {
                    int rowsAffected = _context.Database.ExecuteSqlInterpolated(
                        $"UPDATE [dbo].[tblBook] SET [AvailableQuantity] = [AvailableQuantity] - 1 WHERE [BookID] = {d.BookID} AND [AvailableQuantity] > 0");
                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException($"Sách ID {d.BookID} không còn tồn kho để duyệt phiếu.");
                    }
                }

                _context.SaveChanges();
                transaction.Commit();
                TempData["Success"] = $"Đã duyệt phiếu mượn [{ticket.Code}] và giảm tồn kho cho các cuốn sách.";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                TempData["Error"] = $"Lỗi duyệt phiếu: {ex.Message}";
            }
            return RedirectToAction(nameof(PendingRequests));
        }

        // 9. REJECT BORROW REQUEST
        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult RejectRequest(int id, string? rejectReason)
        {
            var ticket = _context.BorrowTickets.Find(id);
            if (ticket == null || ticket.Status != "Pending")
            {
                TempData["Error"] = "Phiếu không tồn tại hoặc không ở trạng thái chờ duyệt.";
                return RedirectToAction(nameof(PendingRequests));
            }
            ticket.Status = "Từ chối";
            ticket.Note = string.IsNullOrWhiteSpace(rejectReason) ? "Thủ thư từ chối yêu cầu mượn" : $"Từ chối: {rejectReason.Trim()}";
            _context.BorrowTickets.Update(ticket);
            _context.SaveChanges();
            TempData["Success"] = $"Đã từ chối yêu cầu mượn sách [{ticket.Code}].";
            return RedirectToAction(nameof(PendingRequests));
        }

        // 10. APPROVE RETURN REQUEST (THỦ THƯ XÁC NHẬN NHẬN TRẢ SÁCH VÀ HOÀN KHO NGUYÊN TỬ)
        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult ApproveReturn(int id, string? returnNote)
        {
            var ticket = _context.BorrowTickets.Find(id);
            if (ticket == null)
            {
                TempData["Error"] = "Không tìm thấy phiếu mượn tương ứng.";
                return RedirectToAction(nameof(PendingRequests));
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                DateTime now = DateTime.Now;
                var unreturnedDetails = _context.BorrowDetails.Where(d => d.TicketID == id && !d.IsReturned).ToList();

                if (unreturnedDetails.Count == 0)
                {
                    TempData["Error"] = "Tất cả các bản sách trong phiếu này đã được hoàn kho trước đó!";
                    return RedirectToAction(nameof(PendingRequests));
                }

                int countReturned = 0;
                foreach (var d in unreturnedDetails)
                {
                    // CẬP NHẬT NGUYÊN TỬ CÓ ĐIỀU KIỆN [IsReturned] = 0
                    int updated = _context.Database.ExecuteSqlInterpolated(
                        $"UPDATE [dbo].[tblBorrowDetail] SET [IsReturned] = 1, [ReturnDate] = {now}, [Note] = {returnNote} WHERE [DetailID] = {d.DetailID} AND [IsReturned] = 0"
                    );

                    if (updated > 0)
                    {
                        // Hoàn kho đúng 1 lần cho từng cuốn
                        _context.Database.ExecuteSqlInterpolated(
                            $"UPDATE [dbo].[tblBook] SET [AvailableQuantity] = [AvailableQuantity] + 1 WHERE [BookID] = {d.BookID}"
                        );
                        countReturned++;
                    }
                }

                // Cập nhật trạng thái phiếu mượn sang hoàn tất
                ticket.Status = "Đã trả";
                ticket.ReturnDate = now;
                if (string.IsNullOrWhiteSpace(ticket.ReturnCode))
                {
                    ticket.ReturnCode = $"PT{ticket.TicketID:D4}";
                }
                if (!string.IsNullOrWhiteSpace(returnNote))
                {
                    string cleanNote = returnNote.Trim();
                    ticket.Note = string.IsNullOrWhiteSpace(ticket.Note) ? cleanNote : $"{ticket.Note} | Đã nhận trả: {cleanNote}";
                }
                _context.BorrowTickets.Update(ticket);
                _context.SaveChanges();

                transaction.Commit();
                TempData["Success"] = $"Xác nhận trả sách thành công! Đã hoàn kho {countReturned} cuốn sách cho phiếu [{ticket.Code}] và cấp Mã trả sách: [{ticket.ReturnCode}].";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                TempData["Error"] = $"Lỗi khi xác nhận hoàn kho: {ex.Message}";
            }

            return RedirectToAction(nameof(PendingRequests));
        }

        // ==================== END NEW ====================
        }
}
