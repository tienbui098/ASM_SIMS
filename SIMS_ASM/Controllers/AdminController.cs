using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Singleton;

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
                _singleton.Log($"User {user.Username} (ID: {id}) deleted by Admin");
            }
            else
            {
                _singleton.Log($"Failed to delete user with ID {id}: User not found");
            }
            return RedirectToAction("ManageUsers");
        }

        // Quản lý khóa học
        public IActionResult ManageCourses()
        {
            var courses = _context.Courses.ToList();
            return View(courses);
        }

        [HttpPost]
        public async Task<IActionResult> ManageCourses(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                _singleton.Log($"Course {course.CourseName} (ID: {id}) deleted by admin");
            }
            else
            {
                _singleton.Log($"Failed to delete course with ID {id}: Course not found");
            }
            return RedirectToAction("ManageCourses");
        }
    }

}
