using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIMS_ASM.Models;
using SIMS_ASM.Services;
using SIMS_ASM.Singleton;

namespace SIMS_ASM.Controllers
{
    public class StudentClassController : Controller
    {
        // Khai báo các dịch vụ cần thiết
        private readonly IStudentClassService _studentClassService; 
        private readonly IUserService _userService; 
        private readonly ClassService _classService; 
        private readonly AccountSingleton _singleton; 

        // Constructor: Inject các dịch vụ (đã giải thích ở các controller trước)
        public StudentClassController(
            IStudentClassService studentClassService,
            IUserService userService,
            ClassService classService)
        {
            _studentClassService = studentClassService;
            _userService = userService;
            _classService = classService;
            _singleton = AccountSingleton.Instance;
        }

        // Phương thức kiểm tra quyền Admin (đã giải thích trước)
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // Hành động Index: Hiển thị danh sách gán sinh viên vào lớp
        public async Task<IActionResult> Index()
        {
            // Kiểm tra quyền truy cập (logic tương tự các controller khác)
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to StudentClass Management: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Lấy tất cả gán sinh viên vào lớp từ dịch vụ
            var studentClasses = await _studentClassService.GetAllStudentClassesAsync();
            return View(studentClasses); // Trả về view với danh sách
        }

        // Hành động CreateStudentClass: Hiển thị form gán sinh viên vào lớp mới
        public async Task<IActionResult> CreateStudentClass()
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Create StudentClass: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Chuẩn bị dữ liệu cho dropdown
            ViewBag.Students = await _userService.GetStudentsAsync(); // Danh sách sinh viên
            ViewBag.Classes = _classService.GetAllClasses(); // Danh sách lớp học
            return View(); // Trả về form tạo mới
        }

        // Hành động POST CreateStudentClass: Xử lý gán sinh viên vào lớp mới
        [HttpPost]
        [ValidateAntiForgeryToken] // Ngăn chặn tấn công CSRF (đã giải thích trước)
        public async Task<IActionResult> CreateStudentClass(StudentClass studentClass)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to create student-class assignment: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra dữ liệu đầu vào (logic tương tự các POST khác)
            if (!ModelState.IsValid)
            {
                try
                {
                    // Thêm gán sinh viên vào lớp
                    await _studentClassService.AddStudentClassAsync(studentClass);
                    _singleton.Log($"StudentClass {studentClass.StudentClassID} created by admin for student {studentClass.UserID}");
                    TempData["SuccessMessage"] = "Student assigned to class successfully!";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex) // Lỗi thao tác không hợp lệ (ví dụ: trùng lặp)
                {
                    ModelState.AddModelError("", ex.Message);
                    _singleton.Log($"Failed to create student-class assignment: {ex.Message}");
                }
                catch (Exception ex) // Xử lý lỗi chung (đã giải thích trước)
                {
                    _singleton.Log($"Failed to create student-class assignment: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while assigning the student to the class.");
                }
            }
            else
            {
                // Ghi log lỗi ModelState (tương tự các controller khác)
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to create student-class assignment: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            // Tải lại dữ liệu nếu có lỗi
            ViewBag.Students = await _userService.GetStudentsAsync();
            ViewBag.Classes = _classService.GetAllClasses();
            return View(studentClass);
        }

        // Hành động EditStudentClass: Hiển thị form chỉnh sửa gán sinh viên vào lớp
        public async Task<IActionResult> EditStudentClass(int id)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Edit StudentClass: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin gán sinh viên vào lớp theo ID (logic tương tự các Edit khác)
            var studentClass = await _studentClassService.GetStudentClassByIdAsync(id);
            if (studentClass == null)
            {
                _singleton.Log($"Failed to edit student-class assignment with ID {id}: Not found");
                return NotFound();
            }

            // Chuẩn bị dữ liệu cho form
            ViewBag.Students = await _userService.GetStudentsAsync();
            ViewBag.Classes = _classService.GetAllClasses();
            return View(studentClass);
        }

        // Hành động POST EditStudentClass: Xử lý cập nhật gán sinh viên vào lớp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudentClass(int id, StudentClass studentClass)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to edit student-class assignment: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra ID có khớp không (logic tương tự các Edit khác)
            if (id != studentClass.StudentClassID)
            {
                _singleton.Log($"Invalid student-class edit attempt: ID mismatch for StudentClassID {id}");
                return NotFound();
            }

            // Kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                try
                {
                    // Cập nhật gán sinh viên vào lớp
                    await _studentClassService.UpdateStudentClassAsync(studentClass);
                    _singleton.Log($"StudentClass {studentClass.StudentClassID} updated by admin");
                    TempData["SuccessMessage"] = "Student-class assignment updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex) // Xử lý lỗi chung
                {
                    _singleton.Log($"Failed to update student-class assignment {studentClass.StudentClassID}: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the student-class assignment.");
                }
            }
            else
            {
                // Ghi log lỗi ModelState
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to update student-class assignment: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            // Tải lại dữ liệu nếu có lỗi
            ViewBag.Students = await _userService.GetStudentsAsync();
            ViewBag.Classes = _classService.GetAllClasses();
            return View(studentClass);
        }

        // Hành động POST DeleteStudentClass: Xử lý xóa gán sinh viên khỏi lớp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudentClass(int id)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to delete student-class assignment: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Lấy thông tin gán sinh viên vào lớp
                var studentClass = await _studentClassService.GetStudentClassByIdAsync(id);
                if (studentClass == null)
                {
                    _singleton.Log($"Failed to delete student-class assignment with ID {id}: Not found");
                    return NotFound();
                }

                // Kiểm tra xem sinh viên có ghi danh liên quan không
                var hasEnrollments = await _studentClassService.HasAssociatedEnrollmentsAsync(studentClass.UserID, studentClass.ClassID);
                if (hasEnrollments)
                {
                    _singleton.Log($"Failed to delete student-class assignment with ID {id}: Student has enrollments related to this class.");
                    TempData["ErrorMessage"] = "Cannot delete student-class assignment because the student has enrollments related to this class.";
                }
                else
                {
                    // Xóa gán sinh viên khỏi lớp
                    await _studentClassService.DeleteStudentClassAsync(id);
                    _singleton.Log($"StudentClass with ID {id} deleted by admin");
                    TempData["SuccessMessage"] = "Student-class assignment deleted successfully!";
                }
            }
            catch (Exception ex) // Xử lý lỗi chung
            {
                _singleton.Log($"Failed to delete student-class assignment with ID {id}: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while deleting the student-class assignment.";
            }

            return RedirectToAction("Index");
        }

        // Hành động AddMultipleStudents: Hiển thị form thêm nhiều sinh viên vào lớp
        public async Task<IActionResult> AddMultipleStudents(int? classId)
        {
            // Kiểm tra quyền truy cập (ngắn gọn, đã giải thích trước)
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Chuẩn bị dữ liệu cho form
            ViewBag.Classes = _classService.GetAllClasses(); // Danh sách lớp học
            var allStudents = await _userService.GetStudentsAsync(); // Tất cả sinh viên

            // Nếu có classId, đánh dấu sinh viên đã trong lớp
            if (classId.HasValue)
            {
                var studentsInClass = await _studentClassService.GetStudentsInClassAsync(classId.Value);
                var studentIdsInClass = studentsInClass.Select(s => s.UserID).ToList();

                ViewBag.SelectedClassId = classId.Value;
                ViewBag.Students = allStudents.Select(s => new SelectListItem
                {
                    Value = s.UserID.ToString(),
                    Text = s.FullName,
                    Selected = studentIdsInClass.Contains(s.UserID) // Đánh dấu sinh viên đã trong lớp
                }).ToList();
            }
            else
            {
                ViewBag.Students = allStudents.Select(s => new SelectListItem
                {
                    Value = s.UserID.ToString(),
                    Text = s.FullName
                }).ToList();
            }

            return View(); // Trả về form thêm nhiều sinh viên
        }

        // Hành động POST AddMultipleStudents: Xử lý thêm nhiều sinh viên vào lớp
        [HttpPost]
        public async Task<IActionResult> AddMultipleStudents(int classId, List<int> studentIds, string action)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            try
            {
                // Xử lý theo hành động người dùng chọn
                if (action == "addAll")
                {
                    // Thêm toàn bộ sinh viên
                    var allStudents = await _userService.GetStudentsAsync();
                    studentIds = allStudents.Select(s => s.UserID).ToList();
                }
                else if (action == "removeAll")
                {
                    // Xóa toàn bộ sinh viên khỏi lớp
                    await _studentClassService.RemoveAllStudentsFromClassAsync(classId);
                    TempData["SuccessMessage"] = "All students have been removed from the class";
                    return RedirectToAction("Index");
                }

                // Kiểm tra danh sách sinh viên được chọn
                if (studentIds == null || !studentIds.Any())
                {
                    TempData["ErrorMessage"] = "Please select at least one student";
                    return RedirectToAction("AddMultipleStudents", new { classId });
                }

                // Thêm nhiều sinh viên vào lớp
                await _studentClassService.AddMultipleStudentsToClassAsync(studentIds, classId);
                TempData["SuccessMessage"] = $"Successfully updated students in class";
                return RedirectToAction("Index");
            }
            catch (Exception ex) // Xử lý lỗi chung
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("AddMultipleStudents", new { classId });
            }
        }
    }
}