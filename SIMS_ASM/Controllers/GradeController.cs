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
        // Khai báo các dịch vụ cần thiết
        private readonly IGradeService _gradeService; 
        private readonly IEnrollmentService _enrollmentService; 
        private readonly IStudentClassService _studentClassService; 
        private readonly ClassService _classService; 
        private readonly AccountSingleton _singleton; 
        private readonly ICourseService _courseService; 

        // Constructor: Inject các dịch vụ (đã giải thích ở các controller trước)
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

        // Phương thức kiểm tra quyền Admin (đã giải thích trước)
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // Phương thức kiểm tra quyền Lecturer
        private bool IsLecturer()
        {
            return HttpContext.Session.GetString("Role") == "Lecturer";
        }

        // Hành động Index: Hiển thị danh sách tất cả điểm số
        public async Task<IActionResult> Index()
        {
            // Kiểm tra quyền truy cập: Chỉ Admin hoặc Lecturer được phép
            if (!IsAdmin() && !IsLecturer())
            {
                _singleton.Log("Unauthorized access to Grade Management");
                return RedirectToAction("Login", "Account");
            }

            // Lấy tất cả điểm số từ dịch vụ
            var grades = await _gradeService.GetAllGradesAsync();

            // Chuẩn bị dữ liệu cho filter (dropdown)
            ViewBag.Classes = _classService.GetAllClasses();
            ViewBag.Courses = await _courseService.GetAllCoursesAsync();

            return View(grades); // Trả về view với danh sách điểm số
        }

        // Hành động CreateGrade: Hiển thị form tạo điểm số mới
        public async Task<IActionResult> CreateGrade()
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin() && !IsLecturer())
            {
                _singleton.Log("Unauthorized access to Create Grade");
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách lớp học cho dropdown
            var classes = _classService.GetAllClasses();
            ViewBag.Classes = new SelectList(classes, "ClassID", "ClassName");

            // Khởi tạo danh sách enrollments rỗng (sẽ được cập nhật qua AJAX)
            ViewBag.Enrollments = new List<SelectListItem>();

            return View(); // Trả về form tạo mới
        }

        // Hành động POST CreateGrade: Xử lý tạo điểm số mới
        [HttpPost]
        [ValidateAntiForgeryToken] // Ngăn chặn tấn công CSRF (đã giải thích trước)
        public async Task<IActionResult> CreateGrade(Grade grade)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin() && !IsLecturer())
            {
                _singleton.Log("Unauthorized attempt to create grade");
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra dữ liệu đầu vào (logic tương tự các POST khác)
            if (!ModelState.IsValid)
            {
                try
                {
                    // Thêm điểm số mới
                    await _gradeService.AddGradeAsync(grade);
                    _singleton.Log($"Grade created for enrollment {grade.EnrollmentID}");
                    TempData["SuccessMessage"] = "Grade created successfully!";

                    // Điều hướng theo vai trò
                    if (IsAdmin())
                    {
                        return RedirectToAction("Index"); // Admin về danh sách điểm
                    }
                    else if (IsLecturer())
                    {
                        return RedirectToAction("ViewGrade", "Lecturer"); // Lecturer về trang xem điểm
                    }
                }
                catch (Exception ex) // Xử lý lỗi chung (đã giải thích trước)
                {
                    ModelState.AddModelError("", "Error creating grade: " + ex.Message);
                    _singleton.Log($"Error creating grade: {ex.Message}");
                }
            }

            // Tải lại dữ liệu nếu có lỗi
            var classes = _classService.GetAllClasses();
            ViewBag.Classes = new SelectList(classes, "ClassID", "ClassName");
            ViewBag.Enrollments = new List<SelectListItem>(); // Reset danh sách enrollments
            return View(grade);
        }

        // API GetEnrollmentsByClass: Trả về danh sách enrollments theo lớp (AJAX)
        [HttpGet]
        public async Task<JsonResult> GetEnrollmentsByClass(int classId)
        {
            var enrollments = await GetEnrollmentsByClassAsync(classId); // Gọi phương thức hỗ trợ
            return Json(enrollments); // Trả về JSON
        }

        // Phương thức hỗ trợ: Lấy danh sách enrollments theo classId
        private async Task<List<SelectListItem>> GetEnrollmentsByClassAsync(int classId)
        {
            var enrollments = await _enrollmentService.GetAllEnrollmentsAsync();

            // Lọc và tạo danh sách SelectListItem cho dropdown
            var filteredEnrollments = enrollments
                .Where(e => e.ClassCourseFaculty.ClassID == classId) // Lọc theo lớp
                .Select(e => new SelectListItem
                {
                    Value = e.EnrollmentID.ToString(),
                    Text = $"{e.User.FullName} - {e.ClassCourseFaculty.Course.CourseName}" // Chuỗi hiển thị
                })
                .ToList();

            return filteredEnrollments;
        }

        // Hành động EditGrade: Hiển thị form chỉnh sửa điểm số
        public async Task<IActionResult> EditGrade(int id)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin() && !IsLecturer())
            {
                _singleton.Log("Unauthorized access to Edit Grade");
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin điểm số theo ID (logic tương tự các Edit khác)
            var grade = await _gradeService.GetGradeByIdAsync(id);
            if (grade == null)
            {
                return NotFound();
            }

            // Chuẩn bị dữ liệu cho form
            var classes = _classService.GetAllClasses();
            ViewBag.Classes = new SelectList(classes, "ClassID", "ClassName");

            // Lấy danh sách enrollments cho lớp của điểm số hiện tại
            var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(grade.EnrollmentID);
            var enrollments = await GetEnrollmentsByClassAsync(enrollment.ClassCourseFaculty.ClassID);
            ViewBag.Enrollments = new SelectList(enrollments, "Value", "Text", grade.EnrollmentID);

            return View(grade);
        }

        // Hành động POST EditGrade: Xử lý cập nhật điểm số
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGrade(int id, Grade grade)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin() && !IsLecturer())
            {
                _singleton.Log("Unauthorized attempt to edit grade");
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra ID có khớp không (logic tương tự các Edit khác)
            if (id != grade.GradeID)
            {
                return NotFound();
            }

            // Kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                try
                {
                    // Cập nhật điểm số
                    await _gradeService.UpdateGradeAsync(grade);
                    _singleton.Log($"Grade {grade.GradeID} updated");
                    TempData["SuccessMessage"] = "Grade updated successfully!";

                    // Điều hướng theo vai trò
                    if (IsAdmin())
                    {
                        return RedirectToAction("Index");
                    }
                    else if (IsLecturer())
                    {
                        return RedirectToAction("ViewGrade", "Lecturer");
                    }
                }
                catch (Exception ex) // Xử lý lỗi chung
                {
                    _singleton.Log($"Error updating grade {grade.GradeID}: {ex.Message}");
                    ModelState.AddModelError("", "Error updating grade: " + ex.Message);
                }
            }

            // Tải lại dữ liệu nếu có lỗi
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

        // Hành động DeleteGrade: Hiển thị xác nhận xóa điểm số
        public async Task<IActionResult> DeleteGrade(int? id)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin() && !IsLecturer())
            {
                _singleton.Log("Unauthorized access to Delete Grade");
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra ID hợp lệ
            if (id == null)
            {
                return NotFound();
            }

            // Lấy thông tin điểm số
            var grade = await _gradeService.GetGradeByIdAsync(id.Value);
            if (grade == null)
            {
                return NotFound();
            }

            return View(grade); // Trả về view xác nhận xóa
        }

        // Hành động POST DeleteConfirmed: Xử lý xóa điểm số
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin() && !IsLecturer())
            {
                _singleton.Log("Unauthorized attempt to delete grade");
                return RedirectToAction("Login", "Account");
            }

            // Xóa điểm số (logic tương tự các Delete khác)
            await _gradeService.DeleteGradeAsync(id);
            _singleton.Log($"Grade with ID {id} deleted");
            TempData["SuccessMessage"] = "Grade deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}