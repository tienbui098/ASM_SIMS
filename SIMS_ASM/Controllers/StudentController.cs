using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Singleton;
using SIMS_ASM.Services;

namespace SIMS_ASM.Controllers
{
    public class StudentController : Controller
    {
        // Khai báo các dịch vụ và context cần thiết
        private readonly ApplicationDbContex _context; 
        private readonly IUserService _userService; 
        private readonly AccountSingleton _singleton; 
        private readonly ClassService _classService; 
        private readonly IEnrollmentService _enrollmentService; 
        private readonly IGradeService _gradeService; 

        // Constructor: Inject các dịch vụ và context (đã giải thích ở các controller trước)
        public StudentController(ApplicationDbContex context, IUserService userService,
            ClassService classService, IEnrollmentService enrollmentService,
            IGradeService gradeService)
        {
            _context = context;
            _userService = userService;
            _singleton = AccountSingleton.Instance;
            _classService = classService;
            _enrollmentService = enrollmentService;
            _gradeService = gradeService;
        }

        // Phương thức kiểm tra quyền Admin (đã giải thích trước)
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // Phương thức kiểm tra quyền Student
        private bool IsStudent()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Student";
        }

        // Lấy thông tin username của người dùng hiện tại từ Session (đã giải thích ở LecturerController)
        private string GetCurrentUsername()
        {
            return HttpContext.Session.GetString("Username");
        }

        // Hành động Index: Trang chính cho Student
        public async Task<IActionResult> Index()
        {
            // Kiểm tra quyền truy cập: Chỉ Student được phép
            if (!IsStudent())
            {
                return RedirectToAction("Login", "Account");
            }

            var username = GetCurrentUsername(); // Lấy username từ Session
            var user = await _userService.GetUserByUsernameAsync(username); // Lấy thông tin sinh viên

            if (user == null)
            {
                return NotFound(); // Không tìm thấy sinh viên
            }

            return View(user); // Trả về view với thông tin sinh viên
        }

        // Hành động ViewStudent: Xem thông tin sinh viên và danh sách lớp học
        public async Task<IActionResult> ViewStudent()
        {
            // Kiểm tra quyền truy cập
            if (!IsStudent())
            {
                return RedirectToAction("Login", "Account");
            }

            var username = GetCurrentUsername();
            var user = await _userService.GetUserByUsernameAsync(username); // Lấy thông tin sinh viên

            if (user == null)
            {
                return NotFound();
            }

            // Lấy danh sách lớp học của sinh viên
            user.StudentClasses = await _classService.GetStudentClassesByUserIdAsync(user.UserID);

            return View(user); // Trả về view với thông tin sinh viên và danh sách lớp
        }

        // Hành động ViewEnrollment: Xem danh sách ghi danh của sinh viên
        public async Task<IActionResult> ViewEnrollment()
        {
            // Kiểm tra quyền truy cập
            if (!IsStudent())
            {
                return RedirectToAction("Login", "Account");
            }

            var username = GetCurrentUsername();
            var user = await _userService.GetUserByUsernameAsync(username); // Lấy thông tin sinh viên

            if (user == null)
            {
                return NotFound();
            }

            // Lấy danh sách ghi danh của sinh viên
            var enrollments = await _enrollmentService.GetEnrollmentsByUserIdAsync(user.UserID);

            return View(enrollments); // Trả về view với danh sách ghi danh
        }

        // Hành động ViewGrade: Xem điểm số của sinh viên
        public async Task<IActionResult> ViewGrade()
        {
            // Kiểm tra quyền truy cập
            if (!IsStudent())
            {
                return RedirectToAction("Login", "Account");
            }

            var username = GetCurrentUsername();
            var user = await _userService.GetUserByUsernameAsync(username); // Lấy thông tin sinh viên

            if (user == null)
            {
                return NotFound();
            }

            // Lấy danh sách ghi danh và điểm số của sinh viên
            var enrollments = await _enrollmentService.GetEnrollmentsByUserIdAsync(user.UserID);
            var grades = await _gradeService.GetGradesByUserId(user.UserID); // Lấy điểm số theo UserID

            return View(grades); // Trả về view với danh sách điểm số
        }

        // Hành động ManageStudent: Quản lý danh sách sinh viên (chỉ Admin)
        public async Task<IActionResult> ManageStudent()
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

        // Hành động AddStudent: Hiển thị form thêm sinh viên mới
        public IActionResult AddStudent()
        {
            ViewBag.SystemName = "Student Information Management System"; // Gán tên hệ thống cho view
            return View(); // Trả về form thêm mới
        }

        // Hành động POST AddStudent: Xử lý thêm sinh viên mới
        [HttpPost]
        [ValidateAntiForgeryToken] // Ngăn chặn tấn công CSRF (đã giải thích trước)
        public async Task<IActionResult> AddStudent(User user)
        {
            // Kiểm tra dữ liệu đầu vào (logic tương tự các POST khác)
            if (!ModelState.IsValid)
            {
                _singleton.Log($"Failed adding student: Invalid data for {user.Username}");
                ViewBag.SystemName = "Student Information Management System";
                return View(user); // Trả về form nếu dữ liệu không hợp lệ
            }

            try
            {
                // Thêm sinh viên mới với role "Student"
                await _userService.AddUserAsync(user, "Student");
                _singleton.Log($"User {user.Username} added successfully with role Student");
                return RedirectToAction("ManageStudent"); // Chuyển hướng về danh sách
            }
            catch (ArgumentException ex) // Lỗi tham số (ví dụ: username trùng)
            {
                ModelState.AddModelError("Username", ex.Message);
                _singleton.Log($"Failed adding student: {ex.Message}");
            }
            catch (InvalidOperationException ex) // Lỗi thao tác không hợp lệ
            {
                ModelState.AddModelError("", ex.Message);
                _singleton.Log($"Failed adding student: {ex.Message}");
            }
            catch (Exception ex) // Xử lý lỗi chung
            {
                _singleton.Log($"Failed to add student {user.Username}: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while adding.");
            }

            ViewBag.SystemName = "Student Information Management System";
            return View(user); // Trả về form nếu có lỗi
        }

        // Hành động UpdateStudent: Hiển thị form cập nhật sinh viên
        [HttpGet]
        public async Task<IActionResult> UpdateStudent(int id)
        {
            var user = await _context.Users.FindAsync(id); // Lấy thông tin sinh viên theo ID
            if (user == null)
            {
                return NotFound(); // Không tìm thấy
            }

            return View(user); // Trả về form cập nhật
        }

        // Hành động POST UpdateStudent: Xử lý cập nhật sinh viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStudent(int id, User updatedUser)
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
                    var existingUser = await _context.Users.FindAsync(id); // Lấy sinh viên hiện tại
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
                    return RedirectToAction(nameof(ManageStudent));
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

        // Hành động Delete: Xóa sinh viên
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

            return RedirectToAction(nameof(ManageStudent));
        }

        // Phương thức hỗ trợ: Kiểm tra người dùng tồn tại
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }
    }
}