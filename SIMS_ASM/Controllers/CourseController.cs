using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using SIMS_ASM.Singleton;
using Microsoft.EntityFrameworkCore;


namespace SIMS_ASM.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApplicationDbContex _context;
        private readonly AccountSingleton _singleton;

        public CourseController(ApplicationDbContex context)
        {
            _context = context;
            _singleton = AccountSingleton.Instance;
        }

        // Kiểm tra quyền Admin
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin"; // Cập nhật vai trò từ "Administrator" thành "Admin"
        }

        // Hiển thị danh sách khóa học
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Course Management: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách khóa học và bao gồm thông tin về Major
            var courses = await _context.Courses
                .Include(c => c.Major)
                .ToListAsync();
            return View(courses);
        }

        // Thêm khóa học: Hiển thị form
        public async Task<IActionResult> CourseCreate()
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Create Course: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách Major để hiển thị trong dropdown
            ViewBag.Majors = await _context.Majors.ToListAsync();

            // Kiểm tra xem có Major nào không
            if (ViewBag.Majors == null || !((List<Major>)ViewBag.Majors).Any())
            {
                TempData["ErrorMessage"] = "No majors available. Please create a major first.";
                return RedirectToAction("Index");
            }

            return View();
        }

        // Phương thức POST để tạo khóa học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseCreate(Course course)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to create course: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra Major có tồn tại không
                    var major = await _context.Majors.FindAsync(course.MajorID);
                    if (major == null)
                    {
                        ModelState.AddModelError("MajorID", "Invalid major selected.");
                        _singleton.Log($"Failed to create course {course.CourseName}: Invalid Major ID {course.MajorID}");
                        ViewBag.Majors = await _context.Majors.ToListAsync();
                        return View(course);
                    }

                    _context.Courses.Add(course);
                    await _context.SaveChangesAsync();
                    _singleton.Log($"Course {course.CourseName} (ID: {course.CourseID}) created by admin, assigned to Major {major.MajorName}");
                    TempData["SuccessMessage"] = "Course created successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _singleton.Log($"Failed to create course {course.CourseName}: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while creating the course.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to create course: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            // Lấy lại danh sách Major để hiển thị trong view khi gặp lỗi
            ViewBag.Majors = await _context.Majors.ToListAsync();
            return View(course);
        }

        // Phương thức hiển thị trang chỉnh sửa khóa học
        public async Task<IActionResult> CourseEdit(int id)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Edit Course: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Tìm khóa học theo ID
            var course = await _context.Courses
                .Include(c => c.Major)
                .FirstOrDefaultAsync(c => c.CourseID == id);
            if (course == null)
            {
                _singleton.Log($"Failed to edit course with ID {id}: Course not found");
                return NotFound();
            }

            // Lấy danh sách Major để hiển thị trong dropdown
            ViewBag.Majors = await _context.Majors.ToListAsync();
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseEdit(int id, Course course)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to edit course: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            if (id != course.CourseID)
            {
                _singleton.Log($"Invalid course edit attempt: ID mismatch for CourseID {id}");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra Major có tồn tại không
                    var major = await _context.Majors.FindAsync(course.MajorID);
                    if (major == null)
                    {
                        ModelState.AddModelError("MajorID", "Invalid major selected.");
                        _singleton.Log($"Failed to update course {course.CourseName}: Invalid Major ID {course.MajorID}");
                        ViewBag.Majors = await _context.Majors.ToListAsync();
                        return View(course);
                    }

                    _context.Update(course);
                    await _context.SaveChangesAsync();
                    _singleton.Log($"Course {course.CourseName} (ID: {course.CourseID}) updated by admin, assigned to Major {major.MajorName}");
                    TempData["SuccessMessage"] = "Course updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _singleton.Log($"Failed to update course {course.CourseName}: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the course.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to update course: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            ViewBag.Majors = await _context.Majors.ToListAsync();
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseDelete(int id)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to delete course: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.Courses
                .Include(c => c.ClassCourseFaculties)
                .FirstOrDefaultAsync(c => c.CourseID == id);
            if (course == null)
            {
                _singleton.Log($"Failed to delete course with ID {id}: Course not found");
                return NotFound();
            }

            try
            {
                if (course.ClassCourseFaculties.Any())
                {
                    _singleton.Log($"Failed to delete course {course.CourseName} (ID: {id}): Course is associated with classes and faculty.");
                    TempData["ErrorMessage"] = "Cannot delete course because it is associated with classes and faculty.";
                    return RedirectToAction("Index");
                }

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                _singleton.Log($"Course {course.CourseName} (ID: {id}) deleted by admin");
                TempData["SuccessMessage"] = "Course deleted successfully!";
            }
            catch (Exception ex)
            {
                _singleton.Log($"Failed to delete course {course.CourseName} (ID: {id}): {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while deleting the course.";
            }

            return RedirectToAction("Index");
        }
    }
}