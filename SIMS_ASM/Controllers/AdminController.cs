using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using Microsoft.EntityFrameworkCore;

namespace SIMS_ASM.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContex _context;

        public AdminController(ApplicationDbContex context)
        {
            _context = context;
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
            }
            return RedirectToAction("ManageCourses");
        }
    }

}
