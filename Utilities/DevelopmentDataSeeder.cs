using LibraryManagement.Models;

namespace LibraryManagement.Utilities
{
    /// <summary>
    /// Bộ nạp dữ liệu mẫu (Demo / Seed Data) TÁCH BIỆT HOÀN TOÀN khỏi luồng nghiệp vụ.
    /// CHỈ ĐƯỢC PHÉP CHẠY trong môi trường Development hoặc Demo khi CSDL hoàn toàn trống.
    /// Khi chạy Production hoặc khi CSDL đã có dữ liệu, lớp này hoàn toàn không can thiệp.
    /// </summary>
    public static class DevelopmentDataSeeder
    {
        public static void SeedIfEmpty(DataContext context)
        {
            try
            {
                // 1. Khởi tạo Thể loại sách nếu CSDL chưa có
                if (!context.Categories.Any())
                {
                    context.Categories.AddRange(
                        new tblCategory { CategoryName = "Văn học & Nghệ thuật", Description = "Tiểu thuyết, truyện ngắn, thơ ca, văn học Việt Nam và thế giới", IsActive = true },
                        new tblCategory { CategoryName = "Công nghệ thông tin & Lập trình", Description = "Khoa học máy tính, kỹ thuật phần mềm, an toàn mạng và cơ sở dữ liệu", IsActive = true },
                        new tblCategory { CategoryName = "Kinh tế & Quản trị kinh doanh", Description = "Quản trị doanh nghiệp, tài chính - ngân hàng, kế toán và khởi nghiệp", IsActive = true },
                        new tblCategory { CategoryName = "Tâm lý & Kỹ năng sống", Description = "Tâm lý học ứng dụng, kỹ năng giao tiếp, tư duy lãnh đạo và phát triển bản thân", IsActive = true },
                        new tblCategory { CategoryName = "Giáo trình & Học liệu học phần", Description = "Giáo trình chính quy, bài giảng chuyên ngành và tài liệu học tập các khối ngành", IsActive = true },
                        new tblCategory { CategoryName = "Khoa học tự nhiên & Kỹ thuật", Description = "Toán học, Vật lý, Hóa học, Điện - Điện tử và cơ khí ứng dụng", IsActive = true },
                        new tblCategory { CategoryName = "Khoa học xã hội & Nhân văn", Description = "Lịch sử, địa lý, triết học, văn hóa, pháp luật và ngôn ngữ học", IsActive = true },
                        new tblCategory { CategoryName = "Ngoại ngữ & Tài liệu quốc tế", Description = "Tiếng Anh học thuật, IELTS/TOEIC, giáo trình ngoại ngữ và văn hóa thế giới", IsActive = true }
                    );
                    context.SaveChanges();
                }

                // 2. Khởi tạo Tác giả nếu CSDL chưa có
                if (!context.Authors.Any())
                {
                    context.Authors.AddRange(
                        new tblAuthor { AuthorName = "Nguyễn Nhật Ánh", Bio = "Nhà văn Việt Nam nổi tiếng với nhiều tác phẩm tuổi học trò.", IsActive = true },
                        new tblAuthor { AuthorName = "Robert C. Martin", Bio = "Chuyên gia công nghệ phần mềm, tác giả cuốn Clean Code nổi tiếng.", IsActive = true },
                        new tblAuthor { AuthorName = "Dale Carnegie", Bio = "Tác giả cuốn Đắc Nhân Tâm kinh điển về nghệ thuật giao tiếp.", IsActive = true },
                        new tblAuthor { AuthorName = "J.K. Rowling", Bio = "Nữ văn sĩ nước Anh, tác giả của bộ truyện Harry Potter huyền thoại.", IsActive = true },
                        new tblAuthor { AuthorName = "Stuart Russell", Bio = "Giáo sư khoa học máy tính tại UC Berkeley, chuyên gia AI hàng đầu.", IsActive = true },
                        new tblAuthor { AuthorName = "Eric Ries", Bio = "Doanh nhân và tác giả cuốn Khởi nghiệp tinh gọn (The Lean Startup).", IsActive = true }
                    );
                    context.SaveChanges();
                }

                // 3. Khởi tạo Nhà xuất bản nếu CSDL chưa có
                if (!context.Publishers.Any())
                {
                    context.Publishers.AddRange(
                        new tblPublisher { PublisherName = "NXB Trẻ", Address = "161B Lý Chính Thắng, P. Võ Thị Sáu, Q.3, TP.HCM", Phone = "028 3931 6289", IsActive = true },
                        new tblPublisher { PublisherName = "NXB Thông Tin & Truyền Thông", Address = "Tòa nhà Cục Tần số, 115 Trần Duy Hưng, Hà Nội", Phone = "024 3577 2139", IsActive = true },
                        new tblPublisher { PublisherName = "NXB Giáo Dục Việt Nam", Address = "81 Trần Hưng Đạo, Hoàn Kiếm, Hà Nội", Phone = "024 3822 0801", IsActive = true }
                    );
                    context.SaveChanges();
                }

                // Lấy ID thực tế đã tạo
                var categories = context.Categories.ToList();
                var authors = context.Authors.ToList();
                var publishers = context.Publishers.ToList();

                int catIT = categories.FirstOrDefault(c => c.CategoryName.Contains("Lập trình"))?.CategoryID ?? categories.First().CategoryID;
                int catAI = categories.FirstOrDefault(c => c.CategoryName.Contains("Trí tuệ nhân tạo"))?.CategoryID ?? categories.First().CategoryID;
                int catEcon = categories.FirstOrDefault(c => c.CategoryName.Contains("Kinh tế"))?.CategoryID ?? categories.First().CategoryID;
                int catLit = categories.FirstOrDefault(c => c.CategoryName.Contains("Văn học"))?.CategoryID ?? categories.First().CategoryID;
                int catSkill = categories.FirstOrDefault(c => c.CategoryName.Contains("Kỹ năng"))?.CategoryID ?? categories.First().CategoryID;

                int authRowling = authors.FirstOrDefault(a => a.AuthorName.Contains("Rowling"))?.AuthorID ?? authors.First().AuthorID;
                int authUncleBob = authors.FirstOrDefault(a => a.AuthorName.Contains("Robert"))?.AuthorID ?? authors.First().AuthorID;
                int authAnh = authors.FirstOrDefault(a => a.AuthorName.Contains("Nguyễn Nhật Ánh"))?.AuthorID ?? authors.First().AuthorID;
                int authRussell = authors.FirstOrDefault(a => a.AuthorName.Contains("Russell"))?.AuthorID ?? authors.First().AuthorID;
                int authCarnegie = authors.FirstOrDefault(a => a.AuthorName.Contains("Carnegie"))?.AuthorID ?? authors.First().AuthorID;
                int authRies = authors.FirstOrDefault(a => a.AuthorName.Contains("Eric"))?.AuthorID ?? authors.First().AuthorID;

                int pubTre = publishers.FirstOrDefault(p => p.PublisherName.Contains("Trẻ"))?.PublisherID ?? publishers.First().PublisherID;
                int pubIT = publishers.FirstOrDefault(p => p.PublisherName.Contains("Truyền Thông"))?.PublisherID ?? publishers.First().PublisherID;
                int pubGD = publishers.FirstOrDefault(p => p.PublisherName.Contains("Giáo Dục"))?.PublisherID ?? publishers.First().PublisherID;

                // 4. Khởi tạo Sách mẫu nếu CSDL chưa có cuốn nào
                if (!context.Books.Any())
                {
                    context.Books.AddRange(
                        new tblBook
                        {
                            BookCode = "MS0001",
                            Title = "Harry Potter và Hòn Đá Phù Thủy (Tập 1)",
                            CategoryID = catLit,
                            AuthorID = authRowling,
                            PublisherID = pubTre,
                            PublishYear = 2024,
                            Quantity = 12,
                            AvailableQuantity = 12,
                            Images = "/images/books/harry-potter.svg",
                            Summary = "Tác phẩm kỳ ảo kinh điển về cậu bé phù thủy Harry Potter bước chân vào trường Hogwarts.",
                            Contents = "<p>Cuốn sách mở đầu cho series văn học kỳ ảo bán chạy nhất mọi thời đại của nữ văn sĩ J.K. Rowling. Tác phẩm kể về cuộc phiêu lưu của cậu bé mồ côi Harry Potter khi bước sang tuổi 11.</p><p><strong>Vị trí kho:</strong> Kệ C4 - Tầng 2 (Ngăn 2)</p>",
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        },
                        new tblBook
                        {
                            BookCode = "MS0002",
                            Title = "Clean Code - Mã Sạch Thực Chiến",
                            CategoryID = catIT,
                            AuthorID = authUncleBob,
                            PublisherID = pubIT,
                            PublishYear = 2023,
                            Quantity = 15,
                            AvailableQuantity = 15,
                            Images = "/images/books/clean-code.svg",
                            Summary = "Cẩm nang phát triển phần mềm chuyên nghiệp, quy tắc đặt tên, hàm, xử lý lỗi và cấu trúc mã nguồn tối ưu.",
                            Contents = "<p>Clean Code của Robert C. Martin (Uncle Bob) là cuốn sách gối đầu giường của mọi lập trình viên. Cuốn sách hướng dẫn cách viết mã sạch, dễ bảo trì và kiểm thử tự động.</p><p><strong>Vị trí kho:</strong> Kệ A1 - Tầng 1 (Ngăn 1)</p>",
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        },
                        new tblBook
                        {
                            BookCode = "MS0003",
                            Title = "Mắt Biếc (Tiểu Thuyết Tuổi Hoa)",
                            CategoryID = catLit,
                            AuthorID = authAnh,
                            PublisherID = pubTre,
                            PublishYear = 2022,
                            Quantity = 10,
                            AvailableQuantity = 10,
                            Images = "/images/books/mat-biec.svg",
                            Summary = "Câu chuyện tình thơ mộng, đượm buồn về mối tình đơn phương của Ngạn dành cho Hà Lan nơi làng Đo Đo.",
                            Contents = "<p>Mắt Biếc là một trong những tác phẩm được yêu thích nhất của nhà văn Nguyễn Nhật Ánh, khắc họa tuổi thơ êm đềm và nỗi niềm sâu lắng của tình yêu tuổi học trò.</p><p><strong>Vị trí kho:</strong> Kệ B2 - Tầng 1 (Ngăn 3)</p>",
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        },
                        new tblBook
                        {
                            BookCode = "MS0004",
                            Title = "Trí Tuệ Nhân Tạo Trong Kỷ Nguyên Số",
                            CategoryID = catAI,
                            AuthorID = authRussell,
                            PublisherID = pubGD,
                            PublishYear = 2024,
                            Quantity = 14,
                            AvailableQuantity = 14,
                            Images = "/images/books/ai-book.svg",
                            Summary = "Giới thiệu toàn diện về Machine Learning, Deep Learning, Mạng nơ-ron và tác động của AI trong xã hội thông minh.",
                            Contents = "<p>Tài liệu chuyên sâu về cơ sở lý thuyết và ứng dụng thực tiễn của trí tuệ nhân tạo hiện đại, từ giải thuật tìm kiếm đến mô hình ngôn ngữ lớn (LLM).</p><p><strong>Vị trí kho:</strong> Kệ D1 - Tầng 2 (Ngăn 4)</p>",
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        },
                        new tblBook
                        {
                            BookCode = "MS0005",
                            Title = "Đắc Nhân Tâm - How to Win Friends & Influence People",
                            CategoryID = catSkill,
                            AuthorID = authCarnegie,
                            PublisherID = pubTre,
                            PublishYear = 2023,
                            Quantity = 20,
                            AvailableQuantity = 20,
                            Images = "/images/books/dac-nhan-tam.svg",
                            Summary = "Nghệ thuật giao tiếp, thấu hiểu tâm lý và cách xây dựng mối quan hệ xã hội bền vững, thành công.",
                            Contents = "<p>Tác phẩm nổi tiếng nhất thế giới về nghệ thuật kết nối con người, giúp người đọc thấu hiểu tâm lý và tạo lập tầm ảnh hưởng tích cực.</p><p><strong>Vị trí kho:</strong> Kệ E1 - Tầng 1 (Ngăn 2)</p>",
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        },
                        new tblBook
                        {
                            BookCode = "MS0006",
                            Title = "Khởi Nghiệp Tinh Gọn (The Lean Startup)",
                            CategoryID = catEcon,
                            AuthorID = authRies,
                            PublisherID = pubTre,
                            PublishYear = 2023,
                            Quantity = 10,
                            AvailableQuantity = 10,
                            Images = "/images/books/lean-startup.svg",
                            Summary = "Phương pháp luận xây dựng sản phẩm linh hoạt, vòng lặp Xây dựng - Đo lường - Học hỏi giúp startup thành công.",
                            Contents = "<p>Tác phẩm đặt nền móng cho phong trào đổi mới sáng tạo toàn cầu, cách thử nghiệm giả thuyết kinh doanh và giảm thiểu rủi ro lãng phí nguồn lực.</p><p><strong>Vị trí kho:</strong> Kệ B1 - Tầng 2 (Ngăn 3)</p>",
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        }
                    );
                    context.SaveChanges();
                }

                // 5. Khởi tạo Tài khoản mẫu (Admin & Độc giả demo) nếu chưa có
                if (!context.Users.Any(u => u.Role == "Admin"))
                {
                    var adminUser = new tblUser
                    {
                        UserCode = "AD0001",
                        UserName = "admin",
                        FullName = "Nguyễn Quản Trị (Admin)",
                        Email = "admin@thuvien.edu.vn",
                        Role = "Admin",
                        IsActive = true
                    };
                    adminUser.Password = PasswordSecurity.HashPassword(adminUser, "123456");
                    context.Users.Add(adminUser);
                    context.SaveChanges();
                }

                if (!context.Users.Any(u => u.Role == "Reader"))
                {
                    var readerUser = new tblUser
                    {
                        UserCode = "DG0001",
                        UserName = "sinhvien01",
                        FullName = "Lê Hoàng Nam (Sinh Viên)",
                        Email = "nam.lh@sinhvien.edu.vn",
                        Role = "Reader",
                        IsActive = true
                    };
                    readerUser.Password = PasswordSecurity.HashPassword(readerUser, "123456");
                    context.Users.Add(readerUser);
                    context.SaveChanges();
                }
            }
            catch
            {
                // Bỏ qua nếu có lỗi trong quá trình khởi tạo môi trường dev
            }
        }
    }
}
