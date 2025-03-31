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

            ViewBag.Classes = await _context.Classes.ToListAsync();
            ViewBag.ClassCourseFaculties = await _classCourseFacultyService.GetAllClassCourseFacultiesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEnrollment(int classId, int classCourseFacultyId)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to create enrollment: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Lấy danh sách sinh viên trong lớp
                var studentsInClass = await _studentClassService.GetStudentsInClassAsync(classId);

                if (!studentsInClass.Any())
                {
                    TempData["ErrorMessage"] = "No students found in the selected class";
                    return RedirectToAction("CreateEnrollment");
                }

                // Lấy thông tin ClassCourseFaculty để kiểm tra
                var classCourseFaculty = await _classCourseFacultyService.GetClassCourseFacultyByIdAsync(classCourseFacultyId);
                if (classCourseFaculty == null)
                {
                    TempData["ErrorMessage"] = "Selected course not found";
                    return RedirectToAction("CreateEnrollment");
                }

                // Kiểm tra xem ClassCourseFaculty có cùng ClassID với lớp được chọn không
                if (classCourseFaculty.ClassID != classId)
                {
                    TempData["ErrorMessage"] = "Selected course does not belong to the selected class";
                    return RedirectToAction("CreateEnrollment");
                }

                int enrollmentCount = 0;
                foreach (var student in studentsInClass)
                {
                    // Kiểm tra xem sinh viên đã được ghi danh chưa
                    if (!await _enrollmentService.IsStudentAlreadyEnrolledAsync(student.UserID, classCourseFacultyId))
                    {
                        var enrollment = new Enrollment
                        {
                            UserID = student.UserID,
                            ClassCourseFacultyID = classCourseFacultyId,
                            EnrollmentDate = DateTime.Now
                        };
                        await _enrollmentService.AddEnrollmentAsync(enrollment);
                        enrollmentCount++;
                    }
                }

                if (enrollmentCount > 0)
                {
                    TempData["SuccessMessage"] = $"Successfully enrolled {enrollmentCount} students to the course";
                }
                else
                {
                    TempData["InfoMessage"] = "All students in the class are already enrolled in this course";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _singleton.Log($"Failed to create enrollments: {ex.Message}");
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("CreateEnrollment");
            }
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

            if (!ModelState.IsValid)
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
        public async Task<IActionResult> GetCoursesByClass(int classId)
        {
            var courses = await _context.ClassCourseFaculties
                .Include(ccf => ccf.Course)
                .Include(ccf => ccf.User)
                .Where(ccf => ccf.ClassID == classId)
                .Select(ccf => new
                {
                    ccf.ClassCourseFacultyID,
                    DisplayText = $"{ccf.Course.CourseName} (Faculty: {ccf.User.FullName})"
                })
                .ToListAsync();

            return Json(new { success = true, data = courses });
        }
    }
}


