using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Singleton;
using SIMS_ASM.Services;

namespace SIMS_ASM.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContex _context;
        private readonly IUserService _userService;
        private readonly AccountSingleton _singleton;

        public StudentController(ApplicationDbContex context, IUserService userService)
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


        // Kiểm tra quyền Student
        private bool IsStudent()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Student";
        }
        // Lấy thông tin người dùng hiện tại
        private string GetCurrentUsername()
        {
            return HttpContext.Session.GetString("Username");
        }
        // Trang chính cho sinh viên
        public async Task<IActionResult> Index()
        {
            if (!IsStudent())
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


        public async Task<IActionResult> ManageStudent()
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Class Management: User not an admin");
                return RedirectToAction("Login", "Account");
            }
            var users = await _context.Users.ToListAsync();
            return View(users);
        }


        public IActionResult AddStudent()
        {
            ViewBag.SystemName = "Student Information Management System";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(User user)
        {
            if (!ModelState.IsValid)
            {
                _singleton.Log($"Failed adding student: Invalid data for {user.Username}");
                ViewBag.SystemName = "Student Information Management System";
                return View(user);
            }

            try
            {
                await _userService.AddUserAsync(user, "Student"); // Gán role là Student
                _singleton.Log($"User {user.Username} added successfully with role Student");
                return RedirectToAction("ManageStudent"); // Chuyển hướng tùy ý
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("Username", ex.Message);
                _singleton.Log($"Failed adding student: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                _singleton.Log($"Failed adding student: {ex.Message}");
            }
            catch (Exception ex)
            {
                _singleton.Log($"Failed to add student {user.Username}: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while adding.");
            }

            ViewBag.SystemName = "Student Information Management System";
            return View(user);
        }


        [HttpGet]
        public async Task<IActionResult> UpdateStudent(int id)
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
        public async Task<IActionResult> UpdateStudent(int id, User updatedUser)
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
                    return RedirectToAction(nameof(ManageStudent));
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

            return RedirectToAction(nameof(ManageStudent));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }
    }

}
