using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Singleton;
using SIMS_ASM.Services;

namespace SIMS_ASM.Controllers
{
    public class LecturerController : Controller
    {
        // Khai báo các dịch vụ và context cần thiết
        private readonly ApplicationDbContex _context; 
        private readonly IUserService _userService; 
        private readonly AccountSingleton _singleton; 
        private readonly IEnrollmentService _enrollmentService; 
        private readonly IGradeService _gradeService; 
        private readonly IClassCourseFacultyService _classCourseFacultyService; 
        private readonly ICourseService _courseService; 
        private readonly ClassService _classService; 

        // Constructor: Inject các dịch vụ và context (đã giải thích ở các controller trước)
        public LecturerController(ApplicationDbContex context,
            IUserService userService,
            IEnrollmentService enrollmentService,
            IGradeService gradeService,
            IClassCourseFacultyService classCourseFacultyService,
            ICourseService courseService,
            ClassService classService)
        {
            _context = context;
            _userService = userService;
            _singleton = AccountSingleton.Instance;
            _enrollmentService = enrollmentService;
            _gradeService = gradeService;
            _classCourseFacultyService = classCourseFacultyService;
            _courseService = courseService;
            _classService = classService;
        }

        // Phương thức kiểm tra quyền Admin (đã giải thích trước)
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // Phương thức kiểm tra quyền Lecturer (đã giải thích ở GradeController)
        private bool IsLecturer()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Lecturer";
        }

        // Lấy thông tin username của người dùng hiện tại từ Session
        private string GetCurrentUsername()
        {
            return HttpContext.Session.GetString("Username");
        }

        // Hành động Index: Trang chính cho Lecturer
        public async Task<IActionResult> Index()
        {
            // Kiểm tra quyền truy cập: Chỉ Lecturer được phép
            if (!IsLecturer())
            {
                return RedirectToAction("Login", "Account");
            }

            var username = GetCurrentUsername(); // Lấy username từ Session
            var user = await _userService.GetUserByUsernameAsync(username); // Lấy thông tin người dùng

            if (user == null)
            {
                return NotFound(); // Không tìm thấy người dùng
            }

            return View(user); // Trả về view với thông tin người dùng
        }

        // Hành động ViewGrade: Xem danh sách điểm số (tương tự GradeController.Index)
        public async Task<IActionResult> ViewGrade()
        {
            // Kiểm tra quyền truy cập
            if (!IsLecturer())
            {
                return RedirectToAction("Login", "Account");
            }

            var grades = await _gradeService.GetAllGradesAsync(); // Lấy tất cả điểm số

            // Chuẩn bị dữ liệu cho filter (dropdown)
            ViewBag.Classes = _classService.GetAllClasses();
            ViewBag.Courses = await _courseService.GetAllCoursesAsync();

            return View(grades); // Trả về view với danh sách điểm số
        }

        // Hành động ViewClassCourseFaculty: Xem danh sách ClassCourseFaculty của Lecturer
        public async Task<IActionResult> ViewClassCourseFaculty(int? classId, int? courseId, int? facultyId)
        {
            // Kiểm tra quyền truy cập
            if (!IsLecturer())
            {
                return RedirectToAction("Login", "Account");
            }

            var username = GetCurrentUsername();
            var user = await _userService.GetUserByUsernameAsync(username); // Lấy thông tin Lecturer

            if (user == null)
            {
                return NotFound();
            }

            // Lấy danh sách ClassCourseFaculty mà Lecturer này quản lý
            var classCourseFaculties = await _classCourseFacultyService.GetClassCourseFacultiesByUserId(user.UserID);
            ViewBag.CurrentUser = user; // Lưu thông tin người dùng vào ViewBag

            return View(classCourseFaculties); // Trả về view với danh sách ClassCourseFaculty
        }

        // Hành động ManageLecturer: Quản lý danh sách Lecturer (chỉ Admin)
        public async Task<IActionResult> ManageLecturer()
        {
            // Kiểm tra quyền truy cập (logic tương tự các controller khác)
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Class Management: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var users = await _context.Users.ToListAsync(); // Lấy tất cả người dùng
            return View(users); // Trả về view với danh sách người dùng
        }

        // Hành động AddLecturer: Hiển thị form thêm Lecturer mới
        public IActionResult AddLecturer()
        {
            ViewBag.SystemName = "Student Information Management System"; // Gán tên hệ thống cho view
            return View(); // Trả về form thêm mới
        }

        // Hành động POST AddLecturer: Xử lý thêm Lecturer mới
        [HttpPost]
        [ValidateAntiForgeryToken] // Ngăn chặn tấn công CSRF (đã giải thích trước)
        public async Task<IActionResult> AddLecturer(User user)
        {
            // Kiểm tra dữ liệu đầu vào (logic tương tự các POST khác)
            if (!ModelState.IsValid)
            {
                _singleton.Log($"Failed adding teacher: Invalid data for {user.Username}");
                ViewBag.SystemName = "Student Information Management System";
                return View(user); // Trả về lại form nếu dữ liệu không hợp lệ
            }

            try
            {
                // Thêm Lecturer mới với role "Lecturer"
                await _userService.AddUserAsync(user, "Lecturer");
                _singleton.Log($"User {user.Username} added successfully with role Lecturer");
                return RedirectToAction("ManageLecturer"); // Chuyển hướng về danh sách
            }
            catch (ArgumentException ex) // Lỗi tham số (ví dụ: username trùng)
            {
                ModelState.AddModelError("Username", ex.Message);
                _singleton.Log($"Failed adding teacher: {ex.Message}");
            }
            catch (InvalidOperationException ex) // Lỗi thao tác không hợp lệ
            {
                ModelState.AddModelError("", ex.Message);
                _singleton.Log($"Failed adding teacher: {ex.Message}");
            }
            catch (Exception ex) // Xử lý lỗi chung
            {
                _singleton.Log($"Failed to add teacher {user.Username}: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while adding.");
            }

            ViewBag.SystemName = "Student Information Management System";
            return View(user); // Trả về form nếu có lỗi
        }

        // Hành động Delete: Xóa Lecturer
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to delete major: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Xóa người dùng và nhận kết quả (logic tương tự các Delete khác)
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                _singleton.Log($"Failed to delete user with ID {id}: User not found or has associated StudentClass/ClassCourseFaculty/Enrollment");
                TempData["ErrorMessage"] = "Cannot delete user because it is associated with Class Courses or Student Classes or Enrollments, or it was not found.";
            }
            else
            {
                _singleton.Log($"User with ID {id} deleted by admin");
                TempData["SuccessMessage"] = "User deleted successfully!";
            }

            return RedirectToAction(nameof(ManageLecturer));
        }

        // Hành động UpdateLecturer: Hiển thị form cập nhật Lecturer
        [HttpGet]
        public async Task<IActionResult> UpdateLecturer(int id)
        {
            var user = await _context.Users.FindAsync(id); // Lấy thông tin người dùng theo ID
            if (user == null)
            {
                return NotFound(); // Không tìm thấy
            }

            return View(user); // Trả về form cập nhật
        }

        // Hành động POST UpdateLecturer: Xử lý cập nhật Lecturer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLecturer(int id, User updatedUser)
        {
            // Kiểm tra ID có khớp không (logic tương tự các Edit khác)
            if (id != updatedUser.UserID)
            {
                return NotFound();
            }

            // Kiểm tra dữ liệu đầu vào
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id); // Lấy người dùng hiện tại
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các trường thông tin (không thay đổi mật khẩu)
                    existingUser.FullName = updatedUser.FullName;
                    existingUser.Email = updatedUser.Email;
                    existingUser.Date_of_birth = updatedUser.Date_of_birth;
                    existingUser.Address = updatedUser.Address;
                    existingUser.Phone_number = updatedUser.Phone_number;
                    existingUser.Gender = updatedUser.Gender;

                    await _context.SaveChangesAsync(); // Lưu thay đổi
                    return RedirectToAction(nameof(ManageLecturer));
                }
                catch (DbUpdateConcurrencyException) // Xử lý lỗi đồng thời
                {
                    if (!UserExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(updatedUser); // Trả về form nếu có lỗi
        }

        // Phương thức hỗ trợ: Kiểm tra người dùng tồn tại
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }
    }
}