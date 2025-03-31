using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIMS_ASM.Models;
using SIMS_ASM.Services;
using SIMS_ASM.Singleton;

namespace SIMS_ASM.Controllers
{
    public class StudentClassController : Controller
    {
        private readonly IStudentClassService _studentClassService;
        private readonly IUserService _userService;
        private readonly ClassService _classService;
        private readonly AccountSingleton _singleton;

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

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // Hiển thị danh sách gán sinh viên vào lớp
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to StudentClass Management: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var studentClasses = await _studentClassService.GetAllStudentClassesAsync();
            return View(studentClasses);
        }

        // Hiển thị form gán sinh viên vào lớp mới
        public async Task<IActionResult> CreateStudentClass()
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Create StudentClass: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Students = await _userService.GetStudentsAsync();
            ViewBag.Classes = _classService.GetAllClasses();
            return View();
        }

        // Xử lý POST để gán sinh viên vào lớp mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudentClass(StudentClass studentClass)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to create student-class assignment: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    await _studentClassService.AddStudentClassAsync(studentClass);
                    _singleton.Log($"StudentClass {studentClass.StudentClassID} created by admin for student {studentClass.UserID}");
                    TempData["SuccessMessage"] = "Student assigned to class successfully!";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    _singleton.Log($"Failed to create student-class assignment: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _singleton.Log($"Failed to create student-class assignment: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while assigning the student to the class.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to create student-class assignment: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            ViewBag.Students = await _userService.GetStudentsAsync();
            ViewBag.Classes = _classService.GetAllClasses();
            return View(studentClass);
        }

        // Hiển thị form chỉnh sửa gán sinh viên vào lớp
        public async Task<IActionResult> EditStudentClass(int id)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Edit StudentClass: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var studentClass = await _studentClassService.GetStudentClassByIdAsync(id);
            if (studentClass == null)
            {
                _singleton.Log($"Failed to edit student-class assignment with ID {id}: Not found");
                return NotFound();
            }

            ViewBag.Students = await _userService.GetStudentsAsync();
            ViewBag.Classes = _classService.GetAllClasses();
            return View(studentClass);
        }

        // Xử lý POST để cập nhật gán sinh viên vào lớp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudentClass(int id, StudentClass studentClass)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to edit student-class assignment: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            if (id != studentClass.StudentClassID)
            {
                _singleton.Log($"Invalid student-class edit attempt: ID mismatch for StudentClassID {id}");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    await _studentClassService.UpdateStudentClassAsync(studentClass);
                    _singleton.Log($"StudentClass {studentClass.StudentClassID} updated by admin");
                    TempData["SuccessMessage"] = "Student-class assignment updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _singleton.Log($"Failed to update student-class assignment {studentClass.StudentClassID}: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the student-class assignment.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to update student-class assignment: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            ViewBag.Students = await _userService.GetStudentsAsync();
            ViewBag.Classes = _classService.GetAllClasses();
            return View(studentClass);
        }

        // Xử lý xóa gán sinh viên khỏi lớp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudentClass(int id)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to delete student-class assignment: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var studentClass = await _studentClassService.GetStudentClassByIdAsync(id);
                if (studentClass == null)
                {
                    _singleton.Log($"Failed to delete student-class assignment with ID {id}: Not found");
                    return NotFound();
                }

                var hasEnrollments = await _studentClassService.HasAssociatedEnrollmentsAsync(studentClass.UserID, studentClass.ClassID);
                if (hasEnrollments)
                {
                    _singleton.Log($"Failed to delete student-class assignment with ID {id}: Student has enrollments related to this class.");
                    TempData["ErrorMessage"] = "Cannot delete student-class assignment because the student has enrollments related to this class.";
                }
                else
                {
                    await _studentClassService.DeleteStudentClassAsync(id);
                    _singleton.Log($"StudentClass with ID {id} deleted by admin");
                    TempData["SuccessMessage"] = "Student-class assignment deleted successfully!";
                }
            }
            catch (Exception ex)
            {
                _singleton.Log($"Failed to delete student-class assignment with ID {id}: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while deleting the student-class assignment.";
            }

            return RedirectToAction("Index");
        }

        // Hiển thị form thêm nhiều sinh viên vào lớp
        public async Task<IActionResult> AddMultipleStudents(int? classId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.Classes = _classService.GetAllClasses();

            // Load all students
            var allStudents = await _userService.GetStudentsAsync();

            // Nếu có classId, load danh sách sinh viên đã trong lớp đó
            if (classId.HasValue)
            {
                var studentsInClass = await _studentClassService.GetStudentsInClassAsync(classId.Value);
                var studentIdsInClass = studentsInClass.Select(s => s.UserID).ToList();

                ViewBag.SelectedClassId = classId.Value;
                ViewBag.Students = allStudents.Select(s => new SelectListItem
                {
                    Value = s.UserID.ToString(),
                    Text = s.FullName,
                    Selected = studentIdsInClass.Contains(s.UserID)
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

            return View();
        }


        // Xử lý thêm nhiều sinh viên vào lớp
        [HttpPost]
        public async Task<IActionResult> AddMultipleStudents(int classId, List<int> studentIds, string action)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            try
            {
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

                if (studentIds == null || !studentIds.Any())
                {
                    TempData["ErrorMessage"] = "Please select at least one student";
                    return RedirectToAction("AddMultipleStudents", new { classId });
                }

                await _studentClassService.AddMultipleStudentsToClassAsync(studentIds, classId);
                TempData["SuccessMessage"] = $"Successfully updated students in class";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("AddMultipleStudents", new { classId });
            }
        }

    }
}
