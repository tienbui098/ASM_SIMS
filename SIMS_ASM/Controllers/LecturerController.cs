using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Singleton;
using System.Data.Entity;

namespace SIMS_ASM.Controllers
{
    public class LecturerController : Controller
    {
        private readonly ApplicationDbContex _context;
        private readonly AccountSingleton _singleton;

        public LecturerController(ApplicationDbContex context)
        {
            _context = context;
            _singleton = AccountSingleton.Instance;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ManageLecturer()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }
        //private readonly ApplicationDbContex _context;
        //private readonly AccountSingleton _singleton;

        //public LecturerController(ApplicationDbContex context)
        //{
        //    _context = context;
        //    _singleton = AccountSingleton.Instance;
        //}

        //// Trang chính cho giảng viên
        //public async Task<IActionResult> Index()
        //{
        //    var userId = HttpContext.Session.GetInt32("UserId");
        //    if (userId == null)
        //    {
        //        _singleton.Log("Unauthorized access to Lecturer dashboard: User not logged in");
        //        return RedirectToAction("Login", "Account");
        //    }

        //    var grades = await _context.Grades
        //        .Where(g => g.UserID == userId)
        //        .Include(g => g.Course)
        //        .Include(g => g.User)
        //        .ToListAsync();
        //    return View(grades);
        //}
    }
}

