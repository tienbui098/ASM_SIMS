using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using SIMS_ASM.Services;
using SIMS_ASM.Singleton;

namespace SIMS_ASM.Controllers
{
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IUserService _userService;
        private readonly IClassCourseFacultyService _classCourseFacultyService;
        private readonly IStudentClassService _studentClassService;
        private readonly ApplicationDbContex _context;
        private readonly AccountSingleton _singleton;

        public EnrollmentController(
            IEnrollmentService enrollmentService,
            IUserService userService,
            IClassCourseFacultyService classCourseFacultyService,
            IStudentClassService studentClassService,
            ApplicationDbContex context)
        {
            _enrollmentService = enrollmentService;
            _userService = userService;
            _classCourseFacultyService = classCourseFacultyService;
            _studentClassService = studentClassService;
            _context = context;
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
                _singleton.Log("Unauthorized access to Enrollment Management: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var enrollments = await _enrollmentService.GetAllEnrollmentsAsync();
            return View(enrollments);
        }

        public async Task<IActionResult> CreateEnrollment()
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Create Enrollment: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Students = await _userService.GetStudentsAsync();
            ViewBag.ClassCourseFaculties = await _classCourseFacultyService.GetAllClassCourseFacultiesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEnrollment(Enrollment enrollment)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to create enrollment: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _enrollmentService.AddEnrollmentAsync(enrollment);
                    _singleton.Log($"Enrollment {enrollment.EnrollmentID} created by admin for student {enrollment.UserID}");
                    TempData["SuccessMessage"] = "Enrollment created successfully!";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    _singleton.Log($"Failed to create enrollment: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _singleton.Log($"Failed to create enrollment: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while creating the enrollment.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to create enrollment: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            ViewBag.Students = await _userService.GetStudentsAsync();
            ViewBag.ClassCourseFaculties = await _classCourseFacultyService.GetAllClassCourseFacultiesAsync();
            return View(enrollment);
        }

        public async Task<IActionResult> EditEnrollment(int id)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Edit Enrollment: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
            if (enrollment == null)
            {
                _singleton.Log($"Failed to edit enrollment with ID {id}: Enrollment not found");
                return NotFound();
            }

            ViewBag.Students = await _userService.GetStudentsAsync();
            ViewBag.ClassCourseFaculties = await _classCourseFacultyService.GetAllClassCourseFacultiesAsync();
            return View(enrollment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEnrollment(int id, Enrollment enrollment)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to edit enrollment: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            if (id != enrollment.EnrollmentID)
            {
                _singleton.Log($"Invalid enrollment edit attempt: ID mismatch for EnrollmentID {id}");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _enrollmentService.UpdateEnrollmentAsync(enrollment);
                    _singleton.Log($"Enrollment {enrollment.EnrollmentID} updated by admin");
                    TempData["SuccessMessage"] = "Enrollment updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    _singleton.Log($"Failed to update enrollment {enrollment.EnrollmentID}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _singleton.Log($"Failed to update enrollment {enrollment.EnrollmentID}: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the enrollment.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to update enrollment: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            ViewBag.Students = await _userService.GetStudentsAsync();
            ViewBag.ClassCourseFaculties = await _classCourseFacultyService.GetAllClassCourseFacultiesAsync();
            return View(enrollment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to delete enrollment: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var hasGrades = await _enrollmentService.HasAssociatedGradesAsync(id);
                if (hasGrades)
                {
                    _singleton.Log($"Failed to delete enrollment with ID {id}: Enrollment has associated grades.");
                    TempData["ErrorMessage"] = "Cannot delete enrollment because it has associated grades.";
                }
                else
                {
                    await _enrollmentService.DeleteEnrollmentAsync(id);
                    _singleton.Log($"Enrollment with ID {id} deleted by admin");
                    TempData["SuccessMessage"] = "Enrollment deleted successfully!";
                }
            }
            catch (Exception ex)
            {
                _singleton.Log($"Failed to delete enrollment with ID {id}: {ex.Message}");
                TempData["ErrorMessage"] = ("An error occurred while deleting the enrollment.");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetClassCourseFacultiesByStudent(int userId)
        {
            var studentClasses = await _studentClassService.GetClassIdsByStudentAsync(userId);
            if (!studentClasses.Any())
            {
                return Json(new { success = false, message = "Student has not been assigned to any class. Please assign the student to a class first.", redirectUrl = Url.Action("CreateStudentClass", "StudentClass") });
            }

            var classCourseFaculties = await _context.ClassCourseFaculties
                .Include(ccf => ccf.Class)
                .Include(ccf => ccf.Course)
                .Include(ccf => ccf.User)
                .Where(ccf => studentClasses.Contains(ccf.ClassID))
                .Select(ccf => new
                {
                    ccf.ClassCourseFacultyID,
                    DisplayText = $"{ccf.Class.ClassName} - {ccf.Course.CourseName} (Faculty: {ccf.User.FullName})"
                })
                .ToListAsync();

            return Json(new { success = true, data = classCourseFaculties });
        }
    }
}


