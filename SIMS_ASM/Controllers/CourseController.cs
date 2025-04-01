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
        // Khai báo các dịch vụ cần thiết
        private readonly ICourseService _courseService; 
        private readonly IMajorService _majorService; 
        private readonly AccountSingleton _singleton; 

        // Constructor: Inject các dịch vụ (đã giải thích ở các controller trước)
        public CourseController(ICourseService courseService, IMajorService majorService)
        {
            _courseService = courseService;
            _majorService = majorService; // Inject IMajorService
            _singleton = AccountSingleton.Instance;
        }

        // Phương thức kiểm tra quyền Admin (đã giải thích trước)
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // Hành động Index: Hiển thị danh sách khóa học với khả năng lọc theo majorId
        public async Task<IActionResult> Index(int? majorId)
        {
            // Kiểm tra quyền truy cập (logic tương tự các controller khác)
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Course Management: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Lấy tất cả khóa học từ dịch vụ
            var courses = await _courseService.GetAllCoursesAsync();

            // Lọc theo majorId nếu có (logic lọc tương tự ClassController)
            if (majorId.HasValue && majorId.Value > 0)
            {
                courses = courses.Where(c => c.MajorID == majorId.Value).ToList();
            }

            // Chuẩn bị dữ liệu cho dropdown lọc
            ViewBag.Majors = await _majorService.GetAllMajorsAsync();
            ViewBag.SelectedMajor = majorId; // Giá trị chuyên ngành đã chọn

            return View(courses); // Trả về view với danh sách khóa học
        }

        // Hành động CourseCreate: Hiển thị form tạo khóa học mới
        public async Task<IActionResult> CourseCreate()
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Create Course: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách chuyên ngành cho dropdown
            ViewBag.Majors = await _majorService.GetAllMajorsAsync();

            // Kiểm tra xem có chuyên ngành nào không
            if (ViewBag.Majors == null || !((IEnumerable<Major>)ViewBag.Majors).Any())
            {
                TempData["ErrorMessage"] = "No majors available. Please create a major first.";
                return RedirectToAction("Index"); // Quay lại nếu không có chuyên ngành
            }

            return View(); // Trả về form tạo mới
        }

        // Hành động POST CourseCreate: Xử lý tạo khóa học mới
        [HttpPost]
        [ValidateAntiForgeryToken] // Ngăn chặn tấn công CSRF (đã giải thích trước)
        public async Task<IActionResult> CourseCreate(Course course)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to create course: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra dữ liệu đầu vào (logic tương tự các POST khác)
            if (!ModelState.IsValid)
            {
                try
                {
                    // Tạo khóa học mới
                    var createdCourse = await _courseService.CreateCourseAsync(course);
                    _singleton.Log($"Course {createdCourse.CourseName} (ID: {createdCourse.CourseID}) created by admin");
                    TempData["SuccessMessage"] = "Course created successfully!";
                    return RedirectToAction("Index");
                }
                catch (ArgumentException ex) // Lỗi liên quan đến tham số (ví dụ: MajorID không hợp lệ)
                {
                    ModelState.AddModelError("MajorID", ex.Message);
                    _singleton.Log($"Failed to create course {course.CourseName}: {ex.Message}");
                }
                catch (Exception ex) // Lỗi chung (đã giải thích trước)
                {
                    _singleton.Log($"Failed to create course {course.CourseName}: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while creating the course.");
                }
            }
            else
            {
                // Ghi log lỗi nếu ModelState không hợp lệ (tương tự các controller khác)
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to create course: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            // Tải lại dữ liệu nếu có lỗi
            ViewBag.Majors = await _majorService.GetAllMajorsAsync();
            return View(course);
        }

        // Hành động CourseEdit: Hiển thị form chỉnh sửa khóa học
        public async Task<IActionResult> CourseEdit(int id)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Edit Course: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin khóa học theo ID (logic tương tự các Edit khác)
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                _singleton.Log($"Failed to edit course with ID {id}: Course not found");
                return NotFound();
            }

            // Chuẩn bị dữ liệu cho form
            ViewBag.Majors = await _majorService.GetAllMajorsAsync();
            return View(course);
        }

        // Hành động POST CourseEdit: Xử lý cập nhật khóa học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseEdit(int id, Course course)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to edit course: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra ID có khớp không (logic tương tự các Edit khác)
            if (id != course.CourseID)
            {
                _singleton.Log($"Invalid course edit attempt: ID mismatch for CourseID {id}");
                return NotFound();
            }

            // Kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                try
                {
                    // Cập nhật khóa học
                    await _courseService.UpdateCourseAsync(course);
                    _singleton.Log($"Course {course.CourseName} (ID: {course.CourseID}) updated by admin");
                    TempData["SuccessMessage"] = "Course updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (ArgumentException ex) // Lỗi tham số
                {
                    ModelState.AddModelError("MajorID", ex.Message);
                    _singleton.Log($"Failed to update course {course.CourseName}: {ex.Message}");
                }
                catch (Exception ex) // Lỗi chung
                {
                    _singleton.Log($"Failed to update course {course.CourseName}: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the course.");
                }
            }
            else
            {
                // Ghi log lỗi ModelState (tương tự các controller khác)
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to update course: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            // Tải lại dữ liệu nếu có lỗi
            ViewBag.Majors = await _majorService.GetAllMajorsAsync();
            return View(course);
        }

        // Hành động POST CourseDelete: Xử lý xóa khóa học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseDelete(int id)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to delete course: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Xóa khóa học và nhận kết quả (success, message)
            var (success, message) = await _courseService.DeleteCourseAsync(id);

            if (!success) // Nếu xóa thất bại
            {
                _singleton.Log($"Failed to delete course with ID {id}: {message}");
                TempData["ErrorMessage"] = message;
            }
            else // Nếu xóa thành công
            {
                _singleton.Log($"Course with ID {id} deleted by admin");
                TempData["SuccessMessage"] = message;
            }

            return RedirectToAction("Index");
        }
    }
}