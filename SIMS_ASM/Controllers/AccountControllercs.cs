using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using SIMS_ASM.Singleton;

namespace SIMS_ASM.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContex _context;
        private readonly AccountSingleton _singleton;

        public AccountController(ApplicationDbContex context)
        {
            _context = context;
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

            string hashedPassword = HashPassword(password);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == hashedPassword);

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
                case "Administrator":
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
        public async Task<IActionResult> Register(User user, string password)
        {
            if (!ModelState.IsValid)
            {
                _singleton.Log($"Failed registration attempt: Invalid model state for username {user.Username}");
                return View(user);
            }

            // Kiểm tra username đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Username already exists.");
                _singleton.Log($"Failed registration attempt: Username {user.Username} already exists");
                return View(user);
            }

            // Kiểm tra email đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                _singleton.Log($"Failed registration attempt: Email {user.Email} already exists");
                return View(user);
            }

            // Mã hóa mật khẩu
            user.Password = HashPassword(password);

            // Thêm người dùng mới vào cơ sở dữ liệu
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _singleton.Log($"User {user.Username} registered successfully with role {user.Role}");

            // Chuyển hướng về trang đăng nhập sau khi đăng ký thành công
            return RedirectToAction("Login");
        }

        // Đăng xuất
        public IActionResult Logout()
        {
            var username = _context.Users.Where(u => u.UserID == HttpContext.Session.GetInt32("UserId")).Select(u => u.Username).FirstOrDefault();
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
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
