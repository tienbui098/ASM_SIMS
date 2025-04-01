using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Models;
using SIMS_ASM.Services;
using SIMS_ASM.Singleton;

namespace SIMS_ASM.Controllers
{
    public class ClassCourseFacultyController : Controller
    {
        // Khai báo các dịch vụ cần thiết để quản lý ClassCourseFaculty
        private readonly IClassCourseFacultyService _classCourseFacultyService; 
        private readonly IUserService _userService; 
        private readonly ClassService _classService; 
        private readonly ICourseService _courseService; 
        private readonly AccountSingleton _singleton; 

        // Constructor: Khởi tạo controller và inject các dịch vụ cần thiết
        public ClassCourseFacultyController(IClassCourseFacultyService classCourseFacultyService,
            IUserService userService, ICourseService courseService, ClassService classService)
        {
            _classCourseFacultyService = classCourseFacultyService;
            _userService = userService;
            _courseService = courseService;
            _classService = classService;
            _singleton = AccountSingleton.Instance;
        }

        // Phương thức kiểm tra: Xác định xem người dùng hiện tại có phải Admin không
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // Hành động Index: Hiển thị danh sách ClassCourseFaculty với khả năng lọc
        public async Task<IActionResult> Index(int? classId, int? courseId, int? facultyId)
        {
            // Kiểm tra quyền truy cập (đã giải thích ở các controller khác)
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to ClassCourseFaculty Management: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Lấy toàn bộ danh sách ClassCourseFaculty từ dịch vụ
            var classCourseFaculties = await _classCourseFacultyService.GetAllClassCourseFacultiesAsync();

            // Lọc theo classId nếu có (logic lọc tương tự các controller khác)
            if (classId.HasValue)
            {
                classCourseFaculties = classCourseFaculties.Where(ccf => ccf.ClassID == classId.Value).ToList();
            }

            // Lọc theo courseId nếu có
            if (courseId.HasValue)
            {
                classCourseFaculties = classCourseFaculties.Where(ccf => ccf.CourseID == courseId.Value).ToList();
            }

            // Lọc theo facultyId nếu có
            if (facultyId.HasValue)
            {
                classCourseFaculties = classCourseFaculties.Where(ccf => ccf.UserID == facultyId.Value).ToList();
            }

            // Chuẩn bị dữ liệu cho giao diện (dropdown lọc)
            ViewBag.Classes = _classService.GetAllClasses();
            ViewBag.Courses = await _courseService.GetAllCoursesAsync();
            ViewBag.Faculties = await _userService.GetLecturersAsync();

            // Lưu giá trị đã chọn để hiển thị lại trên giao diện
            ViewBag.SelectedClass = classId?.ToString();
            ViewBag.SelectedCourse = courseId?.ToString();
            ViewBag.SelectedFaculty = facultyId?.ToString();

            return View(classCourseFaculties); // Trả về view với danh sách đã lọc
        }

        // Hành động CreateClassCourseFaculty: Hiển thị form tạo mới
        public async Task<IActionResult> CreateClassCourseFaculty()
        {
            // Kiểm tra quyền truy cập (đã giải thích trước)
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Create ClassCourseFaculty: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Chuẩn bị dữ liệu cho form (tương tự các form tạo mới khác)
            ViewBag.Classes = _classService.GetAllClasses();
            ViewBag.Courses = await _courseService.GetAllCoursesAsync();
            ViewBag.Lectureres = await _userService.GetLecturersAsync(); // Lưu ý: Có thể là lỗi typo, nên là "Lecturers"
            return View();
        }

        // Hành động POST CreateClassCourseFaculty: Xử lý tạo mới
        [HttpPost]
        [ValidateAntiForgeryToken] // Ngăn chặn tấn công CSRF (đã giải thích trước)
        public async Task<IActionResult> CreateClassCourseFaculty(ClassCourseFaculty classCourseFaculty)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to create ClassCourseFaculty: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra dữ liệu đầu vào (logic tương tự các POST khác)
            if (!ModelState.IsValid)
            {
                try
                {
                    // Thêm bản ghi mới
                    await _classCourseFacultyService.AddClassCourseFacultyAsync(classCourseFaculty);
                    _singleton.Log($"ClassCourseFaculty {classCourseFaculty.ClassCourseFacultyID} created by admin");
                    TempData["SuccessMessage"] = "ClassCourseFaculty created successfully!";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex) // Lỗi thao tác không hợp lệ
                {
                    ModelState.AddModelError("", ex.Message);
                    _singleton.Log($"Failed to create ClassCourseFaculty: {ex.Message}");
                }
                catch (Exception ex) // Lỗi chung (đã giải thích trước)
                {
                    _singleton.Log($"Failed to create ClassCourseFaculty: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while creating the ClassCourseFaculty.");
                }
            }

            // Tải lại dữ liệu nếu có lỗi (tương tự các form khác)
            ViewBag.Classes = _classService.GetAllClasses();
            ViewBag.Courses = await _courseService.GetAllCoursesAsync();
            ViewBag.Lectureres = await _userService.GetLecturersAsync();
            return View(classCourseFaculty);
        }

        // Hành động EditClassCourseFaculty: Hiển thị form chỉnh sửa
        public async Task<IActionResult> EditClassCourseFaculty(int id)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Edit ClassCourseFaculty: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin theo ID (logic tương tự các Edit khác)
            var classCourseFaculty = await _classCourseFacultyService.GetClassCourseFacultyByIdAsync(id);
            if (classCourseFaculty == null)
            {
                _singleton.Log($"Failed to edit ClassCourseFaculty with ID {id}: Not found");
                return NotFound();
            }

            // Chuẩn bị dữ liệu cho form chỉnh sửa
            ViewBag.Classes = _classService.GetAllClasses();
            ViewBag.Courses = await _courseService.GetAllCoursesAsync();
            ViewBag.Lectureres = await _userService.GetLecturersAsync();
            return View(classCourseFaculty);
        }

        // Hành động POST EditClassCourseFaculty: Xử lý cập nhật
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClassCourseFaculty(int id, ClassCourseFaculty classCourseFaculty)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to edit ClassCourseFaculty: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra ID có khớp không (logic tương tự các Edit khác)
            if (id != classCourseFaculty.ClassCourseFacultyID)
            {
                _singleton.Log($"Invalid ClassCourseFaculty edit attempt: ID mismatch for ClassCourseFacultyID {id}");
                return NotFound();
            }

            // Kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                try
                {
                    // Cập nhật bản ghi
                    await _classCourseFacultyService.UpdateClassCourseFacultyAsync(classCourseFaculty);
                    _singleton.Log($"ClassCourseFaculty {classCourseFaculty.ClassCourseFacultyID} updated by admin");
                    TempData["SuccessMessage"] = "ClassCourseFaculty updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex) // Lỗi chung
                {
                    _singleton.Log($"Failed to update ClassCourseFaculty {classCourseFaculty.ClassCourseFacultyID}: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the ClassCourseFaculty.");
                }
            }

            // Tải lại dữ liệu nếu có lỗi
            ViewBag.Classes = _classService.GetAllClasses();
            ViewBag.Courses = await _courseService.GetAllCoursesAsync();
            ViewBag.Lectureres = await _userService.GetLecturersAsync();
            return View(classCourseFaculty);
        }

        // Hành động POST DeleteClassCourseFaculty: Xử lý xóa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClassCourseFaculty(int id)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to delete ClassCourseFaculty: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Xóa bản ghi theo ID (logic tương tự các Delete khác)
                await _classCourseFacultyService.DeleteClassCourseFacultyAsync(id);
                _singleton.Log($"ClassCourseFaculty with ID {id} deleted by admin");
                TempData["SuccessMessage"] = "ClassCourseFaculty deleted successfully!";
            }
            catch (Exception ex)
            {
                _singleton.Log($"Failed to delete ClassCourseFaculty with ID {id}: {ex.Message}");
                TempData["ErrorMessage"] = "Cannot delete ClassCourseFaculty due to an error or existing dependencies.";
            }

            return RedirectToAction("Index");
        }
    }
}