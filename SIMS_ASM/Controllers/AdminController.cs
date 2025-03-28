using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Singleton;
using SIMS_ASM.Models;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;
using SIMS_ASM.Services;

namespace SIMS_ASM.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContex _context;
        private readonly IUserService _userService; // Thêm IUserService
        private readonly AccountSingleton _singleton;

        public AdminController(ApplicationDbContex context, IUserService userService)
        {
            _context = context;
            _userService = userService;
            _singleton = AccountSingleton.Instance;
        }

        // Trang chính cho quản trị viên
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> ManageAdmin()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // Hiển thị form thêm Admin
        public IActionResult AddAdmin()
        {
            ViewBag.SystemName = "Student Information Management System";
            return View();
        }

        // Xử lý thêm Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAdmin(User user)
        {
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    var key = entry.Key;
                    var errors = entry.Value.Errors.Select(e => e.ErrorMessage);
                    _singleton.Log($"Property: {key}, Errors: {string.Join(", ", errors)}");
                }
                ViewBag.SystemName = "Student Information Management System";
                return View(user);
            }

            try
            {
                await _userService.AddUserAsync(user, "Admin");
                _singleton.Log($"User {user.Username} added successfully with role Admin");
                return RedirectToAction("ManageAdmin");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("Username", ex.Message);
                _singleton.Log($"Failed adding attempt: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                _singleton.Log($"Failed adding attempt: {ex.Message}");
            }
            catch (Exception ex)
            {
                _singleton.Log($"Failed to add user {user.Username}: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while adding. Please try again.");
            }

            ViewBag.SystemName = "Student Information Management System";
            return View(user);
        }


        [HttpGet]
        public async Task<IActionResult> UpdateAdmin(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAdmin(int id, User updatedUser)
        {
            if (id != updatedUser.UserID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Update specific fields, keeping sensitive info like password secure
                    existingUser.FullName = updatedUser.FullName;
                    existingUser.Email = updatedUser.Email;
                    existingUser.Date_of_birth = updatedUser.Date_of_birth;
                    existingUser.Address = updatedUser.Address;
                    existingUser.Phone_number = updatedUser.Phone_number;
                    existingUser.Gender = updatedUser.Gender;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ManageAdmin));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(updatedUser);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }
    }
}
