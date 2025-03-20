using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using Microsoft.EntityFrameworkCore;

namespace SIMS_ASM.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContex _context;

        public TeacherController(ApplicationDbContex context)
        {
            _context = context;
        }

        // Trang chính cho giảng viên
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var grades = await _context.Grades
                .Where(g => g.UserID == userId)
                .Include(g => g.Course)
                .Include(g => g.User)
                .ToListAsync();
            return View(grades);
        }

        // Cập nhật điểm số
        public async Task<IActionResult> UpdateGrade(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade == null)
            {
                return NotFound();
            }
            return View(grade);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGrade(int id, Grade grade)
        {
            if (id != grade.GradeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(grade);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(grade);
        }
    }
}

