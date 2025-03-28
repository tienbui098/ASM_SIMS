using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Models;
using SIMS_ASM.Services;
using SIMS_ASM.Singleton;

namespace SIMS_ASM.Controllers
{
    public class MajorController : Controller
    {
        private readonly IMajorService _majorService;
        private readonly AccountSingleton _singleton;

        public MajorController(IMajorService majorService)
        {
            _majorService = majorService;
            _singleton = AccountSingleton.Instance;
        }

        // Kiểm tra quyền Admin
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // Hiển thị danh sách ngành học
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Major Management: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var majors = await _majorService.GetAllMajorsAsync();
            return View(majors);
        }

        // Hiển thị form tạo ngành học mới
        public IActionResult MajorCreate()
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Create Major: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // Xử lý POST để tạo ngành học mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MajorCreate(Major major)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to create major: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var createdMajor = await _majorService.CreateMajorAsync(major);
                    _singleton.Log($"Major {createdMajor.MajorName} (ID: {createdMajor.MajorID}) created by admin");
                    TempData["SuccessMessage"] = "Major created successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _singleton.Log($"Failed to create major {major.MajorName}: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while creating the major.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to create major: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            return View(major);
        }

        // Hiển thị form chỉnh sửa ngành học
        public async Task<IActionResult> MajorEdit(int id)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Edit Major: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var major = await _majorService.GetMajorByIdAsync(id);
            if (major == null)
            {
                _singleton.Log($"Failed to edit major with ID {id}: Major not found");
                return NotFound();
            }

            return View(major);
        }

        // Xử lý POST để cập nhật ngành học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MajorEdit(int id, Major major)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to edit major: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            if (id != major.MajorID)
            {
                _singleton.Log($"Invalid major edit attempt: ID mismatch for MajorID {id}");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _majorService.UpdateMajorAsync(major);
                    _singleton.Log($"Major {major.MajorName} (ID: {major.MajorID}) updated by admin");
                    TempData["SuccessMessage"] = "Major updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _singleton.Log($"Failed to update major {major.MajorName}: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the major.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to update major: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            return View(major);
        }

        // Xử lý xóa ngành học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MajorDelete(int id)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to delete major: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var success = await _majorService.DeleteMajorAsync(id);
            if (!success)
            {
                _singleton.Log($"Failed to delete major with ID {id}: Major not found or has associated courses/classes");
                TempData["ErrorMessage"] = "Cannot delete major because it is associated with courses or classes, or it was not found.";
            }
            else
            {
                _singleton.Log($"Major with ID {id} deleted by admin");
                TempData["SuccessMessage"] = "Major deleted successfully!";
            }

            return RedirectToAction("Index");
        }
    }
}
