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

        // 1. TRANG CHỦ & TRA CỨU SÁCH TRỰC TUYẾN (Mục 8 trong đề tài)
        public IActionResult Index(string? searchString, int? categoryId)
        {
            var booksQuery = _context.Books.Where(b => b.IsActive == true);

            // Tìm kiếm theo tên sách
            if (!string.IsNullOrEmpty(searchString))
            {
                booksQuery = booksQuery.Where(b => b.Title.Contains(searchString) || b.Summary.Contains(searchString));
                ViewBag.SearchString = searchString;
            }

            // Lọc theo thể loại
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                booksQuery = booksQuery.Where(b => b.CategoryID == categoryId.Value);
                ViewBag.SelectedCategory = categoryId.Value;
            }

            var books = booksQuery.OrderByDescending(b => b.BookID).ToList();

            // Nạp dữ liệu phụ trợ
            ViewBag.Categories = _context.Categories.Where(c => c.IsActive == true).ToList();
            ViewBag.Authors = _context.Authors.ToDictionary(a => a.AuthorID, a => a.AuthorName);

            return View(books);
        }

        // 2. XEM CHI TIẾT SÁCH DÙNG SLUG GENERATOR (Lab 05)
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

            // Lấy thông tin Tác giả, Thể loại, NXB
            ViewBag.Category = _context.Categories.Find(book.CategoryID);
            ViewBag.Author = _context.Authors.Find(book.AuthorID);
            ViewBag.Publisher = _context.Publishers.Find(book.PublisherID);

            return View(book);
        }

        // 3. XỬ LÝ CHATBOT VÀ HỖ TRỢ AI (Mục 11)
[HttpPost]
public IActionResult ChatBot(string message)
{
    if (string.IsNullOrWhiteSpace(message))
    {
        return Json(new { reply = "Xin chào! Tôi có thể giúp gì cho bạn trong việc tìm sách hôm nay?" });
    }

    string msg = message.ToLower().Trim();

    // 1. Chào hỏi
    if (msg.Contains("chào") || msg.Contains("hello") || msg.Contains("hi") || msg.Contains("bạn là ai"))
    {
        return Json(new { 
            reply = "Xin chào bạn! Tôi là <strong>Trợ lý Ảo AI Thư viện</strong>. Tôi có thể giúp bạn gợi ý sách hay, tra cứu kho sách và hướng dẫn thủ tục mượn/trả sách!" 
        });
    }

    // 2. Hỏi về thủ tục & quy định mượn sách
    if (msg.Contains("mượn") || msg.Contains("hạn") || msg.Contains("trả") || msg.Contains("thủ tục") || msg.Contains("quy định"))
    {
        return Json(new { 
            reply = "📚 <strong>Quy định mượn sách:</strong><br/>" +
                    "- Thời hạn mượn: Tối đa <strong>14 ngày</strong>.<br/>" +
                    "- Bạn hãy đến gặp thủ thư tại quầy và đọc Mã độc giả của mình để được quét mượn sách nhé!" 
        });
    }

    // 3. Hỏi gợi ý sách / tìm sách (Tự động quét CSDL LibraryDB)
    if (msg.Contains("sách") || msg.Contains("gợi ý") || msg.Contains("tìm") || msg.Contains("có gì") || msg.Contains("kho"))
    {
        var availableBooks = _context.Books
            .Where(b => b.IsActive == true && b.AvailableQuantity > 0)
            .OrderByDescending(b => b.BookID)
            .Take(4)
            .ToList();

        if (availableBooks.Count == 0)
        {
            return Json(new { reply = "Hiện tại các đầu sách trên kệ tạm thời đang được mượn hết. Bạn vui lòng quay lại sau nhé!" });
        }

        string bookList = "Dưới đây là một số cuốn sách đang có sẵn trên kệ thư viện:<br/>";
        foreach (var b in availableBooks)
        {
            bookList += $"📖 <strong>{b.Title}</strong> (còn {b.AvailableQuantity} cuốn)<br/>";
        }
        bookList += "<br/>👉 Bạn có thể gõ tên sách vào thanh tìm kiếm trên trang chủ để xem chi tiết nhé!";

        return Json(new { reply = bookList });
    }

    // 4. Tìm kiếm sách cụ thể theo từ khóa người dùng gõ
    var matchedBooks = _context.Books
        .Where(b => b.IsActive == true && (b.Title.Contains(message) || b.Summary.Contains(message)))
        .Take(3)
        .ToList();

    if (matchedBooks.Count > 0)
    {
        string result = $"Tìm thấy {matchedBooks.Count} sách liên quan đến yêu cầu của bạn:<br/>";
        foreach (var b in matchedBooks)
        {
            result += $"• <strong>{b.Title}</strong> (Còn: {b.AvailableQuantity} cuốn)<br/>";
        }
        return Json(new { reply = result });
    }

    // Phản hồi mặc định
    return Json(new { 
        reply = "Tôi chưa hiểu rõ câu hỏi của bạn lắm. Bạn có thể hỏi tôi về: <em>'Gợi ý sách'</em>, <em>'Quy định mượn sách'</em> hoặc gõ tên cuốn sách bạn quan tâm nhé!" 
    });
}
    }
}