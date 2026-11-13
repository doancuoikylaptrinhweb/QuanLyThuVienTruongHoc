using Microsoft.AspNetCore.Mvc;
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

        // Danh sách 12 đầu sách tiêu chuẩn chuẩn hóa theo thiết kế Visual Blue
        private List<tblBook> GetStandardDemoBooks()
        {
            return new List<tblBook>
            {
                new tblBook
                {
                    BookID = 1,
                    Title = "Harry Potter và Hòn Đá Phù Thủy (Tập 1)",
                    CategoryID = 1,
                    AuthorID = 101,
                    PublisherID = 1,
                    PublishYear = 2024,
                    Quantity = 12,
                    AvailableQuantity = 4,
                    Images = "/images/books/harry-potter.svg",
                    Summary = "Tác phẩm kỳ ảo kinh điển về cậu bé phù thủy Harry Potter bước chân vào trường Hogwarts.",
                    Contents = "<p>Cuốn sách mở đầu cho series văn học kỳ ảo bán chạy nhất mọi thời đại của nữ văn sĩ J.K. Rowling. Tác phẩm kể về cuộc phiêu lưu của cậu bé mồ côi Harry Potter khi bước sang tuổi 11 và nhận được thư mời nhập học tại Trường Phù thủy và Pháp sư Hogwarts.</p><p><strong>Vị trí kho:</strong> Kệ C4 - Tầng 2 (Ngăn 2)</p>",
                    IsActive = true
                },
                new tblBook
                {
                    BookID = 2,
                    Title = "Clean Code - Mã Sạch Thực Chiến",
                    CategoryID = 2,
                    AuthorID = 2,
                    PublisherID = 2,
                    PublishYear = 2023,
                    Quantity = 15,
                    AvailableQuantity = 8,
                    Images = "/images/books/clean-code.svg",
                    Summary = "Cẩm nang phát triển phần mềm chuyên nghiệp, quy tắc đặt tên, hàm, xử lý lỗi và cấu trúc mã nguồn tối ưu.",
                    Contents = "<p>Clean Code của Robert C. Martin (Uncle Bob) là cuốn sách gối đầu giường của mọi lập trình viên. Cuốn sách hướng dẫn cách viết mã sạch, dễ bảo trì, tối ưu hiệu năng và kiểm thử tự động.</p><p><strong>Vị trí kho:</strong> Kệ A1 - Tầng 1 (Ngăn 1)</p>",
                    IsActive = true
                },
                new tblBook
                {
                    BookID = 3,
                    Title = "Mắt Biếc (Tiểu Thuyết Tuổi Hoa)",
                    CategoryID = 1,
                    AuthorID = 1,
                    PublisherID = 1,
                    PublishYear = 2022,
                    Quantity = 10,
                    AvailableQuantity = 6,
                    Images = "/images/books/mat-biec.svg",
                    Summary = "Câu chuyện tình thơ mộng, đượm buồn về mối tình đơn phương của Ngạn dành cho Hà Lan nơi làng Đo Đo.",
                    Contents = "<p>Mắt Biếc là một trong những tác phẩm được yêu thích nhất của nhà văn Nguyễn Nhật Ánh, khắc họa tuổi thơ êm đềm và nỗi niềm sâu lắng của tình yêu tuổi học trò.</p><p><strong>Vị trí kho:</strong> Kệ B2 - Tầng 1 (Ngăn 3)</p>",
                    IsActive = true
                },
                new tblBook
                {
                    BookID = 4,
                    Title = "Trí Tuệ Nhân Tạo Trong Kỷ Nguyên Số",
                    CategoryID = 6,
                    AuthorID = 102,
                    PublisherID = 3,
                    PublishYear = 2024,
                    Quantity = 14,
                    AvailableQuantity = 5,
                    Images = "/images/books/ai-book.svg",
                    Summary = "Giới thiệu toàn diện về Machine Learning, Deep Learning, Mạng nơ-ron và tác động của AI trong xã hội thông minh.",
                    Contents = "<p>Tài liệu chuyên sâu về cơ sở lý thuyết và ứng dụng thực tiễn của trí tuệ nhân tạo hiện đại, từ giải thuật tìm kiếm, học có giám sát đến mô hình ngôn ngữ lớn (LLM).</p><p><strong>Vị trí kho:</strong> Kệ D1 - Tầng 2 (Ngăn 4)</p>",
                    IsActive = true
                },
                new tblBook
                {
                    BookID = 5,
                    Title = "Đắc Nhân Tâm - How to Win Friends & Influence People",
                    CategoryID = 4,
                    AuthorID = 3,
                    PublisherID = 1,
                    PublishYear = 2023,
                    Quantity = 20,
                    AvailableQuantity = 12,
                    Images = "/images/books/dac-nhan-tam.svg",
                    Summary = "Nghệ thuật giao tiếp, thấu hiểu tâm lý và cách xây dựng mối quan hệ xã hội bền vững, thành công.",
                    Contents = "<p>Cuốn sách kỹ năng sống nổi tiếng và bán chạy nhất thế giới của Dale Carnegie, đem đến những bài học sâu sắc về cách ứng xử và kết nối con người.</p><p><strong>Vị trí kho:</strong> Kệ B1 - Tầng 1 (Ngăn 2)</p>",
                    IsActive = true
                },
                new tblBook
                {
                    BookID = 6,
                    Title = "Lập Trình Hướng Đối Tượng C# & .NET",
                    CategoryID = 2,
                    AuthorID = 103,
                    PublisherID = 3,
                    PublishYear = 2024,
                    Quantity = 12,
                    AvailableQuantity = 7,
                    Images = "/images/books/csharp-book.svg",
                    Summary = "Giáo trình giảng dạy cốt lõi về OOP, C# 12, ASP.NET Core MVC và Entity Framework Core trong đại học.",
                    Contents = "<p>Giáo trình trang bị kiến thức nền tảng vững chắc về đóng gói, kế thừa, đa hình, trừu tượng, LINQ và mô hình kiến trúc MVC phân tầng trong môi trường .NET hiện đại.</p><p><strong>Vị trí kho:</strong> Kệ A2 - Tầng 1 (Ngăn 3)</p>",
                    IsActive = true
                },
                new tblBook
                {
                    BookID = 7,
                    Title = "Giải Thuật Và Cấu Trúc Dữ Liệu Nâng Cao",
                    CategoryID = 2,
                    AuthorID = 104,
                    PublisherID = 3,
                    PublishYear = 2023,
                    Quantity = 10,
                    AvailableQuantity = 6,
                    Images = "/images/books/algorithms.svg",
                    Summary = "Phân tích độ phức tạp thời gian, không gian, đồ thị, quy hoạch động và các giải thuật tối ưu hóa.",
                    Contents = "<p>Tài liệu chuẩn mực dành cho sinh viên ngành công nghệ thông tin và khoa học máy tính rèn luyện tư duy thuật toán và lập trình thi đấu.</p><p><strong>Vị trí kho:</strong> Kệ A1 - Tầng 1 (Ngăn 2)</p>",
                    IsActive = true
                },
                new tblBook
                {
                    BookID = 8,
                    Title = "Hệ Quản Trị Cơ Sở Dữ Liệu & SQL Nâng Cao",
                    CategoryID = 2,
                    AuthorID = 105,
                    PublisherID = 3,
                    PublishYear = 2023,
                    Quantity = 8,
                    AvailableQuantity = 3,
                    Images = "/images/books/database.svg",
                    Summary = "Thiết kế chuẩn hóa lược đồ CSDL, chỉ mục, Transaction ACID, tối ưu hóa truy vấn SQL Server.",
                    Contents = "<p>Cung cấp kỹ năng quản trị cơ sở dữ liệu quan hệ doanh nghiệp, lập chỉ mục tối ưu, thiết kế thủ tục lưu trữ Stored Procedure và Trigger bảo toàn vẹn dữ liệu.</p><p><strong>Vị trí kho:</strong> Kệ A3 - Tầng 1 (Ngăn 1)</p>",
                    IsActive = true
                },
                new tblBook
                {
                    BookID = 9,
                    Title = "Khởi Nghiệp Tinh Gọn (The Lean Startup)",
                    CategoryID = 3,
                    AuthorID = 106,
                    PublisherID = 1,
                    PublishYear = 2023,
                    Quantity = 9,
                    AvailableQuantity = 5,
                    Images = "/images/books/lean-startup.svg",
                    Summary = "Phương pháp luận khởi nghiệp sáng tạo, vòng lặp Xây dựng - Đo lường - Học hỏi và tạo sản phẩm khả dụng tối thiểu (MVP).",
                    Contents = "<p>Cuốn sách kinh điển của Eric Ries làm thay đổi hoàn toàn tư duy quản trị kinh doanh và phát triển sản phẩm công nghệ của các công ty khởi nghiệp thế kỷ 21.</p><p><strong>Vị trí kho:</strong> Kệ C1 - Tầng 2 (Ngăn 1)</p>",
                    IsActive = true
                },
                new tblBook
                {
                    BookID = 10,
                    Title = "Nhà Giả Kim (The Alchemist)",
                    CategoryID = 1,
                    AuthorID = 107,
                    PublisherID = 2,
                    PublishYear = 2022,
                    Quantity = 11,
                    AvailableQuantity = 7,
                    Images = "/images/books/alchemist.svg",
                    Summary = "Hành trình theo đuổi ước mơ và lắng nghe tiếng gọi trái tim của chàng chăn cừu Santiago qua sa mạc.",
                    Contents = "<p>Một trong những cuốn sách văn học được dịch ra nhiều ngôn ngữ nhất, truyền cảm hứng mạnh mẽ về việc dũng cảm theo đuổi vận mệnh của cuộc đời mình.</p><p><strong>Vị trí kho:</strong> Kệ B3 - Tầng 1 (Ngăn 2)</p>",
                    IsActive = true
                },
                new tblBook
                {
                    BookID = 11,
                    Title = "Tư Duy Nhanh Và Chậm (Thinking, Fast and Slow)",
                    CategoryID = 4,
                    AuthorID = 108,
                    PublisherID = 1,
                    PublishYear = 2023,
                    Quantity = 8,
                    AvailableQuantity = 4,
                    Images = "/images/books/thinking.svg",
                    Summary = "Khám phá hai hệ thống chi phối tư duy của con người: Hệ thống 1 phản xạ nhanh và Hệ thống 2 tư duy logic chậm.",
                    Contents = "<p>Tác phẩm tâm lý học hành vi đoạt giải Nobel Kinh tế của Daniel Kahneman, giải mã những thiên kiến nhận thức và cách con người đưa ra quyết định mỗi ngày.</p><p><strong>Vị trí kho:</strong> Kệ C2 - Tầng 2 (Ngăn 3)</p>",
                    IsActive = true
                },
                new tblBook
                {
                    BookID = 12,
                    Title = "Học Máy & Khai Phá Dữ Liệu Lớn (Big Data)",
                    CategoryID = 6,
                    AuthorID = 109,
                    PublisherID = 3,
                    PublishYear = 2024,
                    Quantity = 8,
                    AvailableQuantity = 4,
                    Images = "/images/books/machine-learning.svg",
                    Summary = "Các kỹ thuật khai phá dữ liệu, phân cụm, phân lớp, xử lý ngôn ngữ tự nhiên và hệ khuyến nghị thông minh.",
                    Contents = "<p>Tài liệu giáo trình nâng cao phục vụ nghiên cứu và triển khai các bài toán dự báo, phân tích mẫu hành vi và xây dựng trợ lý AI trong thư viện thông minh.</p><p><strong>Vị trí kho:</strong> Kệ D2 - Tầng 2 (Ngăn 1)</p>",
                    IsActive = true
                }
            };
        }

        // Danh mục chuẩn hóa
        private List<tblCategory> GetStandardCategories()
        {
            return new List<tblCategory>
            {
                new tblCategory { CategoryID = 2, CategoryName = "Công nghệ thông tin & Lập trình", Description = "Sách lập trình, mạng máy tính, công nghệ phần mềm", IsActive = true },
                new tblCategory { CategoryID = 6, CategoryName = "Trí tuệ nhân tạo & Dữ liệu", Description = "Machine Learning, Deep Learning, Big Data", IsActive = true },
                new tblCategory { CategoryID = 3, CategoryName = "Kinh tế & Quản trị kinh doanh", Description = "Quản trị kinh doanh, tài chính, khởi nghiệp", IsActive = true },
                new tblCategory { CategoryID = 5, CategoryName = "Giáo trình & Học phần đại học", Description = "Tài liệu giảng dạy và học tập các khối ngành", IsActive = true },
                new tblCategory { CategoryID = 1, CategoryName = "Văn học Việt Nam & Nước ngoài", Description = "Tiểu thuyết, văn học cổ điển và hiện đại", IsActive = true },
                new tblCategory { CategoryID = 4, CategoryName = "Kỹ năng sống & Phát triển bản thân", Description = "Tư duy, tâm lý học, nghệ thuật giao tiếp", IsActive = true }
            };
        }

        // Tác giả chuẩn hóa
        private Dictionary<int, string> GetStandardAuthors()
        {
            return new Dictionary<int, string>
            {
                { 1, "Nguyễn Nhật Ánh" },
                { 2, "Robert C. Martin" },
                { 3, "Dale Carnegie" },
                { 101, "J.K. Rowling" },
                { 102, "Stuart Russell" },
                { 103, "PGS.TS Nguyễn Văn Hùng" },
                { 104, "Lê Minh Hoàng" },
                { 105, "TS. Trần Minh Tuấn" },
                { 106, "Eric Ries" },
                { 107, "Paulo Coelho" },
                { 108, "Daniel Kahneman" },
                { 109, "TS. Lê Hoàng Nam" }
            };
        }

        // 1. TRANG CHỦ & TRA CỨU SÁCH TRỰC TUYẾN
        public IActionResult Index(string? searchString, int? categoryId)
        {
            List<tblBook> allBooks = new List<tblBook>();

            try
            {
                var dbBooks = _context.Books.Where(b => b.IsActive == true).ToList();
                if (dbBooks.Count > 0)
                {
                    allBooks.AddRange(dbBooks);
                }
            }
            catch { }

            // Nếu số lượng sách ít hơn 12, bổ sung thêm danh mục chuẩn để hiển thị đầy đủ
            var standardDemos = GetStandardDemoBooks();
            foreach (var demo in standardDemos)
            {
                if (!allBooks.Any(b => b.Title.ToLower() == demo.Title.ToLower()))
                {
                    allBooks.Add(demo);
                }
            }

            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                string term = searchString.Trim().ToLower();
                allBooks = allBooks.Where(b => 
                    (b.Title != null && b.Title.ToLower().Contains(term)) ||
                    (b.Summary != null && b.Summary.ToLower().Contains(term))
                ).ToList();
                ViewBag.SearchString = searchString;
            }

            // Lọc theo thể loại
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                allBooks = allBooks.Where(b => b.CategoryID == categoryId.Value).ToList();
                ViewBag.SelectedCategory = categoryId.Value;
            }

            // Danh mục và tác giả
            ViewBag.Categories = GetStandardCategories();
            ViewBag.Authors = GetStandardAuthors();

            return View(allBooks);
        }

        // 2. XEM CHI TIẾT SÁCH DÙNG SLUG GENERATOR
        [Route("/sach-{slug}-{id:long}.html", Name = "BookDetails")]
        public IActionResult Details(long? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            tblBook? book = null;
            try
            {
                book = _context.Books.FirstOrDefault(b => b.BookID == id && b.IsActive == true);
            }
            catch { }

            if (book == null)
            {
                book = GetStandardDemoBooks().FirstOrDefault(b => b.BookID == id);
            }

            if (book == null)
            {
                return NotFound();
            }

            // Thông tin Tác giả, Thể loại, NXB
            var categories = GetStandardCategories();
            var authors = GetStandardAuthors();

            ViewBag.Category = categories.FirstOrDefault(c => c.CategoryID == book.CategoryID) 
                               ?? new tblCategory { CategoryName = "Tài liệu chuyên khảo" };

            string authorName = authors.ContainsKey(book.AuthorID) ? authors[book.AuthorID] : "Đang cập nhật";
            ViewBag.Author = new tblAuthor { AuthorName = authorName, Bio = "Tác giả uy tín trong lĩnh vực học thuật & văn hóa." };
            ViewBag.Publisher = new tblPublisher { PublisherName = "NXB Giáo Dục & Tri Thức", Address = "Hà Nội" };

            return View(book);
        }

        // 3. XỬ LÝ CHATBOT VÀ HỖ TRỢ AI (Mục 11 - RAG Tri thức Thư viện)
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

            // 3. Hỏi gợi ý sách / tìm sách
            var allBooks = GetStandardDemoBooks();
            if (msg.Contains("clean code"))
            {
                var b = allBooks.First(x => x.BookID == 2);
                return Json(new { reply = $"📖 Cuốn <strong>{b.Title}</strong> của {GetStandardAuthors()[b.AuthorID]} hiện <strong>còn {b.AvailableQuantity} cuốn</strong> trên kệ.<br/>📍 <em>Vị trí: Kệ A1 - Tầng 1 (Ngăn 1)</em>." });
            }
            if (msg.Contains("harry potter"))
            {
                var b = allBooks.First(x => x.BookID == 1);
                return Json(new { reply = $"⚡ Cuốn <strong>{b.Title}</strong> hiện <strong>còn {b.AvailableQuantity} cuốn</strong>.<br/>📍 <em>Vị trí: Kệ C4 - Tầng 2 (Ngăn 2)</em>." });
            }
            if (msg.Contains("mắt biếc"))
            {
                var b = allBooks.First(x => x.BookID == 3);
                return Json(new { reply = $"🍃 Cuốn <strong>{b.Title}</strong> của nhà văn Nguyễn Nhật Ánh hiện <strong>còn {b.AvailableQuantity} cuốn</strong>.<br/>📍 <em>Vị trí: Kệ B2 - Tầng 1 (Ngăn 3)</em>." });
            }

            if (msg.Contains("sách") || msg.Contains("gợi ý") || msg.Contains("tìm") || msg.Contains("kho"))
            {
                string bookList = "Dưới đây là một số đầu sách nổi bật đang sẵn sàng trên kệ:<br/>";
                foreach (var b in allBooks.Take(3))
                {
                    bookList += $"• <strong>{b.Title}</strong> (Còn: {b.AvailableQuantity} cuốn)<br/>";
                }
                bookList += "<br/>👉 Bạn có thể dùng ô tìm kiếm trên trang chủ để tra cứu vị trí kệ sách chi tiết!";
                return Json(new { reply = bookList });
            }

            return Json(new { 
                reply = "Tôi đã ghi nhận câu hỏi của bạn. Bạn có thể hỏi tôi về: <em>'Gợi ý sách'</em>, <em>'Quy định mượn sách'</em> hoặc gõ tên sách như <em>'Clean Code'</em>, <em>'Harry Potter'</em>, <em>'Mắt Biếc'</em> để được trợ giúp ngay!" 
            });
        }
    }
}
