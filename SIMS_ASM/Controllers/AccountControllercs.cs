using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using SIMS_ASM.Singleton;
using System.Text.RegularExpressions;
using System.ComponentModel;
using SIMS_ASM.Services;

namespace SIMS_ASM.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly AccountSingleton _singleton;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
            _singleton = AccountSingleton.Instance;
        }

        // Hiển thị form đăng nhập
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.SystemName = "Student Information Management System";
            return View();
        }

        // Xử lý đăng nhập
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Username and password are required.");
                _singleton.Log($"Failed login attempt: Username or password is empty");
                return View();
            }

            var user = await _accountService.AuthenticateAsync(username, password);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                _singleton.Log($"Failed login attempt for user {username}");
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.UserID);
            HttpContext.Session.SetString("Role", user.Role);
            _singleton.Log($"User {username} logged in with role {user.Role}");

            switch (user.Role)
            {
                case "Student":
                    return RedirectToAction("Index", "Student");
                case "Lecturer":
                    return RedirectToAction("Index", "Lecturer");
                case "Admin":
                    return RedirectToAction("Index", "Admin");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }


        // Hiển thị form đăng ký
        public IActionResult Register()
        {
            ViewBag.SystemName = "Student Information Management System";
            return View();
        }

        // Xử lý đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
            {
                _singleton.Log($"Failed registration attempt: Invalid data for {user.Username}");
                return View(user);
            }

            var validRoles = new[] { "Student", "Lecturer", "Admin" };
            if (!validRoles.Contains(user.Role))
            {
                ModelState.AddModelError("Role", "Role must be Student, Lecturer, or Admin.");
                _singleton.Log($"Failed registration attempt: Invalid role {user.Role}");
                return View(user);
            }

            // Kiểm tra regex cho Username (vì Data Annotations không đủ mạnh để xử lý regex phức tạp)
            if (!Regex.IsMatch(user.Username, "^[a-z0-9]+$"))
            {
                ModelState.AddModelError("Username", "Username can only contain lowercase letters and numbers.");
                _singleton.Log($"Failed registration attempt: Invalid username format {user.Username}");
                return View(user);
            }

            try
            {
                await _accountService.RegisterAsync(user);
                _singleton.Log($"User {user.Username} registered successfully with role {user.Role}");
                return RedirectToAction("Login");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                _singleton.Log($"Failed registration attempt: {ex.Message}");
                return View(user);
            }
            catch (Exception ex)
            {
                _singleton.Log($"Failed to register user {user.Username}: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while registering.");
                return View(user);
            }

        }



        // Đăng xuất
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var username = userId.HasValue
                ? _accountService.AuthenticateAsync("", "").Result?.Username ?? "Unknown"
                : "Unknown";
            HttpContext.Session.Clear();
            _singleton.Log($"User {username} logged out");
            return RedirectToAction("Login");
        }



        // Hàm mã hóa password (ví dụ đơn giản)
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var base64 = Convert.ToBase64String(hashedBytes);
                return base64.Length > 50 ? base64.Substring(0, 50) : base64; // Cắt ngắn nếu vượt quá 50
            }
        }
    }
}
