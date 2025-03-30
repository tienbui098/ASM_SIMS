using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Models;
using SIMS_ASM.Services;
using SIMS_ASM.Singleton;

namespace SIMS_ASM.Controllers
{
    public class ClassCourseFacultyController : Controller
    {
        private readonly IClassCourseFacultyService _classCourseFacultyService;
        private readonly IUserService _userService;
        private readonly ClassService _classService;
        private readonly ICourseService _courseService;
        private readonly AccountSingleton _singleton;

        public ClassCourseFacultyController(IClassCourseFacultyService classCourseFacultyService,
            IUserService userService, ICourseService courseService, ClassService classService)
        {
            _classCourseFacultyService = classCourseFacultyService;
            _userService = userService;
            _courseService = courseService;
            _classService = classService;
            _singleton = AccountSingleton.Instance;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // Hiển thị danh sách ClassCourseFaculty
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to ClassCourseFaculty Management: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var classCourseFaculties = await _classCourseFacultyService.GetAllClassCourseFacultiesAsync();
            return View(classCourseFaculties);
        }

        // Hiển thị form tạo ClassCourseFaculty mới
        public async Task<IActionResult> CreateClassCourseFaculty()
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Create ClassCourseFaculty: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Classes = _classService.GetAllClasses();
            ViewBag.Courses = await _courseService.GetAllCoursesAsync();
            ViewBag.Lectureres = await _userService.GetLecturersAsync(); // Giả sử bạn đã thêm phương thức này vào IUserService
            return View();
        }

        // Xử lý POST để tạo ClassCourseFaculty mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClassCourseFaculty(ClassCourseFaculty classCourseFaculty)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to create ClassCourseFaculty: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    await _classCourseFacultyService.AddClassCourseFacultyAsync(classCourseFaculty);
                    _singleton.Log($"ClassCourseFaculty {classCourseFaculty.ClassCourseFacultyID} created by admin");
                    TempData["SuccessMessage"] = "ClassCourseFaculty created successfully!";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    _singleton.Log($"Failed to create ClassCourseFaculty: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _singleton.Log($"Failed to create ClassCourseFaculty: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while creating the ClassCourseFaculty.");
                }
            }

            ViewBag.Classes = _classService.GetAllClasses();
            ViewBag.Courses = await _courseService.GetAllCoursesAsync();
            ViewBag.Lectureres = await _userService.GetLecturersAsync();
            return View(classCourseFaculty);
        }

        // Hiển thị form chỉnh sửa ClassCourseFaculty
        public async Task<IActionResult> EditClassCourseFaculty(int id)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Edit ClassCourseFaculty: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var classCourseFaculty = await _classCourseFacultyService.GetClassCourseFacultyByIdAsync(id);
            if (classCourseFaculty == null)
            {
                _singleton.Log($"Failed to edit ClassCourseFaculty with ID {id}: Not found");
                return NotFound();
            }

            ViewBag.Classes = _classService.GetAllClasses();
            ViewBag.Courses = await _courseService.GetAllCoursesAsync();
            ViewBag.Lectureres = await _userService.GetLecturersAsync();
            return View(classCourseFaculty);
        }

        // Xử lý POST để cập nhật ClassCourseFaculty
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClassCourseFaculty(int id, ClassCourseFaculty classCourseFaculty)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to edit ClassCourseFaculty: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            if (id != classCourseFaculty.ClassCourseFacultyID)
            {
                _singleton.Log($"Invalid ClassCourseFaculty edit attempt: ID mismatch for ClassCourseFacultyID {id}");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    await _classCourseFacultyService.UpdateClassCourseFacultyAsync(classCourseFaculty);
                    _singleton.Log($"ClassCourseFaculty {classCourseFaculty.ClassCourseFacultyID} updated by admin");
                    TempData["SuccessMessage"] = "ClassCourseFaculty updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _singleton.Log($"Failed to update ClassCourseFaculty {classCourseFaculty.ClassCourseFacultyID}: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the ClassCourseFaculty.");
                }
            }

            ViewBag.Classes = _classService.GetAllClasses();
            ViewBag.Courses = await _courseService.GetAllCoursesAsync();
            ViewBag.Lectureres = await _userService.GetLecturersAsync();
            return View(classCourseFaculty);
        }

        // Xử lý xóa ClassCourseFaculty
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClassCourseFaculty(int id)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to delete ClassCourseFaculty: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            try
            {
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
