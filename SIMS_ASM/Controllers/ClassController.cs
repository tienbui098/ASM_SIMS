using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Models;
using SIMS_ASM.Services;
using SIMS_ASM.Singleton;

namespace SIMS_ASM.Controllers
{
    public class ClassController : Controller
    {
        private readonly ClassService _classService;
        private readonly AccountSingleton _singleton;

        public ClassController(ClassService classService)
        {
            _classService = classService;
            _singleton = AccountSingleton.Instance;
        }

        // Kiểm tra quyền Admin
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        public IActionResult Index(int? majorId)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Class Management: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var classes = majorId.HasValue && majorId > 0
                ? _classService.GetClassesByMajor(majorId.Value)
                : _classService.GetAllClasses();

            ViewBag.Majors = _classService.GetAllMajors();
            ViewBag.SelectedMajor = majorId;

            return View(classes);
        }

        // Hiển thị form tạo lớp học mới
        public IActionResult CreateClass()
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Create Class: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Majors = _classService.GetAllMajors(); // Để hiển thị dropdown Major
            return View();
        }

        // Xử lý POST để tạo lớp học mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateClass(Class newClass)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to create class: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    _classService.CreateClass(newClass);
                    _singleton.Log($"Class with ID {newClass.ClassID} created by admin");
                    TempData["SuccessMessage"] = "Class created successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _singleton.Log($"Failed to create class: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while creating the class.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to create class: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            ViewBag.Majors = _classService.GetAllMajors();
            return View(newClass);
        }

        // Hiển thị form chỉnh sửa lớp học
        public IActionResult EditClass(int id)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Edit Class: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var classDetails = _classService.GetClassDetails(id);
            if (classDetails == null)
            {
                _singleton.Log($"Failed to edit class with ID {id}: Class not found");
                return NotFound();
            }

            ViewBag.Majors = _classService.GetAllMajors();
            return View(classDetails);
        }

        // Xử lý POST để cập nhật lớp học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditClass(int id, Class updatedClass)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to edit class: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            if (id != updatedClass.ClassID)
            {
                _singleton.Log($"Invalid class edit attempt: ID mismatch for ClassID {id}");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    _classService.UpdateClass(updatedClass);
                    _singleton.Log($"Class {updatedClass.ClassID} updated by admin");
                    TempData["SuccessMessage"] = "Class updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _singleton.Log($"Failed to update class {updatedClass.ClassID}: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the class.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to update class: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            ViewBag.Majors = _classService.GetAllMajors();
            return View(updatedClass);
        }


        public IActionResult ClassesByMajor(int majorId)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to ClassesByMajor: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var classes = _classService.GetClassesByMajor(majorId);
            if (!classes.Any())
            {
                TempData["InfoMessage"] = "No classes found for this major.";
            }

            ViewBag.MajorId = majorId;
            ViewBag.Majors = _classService.GetAllMajors(); // Để hiển thị dropdown lọc
            return View(classes);
        }

        // Xử lý xóa lớp học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteClass(int id)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to delete class: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                _classService.DeleteClass(id);
                _singleton.Log($"Class with ID {id} deleted by admin");
                TempData["SuccessMessage"] = "Class deleted successfully!";
            }
            catch (Exception ex)
            {
                _singleton.Log($"Failed to delete class with ID {id}: {ex.Message}");
                TempData["ErrorMessage"] = "Cannot delete class due to an error or existing dependencies.";
            }

            return RedirectToAction("Index");
        }
    }
}
