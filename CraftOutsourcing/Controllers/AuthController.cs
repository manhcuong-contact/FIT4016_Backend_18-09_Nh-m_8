using System.Security.Claims;
using CraftOutsourcing.Data;
using CraftOutsourcing.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CraftOutsourcing.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
            {
                if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");
                return RedirectToAction("Index", "User");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ tài khoản và mật khẩu.";
                return View();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác.";
                return View();
            }

            // Kiểm tra tài khoản có được duyệt và active không
            if (!user.IsApproved)
            {
                ViewBag.Error = "Tài khoản chưa được phê duyệt bởi Admin. Vui lòng chờ.";
                return View();
            }

            if (!user.IsActive)
            {
                ViewBag.Error = "Tài khoản đã bị vô hiệu hóa. Liên hệ quản trị viên.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (user.Role.Name == "Admin")
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "User");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
            {
                if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");
                return RedirectToAction("Index", "User");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string fullname, string phone, string? address)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fullname))
            {
                ViewBag.Error = "Vui long nhap du cac thong tin bat buoc.";
                return View();
            }

            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                ViewBag.Error = "Tài khoản đã tồn tại.";
                return View();
            }

            var newUser = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FullName = fullname,
                Phone = phone,
                Address = address,
                RoleId = 2, // User (Hộ dân) - không cần phê duyệt
                IsApproved = true,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMsg"] = "Đăng ký thành công, vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // Đăng ký Admin - cần Admin khác phê duyệt
        [HttpGet]
        public IActionResult RegisterAdmin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAdmin(string username, string password, string fullname, string phone)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fullname))
            {
                ViewBag.Error = "Vui lòng nhập đủ các thông tin bắt buộc.";
                return View();
            }

            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                ViewBag.Error = "Tài khoản đã tồn tại.";
                return View();
            }

            var newAdmin = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FullName = fullname,
                Phone = phone,
                RoleId = 1, // Admin
                IsApproved = false, // CẦN ĐƯỢC PHÊ DUYỆT
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(newAdmin);
            await _context.SaveChangesAsync();

            TempData["SuccessMsg"] = "Đăng ký Admin thành công! Tài khoản cần được Admin hiện tại phê duyệt trước khi đăng nhập.";
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
