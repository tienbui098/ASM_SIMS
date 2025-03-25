using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Singleton;
using SIMS_ASM.Models;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;

namespace SIMS_ASM.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContex _context;
        private readonly AccountSingleton _singleton;

        public AdminController(ApplicationDbContex context)
        {
            _context = context;
            _singleton = AccountSingleton.Instance;
        }

        // Trang chính cho quản trị viên
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // Quản lý tài khoản người dùng
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUsers(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                _singleton.Log($"User {user.Username} (ID: {id}) deleted by admin");
            }
            else
            {
                _singleton.Log($"Failed to delete user with ID {id}: User not found");
            }
            return RedirectToAction("ManageUsers");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }


        // Hiển thị form đăng ký
        public IActionResult AddUser()
        {
            ViewBag.SystemName = "Student Information Management System";
            return View();
        }

        // Xử lý đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(User user)
        {

            // Kiểm tra username chỉ chứa chữ thường và số
            if (!Regex.IsMatch(user.Username, "^[a-z0-9]+$"))
            {
                ModelState.AddModelError("Username", "Username can only contain lowercase letters and numbers.");
                _singleton.Log($"Failed adding attempt: Invalid username format {user.Username}");
                return View(user);
            }


            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    var key = entry.Key; // Tên property
                    var errors = entry.Value.Errors.Select(e => e.ErrorMessage); // Danh sách lỗi
                    _singleton.Log($"Property: {key}, Errors: {string.Join(", ", errors)}");
                }
                return View(user);
            }

            // Kiểm tra username đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Username already exists.");
                _singleton.Log($"Failed adding attempt: Username {user.Username} already exists");
                return View(user);
            }

            // Kiểm tra email đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                _singleton.Log($"Failed adding attempt: Email {user.Email} already exists");
                return View(user);
            }

            // Thêm người dùng mới vào cơ sở dữ liệu
            try
            {
                // Mã hóa mật khẩu
                user.Password = HashPassword(user.Password);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _singleton.Log($"User {user.Username} added successfully with role {user.Role}");
            }
            catch (Exception ex)
            {
                _singleton.Log($"Failed to add user {user.Username}: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while adding. Please try again.");
                return View(user);
            }
            // Chuyển hướng về trang đăng nhập sau khi đăng ký thành công
            return RedirectToAction("Login");
        }
    }
}
