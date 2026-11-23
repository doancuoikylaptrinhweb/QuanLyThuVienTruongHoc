using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using LibraryManagement.Utilities;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Librarian")]
    public class AdminBaseController : Controller
    {
        // Bộ lọc phân quyền: Tách riêng toàn bộ không gian Admin, bắt buộc xác thực Cookie với vai trò Admin hoặc Thủ thư
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                Functions._Message = "Vui lòng đăng nhập tài khoản Quản trị để truy cập!";
                context.Result = new RedirectToActionResult("Index", "Login", new { area = "Admin" });
            }
            else if (!user.IsInRole("Admin") && !user.IsInRole("Librarian"))
            {
                // Người dùng đã đăng nhập nhưng không có vai trò Admin / Thủ thư (ví dụ độc giả Reader)
                context.Result = new RedirectToActionResult("AccessDenied", "Login", new { area = "Admin" });
            }

            base.OnActionExecuting(context);
        }
    }
}
