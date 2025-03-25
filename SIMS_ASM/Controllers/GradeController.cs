using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using SIMS_ASM.Singleton;
using Microsoft.EntityFrameworkCore;


namespace SIMS_ASM.Controllers
{
    public class GradeController : Controller
    {
        private readonly ApplicationDbContex _context;
        private readonly AccountSingleton _singleton;

        public GradeController(ApplicationDbContex context)
        {
            _context = context;
            _singleton = AccountSingleton.Instance;
        }

        // Kiểm tra vai trò người dùng
        private bool IsAuthorized()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Administrator" || role == "Lecturer";
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Administrator";
        }

        // Hiển thị danh sách điểm
        public async Task<IActionResult> Index()
        {
            if (!IsAuthorized())
            {
                _singleton.Log("Unauthorized access to Grade Management: User not authorized");
                return RedirectToAction("Login", "Account");
            }

            var grades = await _context.Grades
                .Include(g => g.User)
                .Include(g => g.Course)
                .ToListAsync();

            // Nếu là giảng viên, chỉ hiển thị điểm của các khóa học mà họ phụ trách
            if (HttpContext.Session.GetString("Role") == "Lecturer")
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                grades = grades.Where(g => g.Course.UserID == userId).ToList();
            }

            return View(grades);
        }

        // Thêm điểm: Hiển thị form
        public async Task<IActionResult> CreateGrade()
        {
            if (!IsAuthorized())
            {
                _singleton.Log("Unauthorized access to Create Grade: User not authorized");
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Students = await _context.Users
                .Where(u => u.Role == "Student")
                .ToListAsync();

            if (HttpContext.Session.GetString("Role") == "Lecturer")
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                ViewBag.Courses = await _context.Courses
                    .Where(c => c.UserID == userId)
                    .ToListAsync();
            }
            else
            {
                ViewBag.Courses = await _context.Courses.ToListAsync();
            }

            return View();
        }

        // Thêm điểm: Xử lý form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGrade(Grade grade)
        {
            if (!IsAuthorized())
            {
                _singleton.Log("Unauthorized attempt to create grade: User not authorized");
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Grades.Add(grade);
                    await _context.SaveChangesAsync();
                    _singleton.Log($"Grade for student ID {grade.UserID} in course ID {grade.CourseID} created with value {grade.GradeValue}");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _singleton.Log($"Failed to create grade: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while creating the grade.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to create grade: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            ViewBag.Students = await _context.Users
                .Where(u => u.Role == "Student")
                .ToListAsync();

            if (HttpContext.Session.GetString("Role") == "Lecturer")
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                ViewBag.Courses = await _context.Courses
                    .Where(c => c.UserID == userId)
                    .ToListAsync();
            }
            else
            {
                ViewBag.Courses = await _context.Courses.ToListAsync();
            }

            return View(grade);
        }

        // Sửa điểm: Hiển thị form
        public async Task<IActionResult> EditGrade(int id)
        {
            if (!IsAuthorized())
            {
                _singleton.Log("Unauthorized access to Edit Grade: User not authorized");
                return RedirectToAction("Login", "Account");
            }

            var grade = await _context.Grades.FindAsync(id);
            if (grade == null)
            {
                _singleton.Log($"Failed to edit grade with ID {id}: Grade not found");
                return NotFound();
            }

            // Kiểm tra quyền của giảng viên
            if (HttpContext.Session.GetString("Role") == "Lecturer")
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                var course = await _context.Courses.FindAsync(grade.CourseID);
                if (course.UserID != userId)
                {
                    _singleton.Log($"Unauthorized attempt to edit grade with ID {id}: Lecturer not assigned to this course");
                    return RedirectToAction("Index");
                }
            }

            ViewBag.Students = await _context.Users
                .Where(u => u.Role == "Student")
                .ToListAsync();

            if (HttpContext.Session.GetString("Role") == "Lecturer")
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                ViewBag.Courses = await _context.Courses
                    .Where(c => c.UserID == userId)
                    .ToListAsync();
            }
            else
            {
                ViewBag.Courses = await _context.Courses.ToListAsync();
            }

            return View(grade);
        }

        // Sửa điểm: Xử lý form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGrade(int id, Grade grade)
        {
            if (!IsAuthorized())
            {
                _singleton.Log("Unauthorized attempt to edit grade: User not authorized");
                return RedirectToAction("Login", "Account");
            }

            if (id != grade.GradeID)
            {
                _singleton.Log($"Invalid grade edit attempt: ID mismatch for GradeID {id}");
                return NotFound();
            }

            // Kiểm tra quyền của giảng viên
            if (HttpContext.Session.GetString("Role") == "Lecturer")
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                var course = await _context.Courses.FindAsync(grade.CourseID);
                if (course.UserID != userId)
                {
                    _singleton.Log($"Unauthorized attempt to edit grade with ID {id}: Lecturer not assigned to this course");
                    return RedirectToAction("Index");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grade);
                    await _context.SaveChangesAsync();
                    _singleton.Log($"Grade for student ID {grade.UserID} in course ID {grade.CourseID} updated to {grade.GradeValue}");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _singleton.Log($"Failed to update grade: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the grade.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to update grade: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            ViewBag.Students = await _context.Users
                .Where(u => u.Role == "Student")
                .ToListAsync();

            if (HttpContext.Session.GetString("Role") == "Lecturer")
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                ViewBag.Courses = await _context.Courses
                    .Where(c => c.UserID == userId)
                    .ToListAsync();
            }
            else
            {
                ViewBag.Courses = await _context.Courses.ToListAsync();
            }

            return View(grade);
        }

        // Xóa điểm (chỉ Admin được phép)
        [HttpPost]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to delete grade: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var grade = await _context.Grades.FindAsync(id);
            if (grade == null)
            {
                _singleton.Log($"Failed to delete grade with ID {id}: Grade not found");
                return NotFound();
            }

            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();
            _singleton.Log($"Grade with ID {id} deleted by admin");
            return RedirectToAction("Index");
        }
    }
}
