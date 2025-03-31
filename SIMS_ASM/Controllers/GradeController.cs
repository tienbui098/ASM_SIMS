using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using SIMS_ASM.Singleton;
using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.Entity;


namespace SIMS_ASM.Controllers
{
    public class GradeController : Controller
    {
        private readonly IGradeService _gradeService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IStudentClassService _studentClassService;
        private readonly ClassService _classService;
        private readonly AccountSingleton _singleton;
        private readonly ICourseService _courseService;

        public GradeController(
            IGradeService gradeService,
            IEnrollmentService enrollmentService,
            IStudentClassService studentClassService,
            ClassService classService,
            ICourseService courseService)
        {
            _gradeService = gradeService;
            _enrollmentService = enrollmentService;
            _studentClassService = studentClassService;
            _classService = classService;
            _courseService = courseService;
            _singleton = AccountSingleton.Instance;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        private bool IsLecturer()
        {
            return HttpContext.Session.GetString("Role") == "Lecturer";
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin() && !IsLecturer())
            {
                _singleton.Log("Unauthorized access to Grade Management");
                return RedirectToAction("Login", "Account");
            }

            var grades = await _gradeService.GetAllGradesAsync();

            // Thêm dữ liệu filter
            ViewBag.Classes = _classService.GetAllClasses();
            ViewBag.Courses = await _courseService.GetAllCoursesAsync();

            return View(grades);
        }

        public async Task<IActionResult> CreateGrade()
        {
            if (!IsAdmin() && !IsLecturer())
            {
                _singleton.Log("Unauthorized access to Create Grade");
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách lớp học
            var classes = _classService.GetAllClasses();
            ViewBag.Classes = new SelectList(classes, "ClassID", "ClassName");

            // Khởi tạo danh sách enrollment rỗng
            ViewBag.Enrollments = new List<SelectListItem>();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGrade(Grade grade)
        {
            if (!IsAdmin() && !IsLecturer())
            {
                _singleton.Log("Unauthorized attempt to create grade");
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    await _gradeService.AddGradeAsync(grade);
                    _singleton.Log($"Grade created for enrollment {grade.EnrollmentID}");
                    TempData["SuccessMessage"] = "Grade created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating grade: " + ex.Message);
                    _singleton.Log($"Error creating grade: {ex.Message}");
                }
            }

            // Nếu có lỗi, cần load lại dropdown
            var classes = _classService.GetAllClasses();
            ViewBag.Classes = new SelectList(classes, "ClassID", "ClassName");

            if (grade.EnrollmentID > 0)
            {
                var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(grade.EnrollmentID);
                if (enrollment != null)
                {
                    var enrollments = await GetEnrollmentsByClassAsync(enrollment.ClassCourseFaculty.ClassID);
                    ViewBag.Enrollments = enrollments;
                }
            }
            else
            {
                ViewBag.Enrollments = new List<SelectListItem>();
            }

            return View(grade);
        }

        [HttpGet]
        public async Task<JsonResult> GetEnrollmentsByClass(int classId)
        {
            var enrollments = await GetEnrollmentsByClassAsync(classId);
            return Json(enrollments);
        }

        private async Task<List<SelectListItem>> GetEnrollmentsByClassAsync(int classId)
        {
            var enrollments = await _enrollmentService.GetAllEnrollmentsAsync();

            // Lọc enrollments theo classId
            var filteredEnrollments = enrollments
                .Where(e => e.ClassCourseFaculty.ClassID == classId)
                .Select(e => new SelectListItem
                {
                    Value = e.EnrollmentID.ToString(),
                    Text = $"{e.User.FullName} - {e.ClassCourseFaculty.Course.CourseName}"
                })
                .ToList();

            return filteredEnrollments;
        }

        public async Task<IActionResult> EditGrade(int id)
        {
            if (!IsAdmin() && !IsLecturer())
            {
                _singleton.Log("Unauthorized access to Edit Grade");
                return RedirectToAction("Login", "Account");
            }

            var grade = await _gradeService.GetGradeByIdAsync(id);
            if (grade == null)
            {
                return NotFound();
            }

            // Lấy danh sách lớp học
            var classes = _classService.GetAllClasses();
            ViewBag.Classes = new SelectList(classes, "ClassID", "ClassName");

            // Lấy danh sách enrollments cho lớp học của grade hiện tại
            var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(grade.EnrollmentID);
            var enrollments = await GetEnrollmentsByClassAsync(enrollment.ClassCourseFaculty.ClassID);
            ViewBag.Enrollments = new SelectList(enrollments, "Value", "Text", grade.EnrollmentID);

            return View(grade);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGrade(int id, Grade grade)
        {
            if (!IsAdmin() && !IsLecturer())
            {
                _singleton.Log("Unauthorized attempt to edit grade");
                return RedirectToAction("Login", "Account");
            }

            if (id != grade.GradeID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    await _gradeService.UpdateGradeAsync(grade);
                    _singleton.Log($"Grade {grade.GradeID} updated");
                    TempData["SuccessMessage"] = "Grade updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _singleton.Log($"Error updating grade {grade.GradeID}: {ex.Message}");
                    ModelState.AddModelError("", "Error updating grade: " + ex.Message);
                }
            }

            // Nếu có lỗi, cần load lại dropdown
            var classes = _classService.GetAllClasses();
            ViewBag.Classes = new SelectList(classes, "ClassID", "ClassName");

            if (grade.EnrollmentID > 0)
            {
                var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(grade.EnrollmentID);
                if (enrollment != null)
                {
                    var enrollments = await GetEnrollmentsByClassAsync(enrollment.ClassCourseFaculty.ClassID);
                    ViewBag.Enrollments = new SelectList(enrollments, "Value", "Text", grade.EnrollmentID);
                }
            }

            return View(grade);
        }

        public async Task<IActionResult> DeleteGrade(int? id)
        {
            if (!IsAdmin() && !IsLecturer())
            {
                _singleton.Log("Unauthorized access to Delete Grade");
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var grade = await _gradeService.GetGradeByIdAsync(id.Value);
            if (grade == null)
            {
                return NotFound();
            }

            return View(grade);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin() && !IsLecturer())
            {
                _singleton.Log("Unauthorized attempt to delete grade");
                return RedirectToAction("Login", "Account");
            }

            await _gradeService.DeleteGradeAsync(id);
            _singleton.Log($"Grade with ID {id} deleted");
            TempData["SuccessMessage"] = "Grade deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
