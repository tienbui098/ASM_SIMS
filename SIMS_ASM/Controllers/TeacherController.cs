using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Singleton;

namespace SIMS_ASM.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContex _context;
        private readonly AccountSingleton _singleton;

        public TeacherController(ApplicationDbContex context)
        {
            _context = context;
            _singleton = AccountSingleton.Instance;
        }

        // Trang chính cho giảng viên
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                _singleton.Log("Unauthorized access to Teacher dashboard: User not logged in");
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
                _singleton.Log($"Grade with ID {id} not found for update");
                return NotFound();
            }
            return View(grade);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGrade(int id, Grade grade)
        {
            if (id != grade.GradeID)
            {
                _singleton.Log($"Invalid grade update attempt: ID mismatch for GradeID {id}");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(grade);
                await _context.SaveChangesAsync();
                _singleton.Log($"Grade updated for GradeID {grade.GradeID}, new value: {grade.GradeValue}");
                return RedirectToAction("Index");
            }
            _singleton.Log($"Failed to update grade for GradeID {grade.GradeID}: Invalid model state");
            return View(grade);
        }
    }
}

