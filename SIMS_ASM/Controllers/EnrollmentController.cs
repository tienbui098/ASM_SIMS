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
        // Khai báo các dịch vụ và context cần thiết
        private readonly IEnrollmentService _enrollmentService; 
        private readonly IUserService _userService; 
        private readonly IClassCourseFacultyService _classCourseFacultyService;
        private readonly IStudentClassService _studentClassService; 
        private readonly ApplicationDbContex _context; 
        private readonly AccountSingleton _singleton; 

        // Constructor: Inject các dịch vụ và context (đã giải thích ở các controller trước)
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

        // Phương thức kiểm tra quyền Admin (đã giải thích trước)
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // Hành động Index: Hiển thị danh sách tất cả ghi danh
        public async Task<IActionResult> Index()
        {
            // Kiểm tra quyền truy cập (logic tương tự các controller khác)
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Enrollment Management: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách ghi danh từ dịch vụ
            var enrollments = await _enrollmentService.GetAllEnrollmentsAsync();
            return View(enrollments); // Trả về view với danh sách ghi danh
        }

        // Hành động CreateEnrollment: Hiển thị form tạo ghi danh mới
        public async Task<IActionResult> CreateEnrollment()
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Create Enrollment: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Chuẩn bị dữ liệu cho form (dropdown)
            ViewBag.Classes = await _context.Classes.ToListAsync(); // Danh sách lớp học
            ViewBag.ClassCourseFaculties = await _classCourseFacultyService.GetAllClassCourseFacultiesAsync(); // Danh sách ClassCourseFaculty
            return View(); // Trả về form tạo mới
        }

        // Hành động POST CreateEnrollment: Xử lý tạo ghi danh cho sinh viên trong lớp
        [HttpPost]
        [ValidateAntiForgeryToken] // Ngăn chặn tấn công CSRF (đã giải thích trước)
        public async Task<IActionResult> CreateEnrollment(int classId, int classCourseFacultyId)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to create enrollment: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Lấy danh sách sinh viên trong lớp được chọn
                var studentsInClass = await _studentClassService.GetStudentsInClassAsync(classId);
                if (!studentsInClass.Any())
                {
                    TempData["ErrorMessage"] = "No students found in the selected class";
                    return RedirectToAction("CreateEnrollment"); // Quay lại nếu không có sinh viên
                }

                // Kiểm tra ClassCourseFaculty tồn tại
                var classCourseFaculty = await _classCourseFacultyService.GetClassCourseFacultyByIdAsync(classCourseFacultyId);
                if (classCourseFaculty == null)
                {
                    TempData["ErrorMessage"] = "Selected course not found";
                    return RedirectToAction("CreateEnrollment");
                }

                // Kiểm tra tính hợp lệ: ClassCourseFaculty phải thuộc lớp đã chọn
                if (classCourseFaculty.ClassID != classId)
                {
                    TempData["ErrorMessage"] = "Selected course does not belong to the selected class";
                    return RedirectToAction("CreateEnrollment");
                }

                int enrollmentCount = 0; // Đếm số sinh viên được ghi danh
                foreach (var student in studentsInClass)
                {
                    // Kiểm tra xem sinh viên đã ghi danh chưa
                    if (!await _enrollmentService.IsStudentAlreadyEnrolledAsync(student.UserID, classCourseFacultyId))
                    {
                        var enrollment = new Enrollment
                        {
                            UserID = student.UserID,
                            ClassCourseFacultyID = classCourseFacultyId,
                            EnrollmentDate = DateTime.Now // Gán ngày ghi danh hiện tại
                        };
                        await _enrollmentService.AddEnrollmentAsync(enrollment); // Thêm ghi danh
                        enrollmentCount++;
                    }
                }

                // Thông báo kết quả
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
            catch (Exception ex) // Xử lý lỗi chung (đã giải thích trước)
            {
                _singleton.Log($"Failed to create enrollments: {ex.Message}");
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("CreateEnrollment");
            }
        }

        // Hành động EditEnrollment: Hiển thị form chỉnh sửa ghi danh
        public async Task<IActionResult> EditEnrollment(int id)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Edit Enrollment: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin ghi danh theo ID (logic tương tự các Edit khác)
            var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
            if (enrollment == null)
            {
                _singleton.Log($"Failed to edit enrollment with ID {id}: Enrollment not found");
                return NotFound();
            }

            // Chuẩn bị dữ liệu cho form
            ViewBag.Students = await _userService.GetStudentsAsync(); // Danh sách sinh viên
            ViewBag.ClassCourseFaculties = await _classCourseFacultyService.GetAllClassCourseFacultiesAsync(); // Danh sách ClassCourseFaculty
            return View(enrollment);
        }

        // Hành động POST EditEnrollment: Xử lý cập nhật ghi danh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEnrollment(int id, Enrollment enrollment)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to edit enrollment: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra ID có khớp không (logic tương tự các Edit khác)
            if (id != enrollment.EnrollmentID)
            {
                _singleton.Log($"Invalid enrollment edit attempt: ID mismatch for EnrollmentID {id}");
                return NotFound();
            }

            // Kiểm tra dữ liệu đầu vào (logic tương tự các POST khác)
            if (!ModelState.IsValid)
            {
                try
                {
                    // Cập nhật ghi danh
                    await _enrollmentService.UpdateEnrollmentAsync(enrollment);
                    _singleton.Log($"Enrollment {enrollment.EnrollmentID} updated by admin");
                    TempData["SuccessMessage"] = "Enrollment updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex) // Lỗi thao tác không hợp lệ
                {
                    ModelState.AddModelError("", ex.Message);
                    _singleton.Log($"Failed to update enrollment {enrollment.EnrollmentID}: {ex.Message}");
                }
                catch (Exception ex) // Lỗi chung
                {
                    _singleton.Log($"Failed to update enrollment {enrollment.EnrollmentID}: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the enrollment.");
                }
            }
            else
            {
                // Ghi log lỗi ModelState (tương tự các controller khác)
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to update enrollment: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            // Tải lại dữ liệu nếu có lỗi
            ViewBag.Students = await _userService.GetStudentsAsync();
            ViewBag.ClassCourseFaculties = await _classCourseFacultyService.GetAllClassCourseFacultiesAsync();
            return View(enrollment);
        }

        // Hành động POST DeleteEnrollment: Xử lý xóa ghi danh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to delete enrollment: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Kiểm tra xem ghi danh có điểm số liên quan không
                var hasGrades = await _enrollmentService.HasAssociatedGradesAsync(id);
                if (hasGrades)
                {
                    _singleton.Log($"Failed to delete enrollment with ID {id}: Enrollment has associated grades.");
                    TempData["ErrorMessage"] = "Cannot delete enrollment because it has associated grades.";
                }
                else
                {
                    // Xóa ghi danh nếu không có điểm số
                    await _enrollmentService.DeleteEnrollmentAsync(id);
                    _singleton.Log($"Enrollment with ID {id} deleted by admin");
                    TempData["SuccessMessage"] = "Enrollment deleted successfully!";
                }
            }
            catch (Exception ex) // Xử lý lỗi chung
            {
                _singleton.Log($"Failed to delete enrollment with ID {id}: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while deleting the enrollment.";
            }

            return RedirectToAction("Index");
        }

        // Hành động GetCoursesByClass: Trả về danh sách khóa học theo lớp (API)
        [HttpGet]
        public async Task<IActionResult> GetCoursesByClass(int classId)
        {
            // Truy vấn danh sách ClassCourseFaculty theo classId, bao gồm thông tin khóa học và giảng viên
            var courses = await _context.ClassCourseFaculties
                .Include(ccf => ccf.Course) // Nạp thông tin khóa học
                .Include(ccf => ccf.User) // Nạp thông tin giảng viên
                .Where(ccf => ccf.ClassID == classId) // Lọc theo classId
                .Select(ccf => new // Tạo đối tượng JSON
                {
                    ccf.ClassCourseFacultyID,
                    DisplayText = $"{ccf.Course.CourseName} (Faculty: {ccf.User.FullName})" // Chuỗi hiển thị
                })
                .ToListAsync();

            return Json(new { success = true, data = courses }); // Trả về JSON
        }
    }
}