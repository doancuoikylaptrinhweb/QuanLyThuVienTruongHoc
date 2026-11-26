using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Globalization;
using LibraryManagement.Models;
using LibraryManagement.Utilities;

namespace LibraryManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _context;

        public HomeController(DataContext context)
        {
            _context = context;
        }

        // =====================================================================
        // 1. TRANG CHỦ (/Home, /) & TRA CỨU SÁCH NÂNG CAO (/Lookup)
        // =====================================================================
        [Route("/")]
        [Route("/Home")]
        [Route("/Home/Index")]
        public IActionResult Index(string? searchString, int? categoryId)
        {
            var filteredBooks = FilterBooks(searchString, categoryId, null, null);
            PopulateCatalogViewBags();

            // Trang chủ hiển thị tối đa 8 sách nổi bật/mới nhất nếu không đang lọc
            var featuredBooks = (string.IsNullOrWhiteSpace(searchString) && (!categoryId.HasValue || categoryId.Value <= 0))
                ? filteredBooks.Take(8).ToList()
                : filteredBooks;

            ViewBag.SearchString = searchString;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.TotalFilteredBooks = filteredBooks.Count;

            return View(featuredBooks);
        }

        [Route("/Lookup")]
        [HttpGet]
        public IActionResult Lookup(string? searchString, int? categoryId, int? authorId, bool? onlyAvailable)
        {
            var matchedBooks = FilterBooks(searchString, categoryId, authorId, onlyAvailable);
            PopulateCatalogViewBags();

            ViewBag.SearchString = searchString;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedAuthor = authorId;
            ViewBag.OnlyAvailable = onlyAvailable == true;
            ViewBag.AuthorsList = _context.Authors
                .Where(a => a.IsActive == true)
                .OrderBy(a => a.AuthorName)
                .ToList();

            return View(matchedBooks);
        }

        [Route("/Rules")]
        [HttpGet]
        public IActionResult Rules()
        {
            return View();
        }

        [Route("/About")]
        [HttpGet]
        public IActionResult About()
        {
            ViewBag.TotalBooks = _context.Books.Count(b => b.IsActive == true);
            ViewBag.TotalCopies = _context.Books.Where(b => b.IsActive == true).Sum(b => (int?)b.Quantity) ?? 0;
            ViewBag.TotalReaders = _context.Users.Count(u => u.Role == "Reader" && u.IsActive == true);
            ViewBag.TotalCategories = _context.Categories.Count(c => c.IsActive == true);
            return View();
        }

        private List<tblBook> FilterBooks(string? searchString, int? categoryId, int? authorId, bool? onlyAvailable)
        {
            var books = _context.Books
                .Include(b => b.Author)
                .Where(b => b.IsActive == true)
                .OrderByDescending(b => b.BookID)
                .ToList();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                books = books.Where(b => b.CategoryID == categoryId.Value).ToList();
            }

            if (authorId.HasValue && authorId.Value > 0)
            {
                books = books.Where(b => b.AuthorID == authorId.Value).ToList();
            }

            if (onlyAvailable == true)
            {
                books = books.Where(b => b.AvailableQuantity > 0).ToList();
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var termNorm = NormalizeString(searchString);
                var termRaw = searchString.Trim().ToLowerInvariant();

                books = books.Where(b =>
                {
                    if (b.Title != null)
                    {
                        if (b.Title.ToLowerInvariant().Contains(termRaw)) return true;
                        if (NormalizeString(b.Title).Contains(termNorm)) return true;
                    }
                    if (b.BookCode != null && b.BookCode.ToLowerInvariant().Contains(termRaw)) return true;
                    if (b.Code.ToLowerInvariant().Contains(termRaw)) return true;
                    if (b.Summary != null)
                    {
                        if (b.Summary.ToLowerInvariant().Contains(termRaw)) return true;
                        if (NormalizeString(b.Summary).Contains(termNorm)) return true;
                    }
                    if (b.Contents != null && b.Contents.ToLowerInvariant().Contains(termRaw)) return true;
                    if (b.Author?.AuthorName != null)
                    {
                        if (b.Author.AuthorName.ToLowerInvariant().Contains(termRaw)) return true;
                        if (NormalizeString(b.Author.AuthorName).Contains(termNorm)) return true;
                    }
                    return false;
                }).ToList();
            }

            return books;
        }

        private void PopulateCatalogViewBags()
        {
            var categories = _context.Categories
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.CategoryID)
                .ToList();

            var categoryBookCounts = _context.Books
                .Where(b => b.IsActive == true && b.CategoryID > 0)
                .GroupBy(b => b.CategoryID)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.Categories = categories;
            ViewBag.CategoryBookCounts = categoryBookCounts;
            ViewBag.Authors = _context.Authors.ToDictionary(a => a.AuthorID, a => a.AuthorName ?? "Chưa rõ");

            ViewBag.TotalBooks = _context.Books.Count(b => b.IsActive == true);
            ViewBag.TotalCopies = _context.Books.Where(b => b.IsActive == true).Sum(b => (int?)b.Quantity) ?? 0;
            ViewBag.TotalReaders = _context.Users.Count(u => u.Role == "Reader" && u.IsActive == true);
        }

        // =====================================================================
        // Helper: Chuẩn hóa chuỗi (loại bỏ dấu, lowercase)
        // =====================================================================
        private static string NormalizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var normalized = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        // Helper: Escape ký tự đặc biệt trong SQL LIKE
        private static string EscapeLike(string pattern)
        {
            return pattern.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
        }

        // =====================================================================
        // 2. XEM CHI TIẾT SÁCH
        // =====================================================================
        [Route("/Book/Detail/{id:long}")]
        [Route("/sach-{slug}-{id:long}.html", Name = "BookDetails")]
        public IActionResult Details(long? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var book = _context.Books.FirstOrDefault(b => b.BookID == id && b.IsActive == true);
            if (book == null)
            {
                return NotFound();
            }

            ViewBag.Category = _context.Categories.Find(book.CategoryID);
            ViewBag.Author = _context.Authors.Find(book.AuthorID);
            ViewBag.Publisher = _context.Publishers.Find(book.PublisherID);

            return View(book);
        }

        // =====================================================================
        // 3. CHATBOT
        // =====================================================================
        [HttpPost]
        public IActionResult ChatBot(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new { reply = "Xin chào! Tôi có thể giúp gì cho bạn trong việc tìm sách và tra cứu hôm nay?" });
            }

            string msg = message.ToLower().Trim();

            if (msg.Contains("chào") || msg.Contains("hello") || msg.Contains("hi") || msg.Contains("bạn là ai"))
            {
                return Json(new
                {
                    reply = "Xin chào bạn! Tôi là <strong>Trợ lý AI Thư Viện</strong>. Tôi có thể hỗ trợ bạn tìm nhanh vị trí sách trên kệ, gợi ý sách chuyên ngành và hướng dẫn quy định mượn/trả sách trực tuyến 24/7!"
                });
            }

            if (msg.Contains("mượn") || msg.Contains("hạn") || msg.Contains("trả") || msg.Contains("thủ tục") || msg.Contains("quy định"))
            {
                return Json(new
                {
                    reply = "📚 <strong>Quy định mượn sách thư viện:</strong><br/>" +
                            "• Thời hạn mượn: Tối đa <strong>14 ngày</strong> / lượt.<br/>" +
                            "• Số lượng: Tối đa <strong>3 cuốn sách</strong> cùng một thời điểm.<br/>" +
                            "• Gia hạn: Bạn có thể bấm nút <em>'Phiếu mượn của tôi'</em> trên thanh menu để gia hạn trực tuyến thêm 7 ngày!"
                });
            }

            var activeBooks = _context.Books.Where(b => b.IsActive == true).ToList();
            var authors = _context.Authors.ToDictionary(a => a.AuthorID, a => a.AuthorName ?? "Chưa rõ");

            var matched = activeBooks.FirstOrDefault(b => b.Title != null && msg.Contains(b.Title.ToLower()));

            if (matched != null)
            {
                string authorName = authors.ContainsKey(matched.AuthorID) ? authors[matched.AuthorID] : "Tác giả thư viện";
                return Json(new { reply = $"📖 Cuốn <strong>{matched.Title}</strong> của {authorName} hiện <strong>còn {matched.AvailableQuantity} cuốn</strong> trên kệ.<br/>📍 <em>Mã sách: {matched.Code}</em>." });
            }

            if (msg.Contains("sách") || msg.Contains("gợi ý") || msg.Contains("tìm") || msg.Contains("kho"))
            {
                if (activeBooks.Count > 0)
                {
                    string bookList = "Dưới đây là một số đầu sách đang sẵn sàng trên kệ:<br/>";
                    foreach (var b in activeBooks.Take(3))
                    {
                        bookList += $"• <strong>{b.Title}</strong> (Còn: {b.AvailableQuantity} cuốn)<br/>";
                    }
                    bookList += "<br/>👉 Bạn có thể dùng ô tìm kiếm trên trang chủ để tra cứu sách chi tiết!";
                    return Json(new { reply = bookList });
                }
                else
                {
                    return Json(new { reply = "Hiện tại thư viện đang cập nhật kho sách mới. Vui lòng quay lại sau ít phút!" });
                }
            }

            return Json(new
            {
                reply = "Tôi đã ghi nhận câu hỏi của bạn. Bạn có thể hỏi tôi về: <em>'Gợi ý sách'</em>, <em>'Quy định mượn sách'</em> hoặc gõ tên sách để được trợ giúp ngay!"
            });
        }

        // =====================================================================
        // 4. ĐĂNG NHẬP
        // =====================================================================
        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password, bool rememberMe = false)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!" });
            }

            string inputUserName = userName.Trim();
            var check = _context.Users.FirstOrDefault(u =>
                (u.UserName != null && u.UserName.ToLower() == inputUserName.ToLower()) ||
                (u.UserCode != null && u.UserCode.ToLower() == inputUserName.ToLower())
            );

            if (check == null)
            {
                return Json(new { success = false, message = "Tên đăng nhập hoặc mật khẩu không chính xác!" });
            }

            if (check.IsActive == false)
            {
                return Json(new { success = false, message = "Tài khoản của bạn hiện đang bị khóa!" });
            }

            bool isValid = PasswordSecurity.VerifyPassword(check, check.Password, password, out bool needRehash);
            if (!isValid)
            {
                return Json(new { success = false, message = "Tên đăng nhập hoặc mật khẩu không chính xác!" });
            }

            if (needRehash)
            {
                try
                {
                    check.Password = PasswordSecurity.HashPassword(check, password);
                    _context.SaveChanges();
                }
                catch { }
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, check.UserID.ToString()),
                new Claim(ClaimTypes.Name, check.UserName ?? string.Empty),
                new Claim(ClaimTypes.GivenName, check.FullName ?? check.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, check.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, check.Role ?? "Reader"),
                new Claim("UserCode", check.Code)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            Functions._UserID = check.UserID;
            Functions._UserName = check.FullName ?? check.UserName;
            Functions._Email = check.Email ?? string.Empty;
            Functions._Role = check.Role ?? "Reader";

            if (check.Role == "Admin" || check.Role == "Librarian")
            {
                return Json(new { success = true, redirectUrl = "/Admin", isAdmin = true });
            }

            return Json(new { success = true, redirectUrl = "/", isAdmin = false });
        }

        // =====================================================================
        // 5. ĐĂNG KÝ TÀI KHOẢN
        // =====================================================================
        [HttpPost]
        public async Task<IActionResult> Register(string userName, string fullName, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                return Json(new { success = false, message = "Vui lòng nhập tên đăng nhập và mật khẩu!" });
            }

            userName = userName.Trim();
            if (userName.Length < 3)
            {
                return Json(new { success = false, message = "Tên đăng nhập phải có ít nhất 3 ký tự!" });
            }

            if (password.Length < 6)
            {
                return Json(new { success = false, message = "Mật khẩu phải có ít nhất 6 ký tự!" });
            }

            if (password != confirmPassword)
            {
                return Json(new { success = false, message = "Mật khẩu xác nhận không khớp!" });
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.UserName != null && u.UserName.ToLower() == userName.ToLower());
            if (existingUser != null)
            {
                return Json(new { success = false, message = "Tên đăng nhập này đã được sử dụng! Vui lòng chọn tên khác." });
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                email = email.Trim();
                var existingEmail = _context.Users.FirstOrDefault(u => u.Email != null && u.Email.ToLower() == email.ToLower());
                if (existingEmail != null)
                {
                    return Json(new { success = false, message = "Địa chỉ email này đã được sử dụng!" });
                }
            }

            var newUser = new tblUser
            {
                UserName = userName,
                FullName = string.IsNullOrWhiteSpace(fullName) ? userName : fullName.Trim(),
                Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
                Role = "Reader",
                IsActive = true
            };

            newUser.Password = PasswordSecurity.HashPassword(newUser, password);

            try
            {
                _context.Users.Add(newUser);
                _context.SaveChanges();

                if (string.IsNullOrWhiteSpace(newUser.UserCode))
                {
                    newUser.UserCode = $"DG{newUser.UserID:D4}";
                    _context.Users.Update(newUser);
                    _context.SaveChanges();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, newUser.UserID.ToString()),
                    new Claim(ClaimTypes.Name, newUser.UserName ?? string.Empty),
                    new Claim(ClaimTypes.GivenName, newUser.FullName ?? newUser.UserName ?? string.Empty),
                    new Claim(ClaimTypes.Email, newUser.Email ?? string.Empty),
                    new Claim(ClaimTypes.Role, "Reader"),
                    new Claim("UserCode", newUser.Code)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity)
                );

                Functions._UserID = newUser.UserID;
                Functions._UserName = newUser.FullName;
                Functions._Email = newUser.Email ?? string.Empty;
                Functions._Role = "Reader";

                return Json(new
                {
                    success = true,
                    message = "Đăng ký tài khoản thành công!",
                    redirectUrl = "/",
                    userCode = newUser.UserCode ?? newUser.Code,
                    fullName = newUser.FullName ?? newUser.UserName,
                    userName = newUser.UserName,
                    role = "Độc giả (Reader)",
                    status = "Đã xác thực & Kích hoạt"
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "Có lỗi xảy ra trong quá trình tạo tài khoản: " + errorMsg });
            }
        }

        // =====================================================================
        // 6. ĐĂNG XUẤT
        // =====================================================================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Functions.ClearSession();
            return RedirectToAction("Index", "Home");
        }

        // =====================================================================
        // 7. TRA CỨU PHIẾU MƯỢN
        // =====================================================================
        [HttpGet]
        public IActionResult SearchTicket(string? keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                if (Functions.IsLogin() && Functions._Role == "Reader")
                {
                    keyword = Functions._UserName;
                }
                else
                {
                    return Json(new { success = false, message = "Vui lòng nhập Mã phiếu mượn (PM...), Mã trả (PT...), hoặc Mã bạn đọc (DG...) để tra cứu!" });
                }
            }

            string term = keyword.Trim().ToLower();
            int parsedTicketId = 0;
            bool isNumeric = int.TryParse(term, out parsedTicketId);

            var matchedUserIds = _context.Users
                .Where(u => (u.FullName != null && u.FullName.ToLower().Contains(term)) ||
                            (u.UserName != null && u.UserName.ToLower().Contains(term)) ||
                            (u.UserCode != null && u.UserCode.ToLower().Contains(term)))
                .Select(u => u.UserID).ToList();

            var matchedBookIds = _context.Books
                .Where(b => (b.Title != null && b.Title.ToLower().Contains(term)) ||
                            (b.BookCode != null && b.BookCode.ToLower().Contains(term)))
                .Select(b => b.BookID).ToList();

            var matchedTicketIdsFromBooks = _context.BorrowDetails
                .Where(d => matchedBookIds.Contains(d.BookID))
                .Select(d => d.TicketID).ToList();

            var tickets = _context.BorrowTickets
                .Where(t =>
                    (t.TicketCode != null && t.TicketCode.ToLower().Contains(term)) ||
                    (t.ReturnCode != null && t.ReturnCode.ToLower().Contains(term)) ||
                    matchedUserIds.Contains(t.UserID) ||
                    matchedTicketIdsFromBooks.Contains(t.TicketID) ||
                    (isNumeric && t.TicketID == parsedTicketId)
                )
                .OrderByDescending(t => t.TicketID)
                .Take(20)
                .ToList();

            if (tickets.Count == 0)
            {
                return Json(new { success = false, message = $"Không tìm thấy phiếu mượn/trả nào khớp với mã hoặc từ khóa '{keyword}'!" });
            }

            var ticketIds = tickets.Select(t => t.TicketID).ToList();
            var details = _context.BorrowDetails.Where(d => ticketIds.Contains(d.TicketID)).ToList();
            var bookIds = details.Select(d => d.BookID).Distinct().ToList();
            var books = _context.Books.Where(b => bookIds.Contains(b.BookID)).ToDictionary(b => b.BookID, b => b);
            var users = _context.Users.ToDictionary(u => u.UserID, u => u);

            var result = tickets.Select(t =>
            {
                var detail = details.FirstOrDefault(d => d.TicketID == t.TicketID);
                var book = detail != null && books.ContainsKey(detail.BookID) ? books[detail.BookID] : null;
                var user = users.ContainsKey(t.UserID) ? users[t.UserID] : null;

                return new
                {
                    ticketId = t.TicketID,
                    ticketCode = t.Code,
                    returnCode = t.DisplayReturnCode,
                    userCode = user?.Code ?? "DG----",
                    userName = user?.FullName ?? user?.UserName ?? "Bạn đọc",
                    bookCode = book?.Code ?? "MS----",
                    bookTitle = book?.Title ?? "Tài liệu thư viện",
                    borrowDate = t.BorrowDate.ToString("dd/MM/yyyy"),
                    dueDate = t.DueDate.ToString("dd/MM/yyyy"),
                    returnDate = t.ReturnDate.HasValue ? t.ReturnDate.Value.ToString("dd/MM/yyyy") : "---",
                    status = t.Status ?? "Đang mượn"
                };
            }).ToList();

            return Json(new { success = true, data = result });
        }
    }
}
