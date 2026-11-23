using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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

        // 1. TRANG CHỦ & TRA CỨU SÁCH TRỰC TUYẾN (DỮ LIỆU 100% TRUY VẤN TỪ DATABASE)
        public IActionResult Index(string? searchString, int? categoryId)
        {
            var booksQuery = _context.Books.Where(b => b.IsActive == true);

            // Lọc theo từ khóa tìm kiếm (hỗ trợ tìm theo tên sách, tóm tắt, hoặc mã sách như MS0001)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                string term = searchString.Trim().ToLower();
                booksQuery = booksQuery.Where(b => 
                    (b.Title != null && b.Title.ToLower().Contains(term)) ||
                    (b.Summary != null && b.Summary.ToLower().Contains(term)) ||
                    (b.BookCode != null && b.BookCode.ToLower().Contains(term)) ||
                    (b.Contents != null && b.Contents.ToLower().Contains(term))
                );
                ViewBag.SearchString = searchString;
            }

            // Lọc theo thể loại sách
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                booksQuery = booksQuery.Where(b => b.CategoryID == categoryId.Value);
                ViewBag.SelectedCategory = categoryId.Value;
            }

            var allBooks = booksQuery.OrderByDescending(b => b.BookID).ToList();

            // Toàn bộ danh mục và tác giả lấy 100% từ Database
            ViewBag.Categories = _context.Categories.Where(c => c.IsActive == true).OrderBy(c => c.CategoryID).ToList();
            ViewBag.Authors = _context.Authors.ToDictionary(a => a.AuthorID, a => a.AuthorName ?? "Chưa rõ");

            // Chỉ số thống kê thời gian thực từ Database
            ViewBag.TotalBooks = _context.Books.Count(b => b.IsActive == true);
            ViewBag.TotalCopies = _context.Books.Where(b => b.IsActive == true).Sum(b => (int?)b.Quantity) ?? 0;
            ViewBag.TotalReaders = _context.Users.Count(u => u.Role == "Reader" && u.IsActive == true);

            return View(allBooks);
        }

        // 2. XEM CHI TIẾT SÁCH (100% TỪ DATABASE)
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

            // Thông tin Tác giả, Thể loại, NXB từ Database
            ViewBag.Category = _context.Categories.Find(book.CategoryID);
            ViewBag.Author = _context.Authors.Find(book.AuthorID);
            ViewBag.Publisher = _context.Publishers.Find(book.PublisherID);

            return View(book);
        }

        // 3. XỬ LÝ CHATBOT VÀ HỖ TRỢ AI (TRA CỨU TRỰC TIẾP TỪ DATABASE)
        [HttpPost]
        public IActionResult ChatBot(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new { reply = "Xin chào! Tôi có thể giúp gì cho bạn trong việc tìm sách và tra cứu hôm nay?" });
            }

            string msg = message.ToLower().Trim();

            // 1. Chào hỏi
            if (msg.Contains("chào") || msg.Contains("hello") || msg.Contains("hi") || msg.Contains("bạn là ai"))
            {
                return Json(new { 
                    reply = "Xin chào bạn! Tôi là <strong>Trợ lý AI Thư Viện</strong>. Tôi có thể hỗ trợ bạn tìm nhanh vị trí sách trên kệ, gợi ý sách chuyên ngành và hướng dẫn quy định mượn/trả sách trực tuyến 24/7!" 
                });
            }

            // 2. Hỏi về thủ tục & quy định mượn sách
            if (msg.Contains("mượn") || msg.Contains("hạn") || msg.Contains("trả") || msg.Contains("thủ tục") || msg.Contains("quy định"))
            {
                return Json(new { 
                    reply = "📚 <strong>Quy định mượn sách thư viện:</strong><br/>" +
                            "• Thời hạn mượn: Tối đa <strong>14 ngày</strong> / lượt.<br/>" +
                            "• Số lượng: Tối đa <strong>3 cuốn sách</strong> cùng một thời điểm.<br/>" +
                            "• Gia hạn: Bạn có thể bấm nút <em>'Phiếu mượn của tôi'</em> trên thanh menu để gia hạn trực tuyến thêm 7 ngày!" 
                });
            }

            // 3. Tra cứu sách thực tế trong CSDL
            var activeBooks = _context.Books.Where(b => b.IsActive == true).ToList();
            var authors = _context.Authors.ToDictionary(a => a.AuthorID, a => a.AuthorName ?? "Chưa rõ");

            // Tìm sách phù hợp với câu hỏi trong CSDL
            var matched = activeBooks.FirstOrDefault(b => b.Title != null && msg.Contains(b.Title.ToLower()));
            if (matched == null && msg.Contains("clean code"))
            {
                matched = activeBooks.FirstOrDefault(b => b.Title != null && b.Title.ToLower().Contains("clean code"));
            }
            if (matched == null && msg.Contains("harry potter"))
            {
                matched = activeBooks.FirstOrDefault(b => b.Title != null && b.Title.ToLower().Contains("harry potter"));
            }
            if (matched == null && msg.Contains("mắt biếc"))
            {
                matched = activeBooks.FirstOrDefault(b => b.Title != null && b.Title.ToLower().Contains("mắt biếc"));
            }

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

            return Json(new { 
                reply = "Tôi đã ghi nhận câu hỏi của bạn. Bạn có thể hỏi tôi về: <em>'Gợi ý sách'</em>, <em>'Quy định mượn sách'</em> hoặc gõ tên sách để được trợ giúp ngay!" 
            });
        }

        // 4. ĐĂNG NHẬP DÀNH CHO BẠN ĐỌC / KHÁCH TRUY CẬP (Cookie Authentication + PasswordHasher)
        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!" });
            }

            string inputUserName = userName.Trim();
            var check = _context.Users.FirstOrDefault(u => u.UserName != null && u.UserName.ToLower() == inputUserName.ToLower());

            if (check == null)
            {
                return Json(new { success = false, message = "Tên đăng nhập hoặc mật khẩu không chính xác!" });
            }

            if (check.IsActive == false)
            {
                return Json(new { success = false, message = "Tài khoản của bạn hiện đang bị khóa!" });
            }

            // Kiểm tra mật khẩu an toàn bằng PasswordHasher (PBKDF2) hoặc tương thích MD5 cũ
            bool isValid = PasswordSecurity.VerifyPassword(check, check.Password, password, out bool needRehash);
            if (!isValid)
            {
                return Json(new { success = false, message = "Tên đăng nhập hoặc mật khẩu không chính xác!" });
            }

            // Tự động nâng cấp hash mật khẩu nếu người dùng dùng mật khẩu cũ
            if (needRehash)
            {
                try
                {
                    check.Password = PasswordSecurity.HashPassword(check, password);
                    _context.SaveChanges();
                }
                catch { }
            }

            // Thiết lập Cookie Claims
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
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            // Cập nhật fallback để tương thích các chức năng cũ
            Functions._UserID = check.UserID;
            Functions._UserName = check.FullName ?? check.UserName;
            Functions._Email = check.Email ?? string.Empty;
            Functions._Role = check.Role ?? "Reader";

            // Nếu là tài khoản Admin / Thủ thư -> chuyển hướng vào trang quản trị /Admin
            if (check.Role == "Admin" || check.Role == "Librarian")
            {
                return Json(new { success = true, redirectUrl = "/Admin", isAdmin = true });
            }

            // Nếu là Độc giả bình thường -> ở lại trang chủ độc giả
            return Json(new { success = true, redirectUrl = "/", isAdmin = false });
        }

        // 5. ĐĂNG KÝ TÀI KHOẢN BẠN ĐỌC MỚI
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

            // Kiểm tra trùng username
            var existingUser = _context.Users.FirstOrDefault(u => u.UserName != null && u.UserName.ToLower() == userName.ToLower());
            if (existingUser != null)
            {
                return Json(new { success = false, message = "Tên đăng nhập này đã được sử dụng! Vui lòng chọn tên khác." });
            }

            // Kiểm tra trùng email nếu có nhập
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

            // Băm mật khẩu bằng PasswordHasher (PBKDF2 HMAC-SHA256)
            newUser.Password = PasswordSecurity.HashPassword(newUser, password);

            try
            {
                _context.Users.Add(newUser);
                _context.SaveChanges();

                // Tự động gán Mã bạn đọc chuẩn: DG0001
                if (string.IsNullOrWhiteSpace(newUser.UserCode))
                {
                    newUser.UserCode = $"DG{newUser.UserID:D4}";
                    _context.Users.Update(newUser);
                    _context.SaveChanges();
                }

                // Tự động ký Cookie Authentication cho bạn đọc mới đăng ký
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

                return Json(new { success = true, message = "Đăng ký tài khoản thành công!", redirectUrl = "/" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra trong quá trình tạo tài khoản: " + ex.Message });
            }
        }

        // 6. ĐĂNG XUẤT CHO BẠN ĐỌC TRÊN TRANG CHỦ (Cookie Authentication)
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Functions.ClearSession();
            return RedirectToAction("Index", "Home");
        }

        // 7. TRA CỨU PHIẾU MƯỢN / PHIẾU TRẢ BẰNG MÃ
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

            // Tìm người dùng phù hợp
            var matchedUserIds = _context.Users
                .Where(u => (u.FullName != null && u.FullName.ToLower().Contains(term)) ||
                            (u.UserName != null && u.UserName.ToLower().Contains(term)) ||
                            (u.UserCode != null && u.UserCode.ToLower().Contains(term)))
                .Select(u => u.UserID).ToList();

            // Tìm sách phù hợp
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

            var result = tickets.Select(t => {
                var detail = details.FirstOrDefault(d => d.TicketID == t.TicketID);
                var book = detail != null && books.ContainsKey(detail.BookID) ? books[detail.BookID] : null;
                var user = users.ContainsKey(t.UserID) ? users[t.UserID] : null;

                return new {
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
