using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using SIMS_ASM.Singleton;
using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Services;


namespace SIMS_ASM.Controllers
{
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IMajorService _majorService; // Thêm IMajorService
        private readonly AccountSingleton _singleton;

        public CourseController(ICourseService courseService, IMajorService majorService)
        {
            _courseService = courseService;
            _majorService = majorService; // Inject IMajorService
            _singleton = AccountSingleton.Instance;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Course Management: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var courses = await _courseService.GetAllCoursesAsync();
            return View(courses);
        }

        public async Task<IActionResult> CourseCreate()
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Create Course: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Majors = await _majorService.GetAllMajorsAsync(); // Dùng IMajorService

            if (ViewBag.Majors == null || !((IEnumerable<Major>)ViewBag.Majors).Any())
            {
                TempData["ErrorMessage"] = "No majors available. Please create a major first.";
                return RedirectToAction("Index");
            }

            return View();
        }

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
                    var createdCourse = await _courseService.CreateCourseAsync(course);
                    _singleton.Log($"Course {createdCourse.CourseName} (ID: {createdCourse.CourseID}) created by admin");
                    TempData["SuccessMessage"] = "Course created successfully!";
                    return RedirectToAction("Index");
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("MajorID", ex.Message);
                    _singleton.Log($"Failed to create course {course.CourseName}: {ex.Message}");
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

            ViewBag.Majors = await _majorService.GetAllMajorsAsync(); // Dùng IMajorService
            return View(course);
        }

        public async Task<IActionResult> CourseEdit(int id)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Edit Course: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                _singleton.Log($"Failed to edit course with ID {id}: Course not found");
                return NotFound();
            }

            ViewBag.Majors = await _majorService.GetAllMajorsAsync(); // Dùng IMajorService
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
                    await _courseService.UpdateCourseAsync(course);
                    _singleton.Log($"Course {course.CourseName} (ID: {course.CourseID}) updated by admin");
                    TempData["SuccessMessage"] = "Course updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("MajorID", ex.Message);
                    _singleton.Log($"Failed to update course {course.CourseName}: {ex.Message}");
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

            ViewBag.Majors = await _majorService.GetAllMajorsAsync(); // Dùng IMajorService
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

            var success = await _courseService.DeleteCourseAsync(id);
            if (!success)
            {
                var hasClasses = await _courseService.HasAssociatedClassesAsync(id);
                if (hasClasses)
                {
                    _singleton.Log($"Failed to delete course with ID {id}: Course is associated with classes and faculty.");
                    TempData["ErrorMessage"] = "Cannot delete course because it is associated with classes and faculty.";
                }
                else
                {
                    _singleton.Log($"Failed to delete course with ID {id}: Course not found");
                    TempData["ErrorMessage"] = "Course not found.";
                }
            }
            else
            {
                _singleton.Log($"Course with ID {id} deleted by admin");
                TempData["SuccessMessage"] = "Course deleted successfully!";
            }

            return RedirectToAction("Index");
        }
    }
}