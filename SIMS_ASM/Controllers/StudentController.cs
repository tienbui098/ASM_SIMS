using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Singleton;

namespace SIMS_ASM.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContex _context;
        private readonly AccountSingleton _singleton;

        public StudentController(ApplicationDbContex context)
        {
            _context = context;
            _singleton = AccountSingleton.Instance;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ManageStudent()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        //private readonly ApplicationDbContex _context;

        //public StudentController(ApplicationDbContex context)
        //{
        //    _context = context;
        //}

        //// Trang chính cho sinh viên
        //public async Task<IActionResult> Index()
        //{
        //    var userId = HttpContext.Session.GetInt32("UserId");
        //    if (userId == null)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    var courses = await _context.Courses
        //        .Where(c => c.UserID == userId)
        //        .Include(c => c.Grade)
        //        .ToListAsync();
        //    return View(courses);
        //}

    }
}
