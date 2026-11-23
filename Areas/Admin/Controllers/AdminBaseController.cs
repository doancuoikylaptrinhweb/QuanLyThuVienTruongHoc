using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using LibraryManagement.Utilities;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminBaseController : Controller
    {
        // Bộ lọc phân quyền: Tách riêng toàn bộ không gian Admin, bắt buộc đăng nhập với quyền Admin hoặc Thủ thư
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!Functions.IsLogin() || !Functions.IsAdmin())
            {
                Functions._Message = "Vui lòng đăng nhập tài khoản Quản trị để truy cập!";
                context.Result = new RedirectToActionResult("Index", "Login", new { area = "Admin" });
            }
            base.OnActionExecuting(context);
        }
    }
}
