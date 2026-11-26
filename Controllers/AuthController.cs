using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LibraryManagement.Models;
using LibraryManagement.Utilities;

namespace LibraryManagement.Controllers
{
    public class AuthController : Controller
    {
        private readonly DataContext _context;

        public AuthController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("/Auth/Login")]
        public IActionResult Login(string? returnUrl = null)
        {
            if (Functions.IsLogin())
            {
                return Functions.IsAdmin() ? Redirect("/Admin") : Redirect("/Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [Route("/Auth/Login")]
        public async Task<IActionResult> Login(string? userName, string? password, bool rememberMe = false, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!";
                return View();
            }

            string inputUserName = userName.Trim();
            var check = _context.Users.FirstOrDefault(u =>
                (u.UserName != null && u.UserName.ToLower() == inputUserName.ToLower()) ||
                (u.UserCode != null && u.UserCode.ToLower() == inputUserName.ToLower())
            );

            if (check == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác!";
                return View();
            }

            if (check.IsActive == false)
            {
                ViewBag.Error = "Tài khoản của bạn hiện đang bị khóa!";
                return View();
            }

            if (check.Password == PasswordSecurity.GoogleOAuthPlaceholder)
            {
                ViewBag.Error = "Tài khoản này được tạo qua Google. Vui lòng nhấn 'Đăng nhập bằng Google'!";
                return View();
            }

            bool isValid = PasswordSecurity.VerifyPassword(check, check.Password, password, out bool needRehash);
            if (!isValid)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác!";
                return View();
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
                return Redirect("/Admin");
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("/Home");
        }

        [HttpGet]
        [Route("/Auth/Register")]
        public IActionResult Register()
        {
            if (Functions.IsLogin())
            {
                return Redirect("/Home");
            }
            return View();
        }

        [HttpPost]
        [Route("/Auth/Register")]
        public async Task<IActionResult> Register(string? fullName, string? userName, string? email, string? password, string? confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu!";
                return View();
            }

            userName = userName.Trim();
            if (userName.Length < 3)
            {
                ViewBag.Error = "Tên đăng nhập phải có ít nhất 3 ký tự!";
                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.Error = "Mật khẩu phải có ít nhất 6 ký tự!";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.UserName != null && u.UserName.ToLower() == userName.ToLower());
            if (existingUser != null)
            {
                ViewBag.Error = "Tên đăng nhập này đã được sử dụng! Vui lòng chọn tên khác.";
                return View();
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                email = email.Trim();
                var existingEmail = _context.Users.FirstOrDefault(u => u.Email != null && u.Email.ToLower() == email.ToLower());
                if (existingEmail != null)
                {
                    ViewBag.Error = "Địa chỉ email này đã được sử dụng!";
                    return View();
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

                TempData["AuthSuccess"] = $"Đăng ký tài khoản Bạn đọc [{newUser.Code}] thành công!";
                return Redirect("/Profile");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra khi tạo tài khoản: " + (ex.InnerException?.Message ?? ex.Message);
                return View();
            }
        }

        [Route("/Auth/Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Functions.ClearSession();
            return Redirect("/Home");
        }
    }
}
