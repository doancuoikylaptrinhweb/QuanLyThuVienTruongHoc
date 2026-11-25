-- =================================================================
-- DỮ LIỆU CƠ SỞ DỮ LIỆU MỚI NHẤT: LibraryDB
-- Thời gian xuất tự động: 25/11/2026 20:31:22
-- Dự án: QuanLyThuVienTruongHoc (ASP.NET Core MVC)
-- =================================================================
USE [LibraryDB];
GO

-- -------------------------------------------------------------
-- Bảng: [tblCategory] (8 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [tblCategory] ON;
INSERT INTO [tblCategory] ([CategoryID], [CategoryName], [Description], [IsActive]) VALUES
(1, N'Văn học & Nghệ thuật', N'Tiểu thuyết, truyện ngắn, thơ ca, văn học Việt Nam và thế giới', 1),
(2, N'Công nghệ thông tin & Lập trình', N'Khoa học máy tính, kỹ thuật phần mềm, an toàn mạng và cơ sở dữ liệu', 1),
(3, N'Kinh tế & Quản trị kinh doanh', N'Quản trị doanh nghiệp, tài chính - ngân hàng, kế toán và khởi nghiệp', 1),
(4, N'Tâm lý & Kỹ năng sống', N'Tâm lý học ứng dụng, kỹ năng giao tiếp, tư duy lãnh đạo và phát triển bản thân', 1),
(5, N'Giáo trình & Học liệu học phần', N'Giáo trình chính quy, bài giảng chuyên ngành và tài liệu học tập các khối ngành', 1),
(6, N'Khoa học tự nhiên & Kỹ thuật', N'Toán học, Vật lý, Hóa học, Điện - Điện tử và cơ khí ứng dụng', 1),
(7, N'Khoa học xã hội & Nhân văn', N'Lịch sử, địa lý, triết học, văn hóa, pháp luật và ngôn ngữ học', 1),
(8, N'Ngoại ngữ & Tài liệu quốc tế', N'Tiếng Anh học thuật, IELTS/TOEIC, giáo trình ngoại ngữ và văn hóa thế giới', 1);
SET IDENTITY_INSERT [tblCategory] OFF;
GO

-- -------------------------------------------------------------
-- Bảng: [tblAuthor] (3 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [tblAuthor] ON;
INSERT INTO [tblAuthor] ([AuthorID], [AuthorName], [Bio], [IsActive]) VALUES
(1, N'Nguyễn Nhật Ánh', N'Nhà văn nổi tiếng của Việt Nam với các tác phẩm tuổi học trò', 1),
(2, N'Robert C. Martin', N'Chuyên gia công nghệ phần mềm, tác giả cuốn Clean Code', 1),
(3, N'Dale Carnegie', N'Tác giả cuốn Đắc Nhân Tâm', 1);
SET IDENTITY_INSERT [tblAuthor] OFF;
GO

-- -------------------------------------------------------------
-- Bảng: [tblPublisher] (4 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [tblPublisher] ON;
INSERT INTO [tblPublisher] ([PublisherID], [PublisherName], [Address], [Phone], [IsActive]) VALUES
(1, N'NXB Trẻ', N'161B Lý Chính Thắng, Quận 3, TP.HCM', N'02839316289', 1),
(2, N'NXB Kim Đồng', N'55 Quang Trung, Hai Bà Trưng, Hà Nội', N'02439434730', 1),
(3, N'NXB Giáo Dục', N'81 Trần Hưng Đạo, Hoàn Kiếm, Hà Nội', N'02438220801', 1),
(4, N'Nhà xuất bản giáo giục Việt Nam', N'Số 81 Trần Hưng Đạo, phường Cửa Nam, TP. Hà Nộ', N'024.38221386', 1);
SET IDENTITY_INSERT [tblPublisher] OFF;
GO

-- -------------------------------------------------------------
-- Bảng: [tblBook] (3 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [tblBook] ON;
INSERT INTO [tblBook] ([BookID], [Title], [CategoryID], [AuthorID], [PublisherID], [PublishYear], [Quantity], [AvailableQuantity], [Images], [Summary], [Contents], [PostOrder], [CreatedDate], [IsActive], [BookCode]) VALUES
(1, N'Tai', 2, 2, 2, 2024, 10, 9, N'files/Screenshot%202026-11-05%20184158.png', N'Test', NULL, 1, '2026-11-07 10:43:25', 1, NULL),
(2, N'Mắt biếc(1990)', 1, 1, 1, 2024, 10, 8, N'files/.tmb/M%E1%BA%AFt%20bi%E1%BA%BFc.jpg', N'Mắt biếc (xuất bản năm 1990) của nhà văn Nguyễn Nhật Ánh kể về mối tình đơn phương sâu sắc, kéo dài từ thuở ấu thơ đến lúc trưởng thành của Ngạn dành cho Hà Lan – cô bạn cùng làng Đo Đo có đôi mắt đẹp buồn.', N'<p>• Tuổi thơ ở làng Đo Đo: Ngạn và Hà Lan lớn lên bên nhau với nhiều kỷ niệm như đồi sim, tiếng trống trường và những rung động đầu đời. Ngạn âm thầm yêu Hà Lan nhưng không dám thổ lộ.</p><p>• Lên thành phố và biến cố: Lớn lên, cả hai ra thành phố học tập. Trong khi Ngạn vẫn giữ trọn tình cảm mộc mạc, hướng về quê nhà, Hà Lan bị cuốn hút bởi sự phồn hoa đô thị, yêu Dũng – một chàng trai hào hoa nhưng thiếu chung thủy – và mang thai.</p><p>• Trở về và Trà Long: Sau biến cố, Hà Lan sinh con gái tên Trà Long và đưa con về sống cùng ông bà ngoại ở làng Đo Đo. Ngạn luôn ở bên che chở, quan tâm mẹ con Hà Lan.</p><p>• Cái kết nhiều nuối tiếc: Trà Long lớn lên, mang nhiều nét giống Hà Lan và nảy sinh tình cảm với Ngạn. Nhận ra mình chỉ đang vấn vương bóng hình cũ của Hà Lan chứ không thể đáp lại tình yêu của Trà Long, Ngạn quyết định rời làng Đo Đo để khép lại mối tình day dứt.</p>', 1, '2026-11-23 19:35:06', 1, N'MS0002'),
(3, N'Đắc nhân tâm', 1, 3, 1, 1936, 20, 20, N'files/.tmb/%C4%91%E1%BA%AFc%20nh%C3%A2n%20t%C3%A2m.webp', N'Đắc Nhân Tâm (tên gốc tiếng Anh: How to Win Friends and Influence People) của tác giả Dale Carnegie, xuất bản lần đầu năm 1936, là cuốn sách về nghệ thuật giao tiếp, thấu hiểu tâm lý và xây dựng mối quan hệ bền vững. Sách gồm 4 phần với 30 nguyên tắc cốt lõi dựa trên sự chân thành, lắng nghe và tôn trọng người khác.', N'<div class="ZLzG0d ncoeY E8xBUb" role="heading" aria-level="3" data-animation-nesting="" data-sfc-cp="" jsaction="" data-wiz-attrbind="aria-level=pZe2Mc_1d/fLk2Md" jscontroller="zg1ZQe#TswZNc" data-sfc-root="c" jsuid="pZe2Mc_1d" data-hveid="CAIIBhAF" style="font-family: &quot;Google Sans&quot;, Arial, sans-serif, &quot;Noto Color Emoji&quot;; font-size: 20px; font-weight: 600; margin: 0px 0px 12px; border-bottom: 0px rgb(232, 234, 242);">Cấu trúc 4 phần chính<!--TgQPHdpZe2Mc_f/LL5gb|||[[2,[null,null,1,null,null,null,null,null,null,null,null,null,[351768,null,0]],[],null,1]]--></div><ul class="XSq4R KI3fJ" data-sfc-cp="" jsaction="" jscontroller="wiLEJc#NS0kde" data-sfc-root="c" jsuid="pZe2Mc_1j" data-hveid="CAIIBhAG" style="font-family: &quot;Google Sans&quot;, Arial, sans-serif, &quot;Noto Color Emoji&quot;; margin: 0px 0px 16px; border-bottom: 0px rgb(230, 232, 240);"><div data-bfc="" class="" data-sfc-cp="" jsaction="" jscontroller="eHDDzf#DtOdcb" data-sfc-root="c" jsuid="pZe2Mc_1k" data-ved="2ahUKEwikxJ_c7qCYAxUYSPUHHeGJAt4Qi4wTegYIAggGEAc" data-hveid="CAIIBhAH" style="margin: 0px; border-bottom: 0px rgb(230, 232, 240);"><li class="Z1qcYe" data-sfc-cp="" jsaction="" jscontroller="oSLmPe#HokZ0d" data-sfc-root="c" jsuid="pZe2Mc_1l" data-hveid="CAIIBhAI" style="margin: 0px 0px 12px; border-bottom: 0px rgb(230, 232, 240);"><span class="iNqyIf" data-sfc-cp="" jsaction="" jscontroller="KdQfFc#d5Qebf" data-sfc-root="c" jsuid="pZe2Mc_1m" style="margin: 0px; border-bottom: 0px rgb(230, 232, 240);"><span class="rQesXe MPyX" data-sfc-cp="" jsaction="" jscontroller="tP2kf#s32ZS" data-sfc-root="c" jsuid="pZe2Mc_1n" style="font-weight: 700; margin: 0px; border-bottom: 0px rgb(230, 232, 240);">Phần 1: Nghệ thuật ứng xử cơ bản (3 nguyên tắc)<!--TgQPHd|||[]--></span> — Tránh chỉ trích, oán trách hay than phiền; khen ngợi và ghi nhận chân thành; khơi gợi mong muốn ở người khác.<!--TgQPHd|||[]--></span><!--TgQPHd|||[]--></li><!--TgQPHd|||[]--></div><div data-bfc="" class="" data-sfc-cp="" jsaction="" jscontroller="eHDDzf#DtOdcb" data-sfc-root="c" jsuid="pZe2Mc_1p" data-ved="2ahUKEwikxJ_c7qCYAxUYSPUHHeGJAt4Qi4wTegYIAggGEAk" data-hveid="CAIIBhAJ" style="margin: 0px; border-bottom: 0px rgb(230, 232, 240);"><li class="Z1qcYe" data-sfc-cp="" jsaction="" jscontroller="oSLmPe#HokZ0d" data-sfc-root="c" jsuid="pZe2Mc_1r" data-hveid="CAIIBhAK" style="margin: 0px 0px 12px; border-bottom: 0px rgb(230, 232, 240);"><span class="iNqyIf" data-sfc-cp="" jsaction="" jscontroller="KdQfFc#d5Qebf" data-sfc-root="c" jsuid="pZe2Mc_1t" style="margin: 0px; border-bottom: 0px rgb(230, 232, 240);"><span class="rQesXe MPyX" data-sfc-cp="" jsaction="" jscontroller="tP2kf#s32ZS" data-sfc-root="c" jsuid="pZe2Mc_1u" style="font-weight: 700; margin: 0px; border-bottom: 0px rgb(230, 232, 240);">Phần 2: 6 cách tạo thiện cảm (Nguyên tắc 4–9)<!--TgQPHd|||[]--></span> — Quan tâm chân thành đến người khác, mỉm cười, nhớ tên người đối diện, biết lắng nghe và khuyến khích họ nói về điều họ quan tâm, giúp họ cảm thấy mình quan trọng.<!--TgQPHd|||[]--></span><!--TgQPHd|||[]--></li><!--TgQPHd|||[]--></div><div data-bfc="" class="" data-sfc-cp="" jsaction="" jscontroller="eHDDzf#DtOdcb" data-sfc-root="c" jsuid="pZe2Mc_1v" data-ved="2ahUKEwikxJ_c7qCYAxUYSPUHHeGJAt4Qi4wTegYIAggGEAs" data-hveid="CAIIBhAL" style="margin: 0px; border-bottom: 0px rgb(230, 232, 240);"><li class="Z1qcYe" data-sfc-cp="" jsaction="" jscontroller="oSLmPe#HokZ0d" data-sfc-root="c" jsuid="pZe2Mc_1w" data-hveid="CAIIBhAM" style="margin: 0px 0px 12px; border-bottom: 0px rgb(230, 232, 240);"><span class="iNqyIf" data-sfc-cp="" jsaction="" jscontroller="KdQfFc#d5Qebf" data-sfc-root="c" jsuid="pZe2Mc_1x" style="margin: 0px; border-bottom: 0px rgb(230, 232, 240);"><span class="rQesXe MPyX" data-sfc-cp="" jsaction="" jscontroller="tP2kf#s32ZS" data-sfc-root="c" jsuid="pZe2Mc_1y" style="font-weight: 700; margin: 0px; border-bottom: 0px rgb(230, 232, 240);">Phần 3: Hướng người khác theo suy nghĩ của bạn (Nguyên tắc 10–21)<!--TgQPHd|||[]--></span><span style="margin: 0px; border-bottom: 0px rgb(230, 232, 240);"> — Tránh tranh cãi gay gắt, tôn trọng ý kiến đối phương, nhận sai khi cần, bắt đầu bằng sự đồng thuận, để người khác tự rút ra kết luận và đặt mình vào vị trí của họ.</span><!--TgQPHd|||[]--></span><!--TgQPHd|||[]--></li><!--TgQPHd|||[]--></div><span style="margin: 0px; border-bottom: 0px rgb(230, 232, 240);"><span class="rQesXe MPyX" data-sfc-cp="" jsaction="" jscontroller="tP2kf#s32ZS" data-sfc-root="c" jsuid="pZe2Mc_c0" style="font-weight: 700; margin: 0px; border-bottom: 0px rgb(230, 232, 240);">Phần 4: Thay đổi người khác mà không gây chống đối (Nguyên tắc 22–30)<!--TgQPHd|||[]--></span> — Bắt đầu bằng lời khen và sự trân trọng, góp ý khéo léo, tự nhận khuyết điểm trước khi phê bình, đặt câu hỏi thay vì ra lệnh, giữ thể diện cho người khác và cổ vũ từng bước tiến nhỏ.<!--TgQPHd|||[]--></span><!--TgQPHd|||[]--></ul><div class="yhAwj" data-sfc-cp="" jsaction="rcuQ6b:&amp;pZe2Mc_c2|npT2md" jscontroller="UTzWVc#U8DOt" data-sfc-root="c" jsuid="pZe2Mc_c2" data-sfc-inited="2" style="font-family: &quot;Google Sans&quot;, Arial, sans-serif, &quot;Noto Color Emoji&quot;; margin: 0px; border-bottom: 0px rgb(230, 232, 240);"><!--TgQPHd|||[]--></div><div data-bfc="" class="" data-sfc-cp="" jsaction="" jscontroller="eHDDzf#DtOdcb" data-sfc-root="c" jsuid="pZe2Mc_c4" data-ved="2ahUKEwikxJ_c7qCYAxUYSPUHHeGJAt4Qi4wTegYIAggPEAA" data-hveid="CAIIDxAA" style="font-family: &quot;Google Sans&quot;, Arial, sans-serif, &quot;Noto Color Emoji&quot;; margin: 0px; border-bottom: 0px rgb(230, 232, 240);"><div class="ZLzG0d ncoeY E8xBUb" role="heading" aria-level="3" data-animation-nesting="" data-sfc-cp="" jsaction="" data-wiz-attrbind="aria-level=pZe2Mc_c6/fLk2Md" jscontroller="zg1ZQe#TswZNc" data-sfc-root="c" jsuid="pZe2Mc_c6" data-hveid="CAIIDxAB" style="font-size: 20px; font-weight: 600; margin: 0px 0px 12px; border-bottom: 0px rgb(232, 234, 242);">Thông điệp cốt lõi<!--TgQPHd|||[[2,[null,null,1,null,null,null,null,null,null,null,null,null,[351768,null,0]],[],null,1]]--></div><!--TgQPHd|||[]--></div><p><span style="font-family: &quot;Google Sans&quot;, Arial, sans-serif, &quot;Noto Color Emoji&quot;; margin: 0px; border-bottom: 0px rgb(230, 232, 240);"></span></p><ul class="XSq4R KI3fJ" data-sfc-cp="" jsaction="" jscontroller="wiLEJc#NS0kde" data-sfc-root="c" jsuid="pZe2Mc_ca" data-hveid="CAIIDxAC" style="font-family: &quot;Google Sans&quot;, Arial, sans-serif, &quot;Noto Color Emoji&quot;; font-size: 16px; font-weight: 400; margin: 0px 0px 16px; text-decoration: none; border-bottom: 0px rgb(230, 232, 240);"><div data-bfc="" class="" data-sfc-cp="" jsaction="" jscontroller="eHDDzf#DtOdcb" data-sfc-root="c" jsuid="pZe2Mc_cd" data-ved="2ahUKEwikxJ_c7qCYAxUYSPUHHeGJAt4Qi4wTegYIAggPEAM" data-hveid="CAIIDxAD" style="font-family: &quot;Google Sans&quot;, Arial, sans-serif, &quot;Noto Color Emoji&quot;; font-size: 16px; font-weight: 400; margin: 0px; text-decoration: none; border-bottom: 0px rgb(230, 232, 240);"><li class="Z1qcYe" data-sfc-cp="" jsaction="" jscontroller="oSLmPe#HokZ0d" data-sfc-root="c" jsuid="pZe2Mc_ce" data-hveid="CAIIDxAE" style="font-family: &quot;Google Sans&quot;, Arial, sans-serif, &quot;Noto Color Emoji&quot;; font-size: 16px; font-weight: 400; margin: 0px 0px 12px; text-decoration: none; border-bottom: 0px rgb(230, 232, 240);"><span class="iNqyIf" data-sfc-cp="" jsaction="" jscontroller="KdQfFc#d5Qebf" data-sfc-root="c" jsuid="pZe2Mc_cf" style="font-family: &quot;Google Sans&quot;, Arial, sans-serif, &quot;Noto Color Emoji&quot;; font-size: 16px; font-weight: 400; margin: 0px; text-decoration: none; border-bottom: 0px rgb(230, 232, 240);">Thành công và sự gắn kết đến từ việc <strong class="rQesXe MPyX" data-sfc-cp="" jsaction="" jscontroller="tP2kf#s32ZS" data-sfc-root="c" jsuid="pZe2Mc_cg" style="font-family: &quot;Google Sans&quot;, Arial, sans-serif, &quot;Noto Color Emoji&quot;; font-size: 16px; font-weight: 700; margin: 0px; text-decoration: none; border-bottom: 0px rgb(230, 232, 240);">quan tâm chân thành<!--TgQPHd|||[]--></strong> và <strong class="rQesXe MPyX" data-sfc-cp="" jsaction="" jscontroller="tP2kf#s32ZS" data-sfc-root="c" jsuid="pZe2Mc_ch" style="font-family: &quot;Google Sans&quot;, Arial, sans-serif, &quot;Noto Color Emoji&quot;; font-size: 16px; font-weight: 700; margin: 0px; text-decoration: none; border-bottom: 0px rgb(230, 232, 240);">nâng đỡ giá trị<!--TgQPHd|||[]--></strong> của người khác, không phải từ sự ép buộc hay thao túng.<!--TgQPHd|||[]--></span><!--TgQPHd|||[]--></li><!--TgQPHd|||[]--></div><div data-bfc="" class="" data-sfc-cp="" jsaction="" jscontroller="eHDDzf#DtOdcb" data-sfc-root="c" jsuid="pZe2Mc_cj" data-ved="2ahUKEwikxJ_c7qCYAxUYSPUHHeGJAt4Qi4wTegYIAggPEAU" data-hveid="CAIIDxAF" style="font-family: &quot;Google Sans&quot;, Arial, sans-serif, &quot;Noto Color Emoji&quot;; font-size: 16px; font-weight: 400; margin: 0px; text-decoration: none; border-bottom: 0px rgb(230, 232, 240);"><li class="Z1qcYe" data-sfc-cp="" jsaction="" jscontroller="oSLmPe#HokZ0d" data-sfc-root="c" jsuid="pZe2Mc_ck" data-hveid="CAIIDxAG" style="font-family: &quot;Google Sans&quot;, Arial, sans-serif, &quot;Noto Color Emoji&quot;; font-size: 16px; font-weight: 400; margin: 0px; text-decoration: none; border-bottom: 0px rgb(230, 232, 240);"><span class="iNqyIf" data-sfc-cp="" jsaction="" jscontroller="KdQfFc#d5Qebf" data-sfc-root="c" jsuid="pZe2Mc_cl" style="font-family: &quot;Google Sans&quot;, Arial, sans-serif, &quot;Noto Color Emoji&quot;; font-size: 16px; font-weight: 400; margin: 0px; text-decoration: none; border-bottom: 0px rgb(230, 232, 240);">Khi áp dụng vào đời sống hiện đại, người đọc nên chọn lọc và linh hoạt điều chỉnh cho phù hợp với văn hóa giao tiếp thực tế.</span></li></div></ul>', 1, '2026-11-24 21:57:12', 1, N'MS0003');
SET IDENTITY_INSERT [tblBook] OFF;
GO

-- -------------------------------------------------------------
-- Bảng: [tblUser] (9 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [tblUser] ON;
INSERT INTO [tblUser] ([UserID], [UserName], [FullName], [Email], [Password], [Role], [IsActive], [UserCode], [GoogleId], [AuthProvider]) VALUES
(1, N'docgia01', N'Nguyễn Văn An', N'an.nguyen@gmail.com', N'123456', N'Reader', 1, NULL, NULL, NULL),
(2, N'docgia02', N'Trần Thị Mai', N'mai.tran@gmail.com', N'123456', N'Reader', 1, NULL, NULL, NULL),
(4, N'Tai', N'Tai', N'tai@gmail.com', N'04122446ac673ab64ca79159060709ed', N'Reader', 1, NULL, NULL, NULL),
(1013, N'admin', N'Nguyễn Quản Trị (Admin)', N'admin@thuvien.edu.vn', N'AQAAAAIAAYagAAAAEPmED+p4W2Ygp75u1mFX3+aIPOvwLeu1sN0bTNwkPzKGu8v8aLSTJbSRFYsFIb7UoA==', N'Admin', 1, NULL, NULL, NULL),
(1014, N'Tai123', N'Nguyễn Hữu Tài', N'tai123@gmail.com', N'2a417fee7c18e2f58b645e032084e11a', N'Reader', 1, N'DG1014', NULL, NULL),
(1030, N'Ngan123', N'Lê Thị Ngân', N'ngan123@gmail.com', N'AQAAAAIAAYagAAAAEGBeQULQ0yLTWKHq8np4Eukpk+rcWm0nGwEQ0IZdl27at1LFm35YbcUNfYDXsZg4Tw==', N'Reader', 1, N'DG1030', NULL, NULL),
(1031, N'bobo12', N'Nguyễn Thị Bơ', N'bo12@gmail.com', N'AQAAAAIAAYagAAAAEOb0yKt8OQn3ct24lsiNedAvjApatnkq3zdJGHP4lcK1vDCpdQ7H3EFvBxfV+cvDKA==', N'Reader', 1, N'DG1031', NULL, NULL),
(1032, N'KimChi', N'Lê Kim Chi', N'kimchi123@gmail.com', N'AQAAAAIAAYagAAAAEGQdxjEDsWezGWs/QVuDcQ8O/fPyoTNj/XVJI0UjPZzxksdhOjapsHvMnpKUl3fSaw==', N'Reader', 1, N'DG1032', NULL, NULL),
(1033, N'NgaHoang', N'Hoàng Thị Nga', N'ngahoang@gmail.com', N'AQAAAAIAAYagAAAAENtXIvI4iNx2AFLhIoDZX0wdOB19G9iiDHbBpJdkj+iUMtxVEwki8zJ5hD31xoBx8A==', N'Reader', 1, N'DG1033', NULL, NULL);
SET IDENTITY_INSERT [tblUser] OFF;
GO

-- -------------------------------------------------------------
-- Bảng: [tblBorrowTicket] (5 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [tblBorrowTicket] ON;
INSERT INTO [tblBorrowTicket] ([TicketID], [UserID], [BorrowDate], [DueDate], [ReturnDate], [Status], [Note], [TicketCode], [ReturnCode]) VALUES
(1, 1, '2026-11-07 10:51:27', '2026-11-21 00:00:00', '2026-11-07 10:51:42', N'Đã trả', NULL, NULL, NULL),
(2, 4, '2026-11-23 19:02:07', '2026-12-13 00:00:00', '2026-11-23 19:17:29', N'Đã trả', NULL, N'PM0002', N'PT0002'),
(3, 1014, '2026-11-23 20:13:19', '2026-12-07 20:12:19', '2026-11-23 20:22:40', N'Đã trả', N'Đăng ký mượn trực tuyến qua trang chi tiết sách', N'PM0003', N'PT0003'),
(4, 1031, '2026-11-24 21:47:15', '2026-12-08 21:46:58', NULL, N'Đang mượn', N'Đăng ký mượn trực tuyến qua trang chi tiết sách', N'PM0004', NULL),
(5, 1032, '2026-11-25 14:41:53', '2026-12-09 14:41:39', NULL, N'Đang mượn', N'Đăng ký mượn trực tuyến qua trang chi tiết sách', N'PM0005', NULL);
SET IDENTITY_INSERT [tblBorrowTicket] OFF;
GO

-- -------------------------------------------------------------
-- Bảng: [tblBorrowDetail] (5 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [tblBorrowDetail] ON;
INSERT INTO [tblBorrowDetail] ([DetailID], [TicketID], [BookID], [FineAmount], [IsReturned], [ReturnDate], [Note]) VALUES
(1, 1, 1, 0.00, 0, NULL, NULL),
(2, 2, 1, 0.00, 1, '2026-11-23 19:17:29', N'Hoàn kho toàn bộ qua nút thao tác nhanh'),
(3, 3, 2, 0.00, 1, '2026-11-23 20:22:40', NULL),
(4, 4, 2, 0.00, 0, NULL, NULL),
(5, 5, 2, 0.00, 0, NULL, NULL);
SET IDENTITY_INSERT [tblBorrowDetail] OFF;
GO

-- -------------------------------------------------------------
-- Bảng: [tblAdminMenu] (11 bản ghi)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [tblAdminMenu] ON;
INSERT INTO [tblAdminMenu] ([AdminMenuID], [ItemName], [ItemLevel], [ParentLevel], [ItemOrder], [IsActive], [ItemTarget], [AreaName], [ControllerName], [ActionName], [Icon], [IdName]) VALUES
(1, N'Bảng điều khiển', 1, 0, 1, 1, NULL, N'Admin', N'Home', N'Index', N'bi bi-grid', NULL),
(2, N'Quản lý Sách', 1, 0, 2, 1, N'#books-nav', N'Admin', N'Book', N'Index', N'bi bi-book', N'books-nav'),
(3, N'Quản lý Mượn / Trả', 1, 0, 3, 1, N'#borrow-nav', N'Admin', N'Borrow', N'Index', N'bi bi-journal-arrow-right', N'borrow-nav'),
(4, N'Quản trị Hệ thống', 1, 0, 4, 1, N'#system-nav', N'Admin', N'User', N'Index', N'bi bi-people', N'system-nav'),
(5, N'Danh mục sách', 2, 2, 1, 1, NULL, N'Admin', N'Category', N'Index', N'bi bi-circle', NULL),
(6, N'Tác giả', 2, 2, 2, 1, NULL, N'Admin', N'Author', N'Index', N'bi bi-circle', NULL),
(7, N'Nhà xuất bản', 2, 2, 3, 1, NULL, N'Admin', N'Publisher', N'Index', N'bi bi-circle', NULL),
(8, N'Kho sách', 2, 2, 4, 1, NULL, N'Admin', N'Book', N'Index', N'bi bi-circle', NULL),
(9, N'Lập phiếu mượn', 2, 3, 1, 1, NULL, N'Admin', N'Borrow', N'Create', N'bi bi-circle', NULL),
(10, N'Danh sách phiếu mượn', 2, 3, 2, 1, NULL, N'Admin', N'Borrow', N'Index', N'bi bi-circle', NULL),
(11, N'Tài khoản người dùng', 2, 4, 1, 1, NULL, N'Admin', N'User', N'Index', N'bi bi-circle', NULL);
SET IDENTITY_INSERT [tblAdminMenu] OFF;
GO

