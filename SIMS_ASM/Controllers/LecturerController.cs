using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Singleton;
using SIMS_ASM.Services;

namespace SIMS_ASM.Controllers
{
    public class LecturerController : Controller
    {
        private readonly ApplicationDbContex _context;
        private readonly IUserService _userService;
        private readonly AccountSingleton _singleton;

        public LecturerController(ApplicationDbContex context, IUserService userService)
        {
            _context = context;
            _userService = userService;
            _singleton = AccountSingleton.Instance;
        }
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        private bool IsLecturer()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Lecturer";
        }

        // Lấy thông tin người dùng hiện tại
        private string GetCurrentUsername()
        {
            return HttpContext.Session.GetString("Username");
        }
        // Trang chính cho sinh viên
        public async Task<IActionResult> Index()
        {
            if (!IsLecturer())
            {
                return RedirectToAction("Login", "Account");
            }

            var username = GetCurrentUsername();
            var user = await _userService.GetUserByUsernameAsync(username);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        public async Task<IActionResult> ManageLecturer()
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Class Management: User not an admin");
                return RedirectToAction("Login", "Account");
            }
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        public IActionResult AddLecturer()
        {
            ViewBag.SystemName = "Student Information Management System";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLecturer(User user)
        {
            if (!ModelState.IsValid)
            {
                _singleton.Log($"Failed adding teacher: Invalid data for {user.Username}");
                ViewBag.SystemName = "Student Information Management System";
                return View(user);
            }

            try
            {
                await _userService.AddUserAsync(user, "Lecturer"); // Gán role là Lecturer
                _singleton.Log($"User {user.Username} added successfully with role Lecturer");
                return RedirectToAction("ManageLecturer"); // Chuyển hướng tùy ý
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("Username", ex.Message);
                _singleton.Log($"Failed adding teacher: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                _singleton.Log($"Failed adding teacher: {ex.Message}");
            }
            catch (Exception ex)
            {
                _singleton.Log($"Failed to add teacher {user.Username}: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while adding.");
            }

            ViewBag.SystemName = "Student Information Management System";
            return View(user);
        }

        [HttpGet]
            public async Task<IActionResult> Delete(int id)
            {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to delete major: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                _singleton.Log($"Failed to delete user with ID {id}: User not found or has associated StudentClass/ClassCourseFaculty/Enrollment");
                TempData["ErrorMessage"] = "Cannot delete user because it is associated with Class Courses or Student Classes or Enrollments, or it was not found.";
            }
            else
            {
                _singleton.Log($"User with ID {id} deleted by admin");
                TempData["SuccessMessage"] = "User deleted successfully!";
            }

            return RedirectToAction(nameof(ManageLecturer));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateLecturer(int id)
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
        public async Task<IActionResult> UpdateLecturer(int id, User updatedUser)
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
                    return RedirectToAction(nameof(ManageLecturer));
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

