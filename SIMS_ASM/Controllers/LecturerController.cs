using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Singleton;

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

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

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

