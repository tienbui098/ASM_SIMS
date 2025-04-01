using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Models;
using SIMS_ASM.Services;
using SIMS_ASM.Singleton;

namespace SIMS_ASM.Controllers
{
    public class MajorController : Controller
    {
        // Khai báo dịch vụ và singleton cần thiết
        private readonly IMajorService _majorService; 
        private readonly AccountSingleton _singleton; 

        // Constructor: Inject dịch vụ (đã giải thích ở các controller trước)
        public MajorController(IMajorService majorService)
        {
            _majorService = majorService;
            _singleton = AccountSingleton.Instance;
        }

        // Phương thức kiểm tra quyền Admin (đã giải thích trước)
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // Hành động Index: Hiển thị danh sách ngành học
        public async Task<IActionResult> Index()
        {
            // Kiểm tra quyền truy cập (logic tương tự các controller khác)
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Major Management: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Lấy tất cả ngành học từ dịch vụ
            var majors = await _majorService.GetAllMajorsAsync();
            return View(majors); // Trả về view với danh sách ngành học
        }

        // Hành động MajorCreate: Hiển thị form tạo ngành học mới
        public IActionResult MajorCreate()
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Create Major: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            return View(); // Trả về form tạo mới
        }

        // Hành động POST MajorCreate: Xử lý tạo ngành học mới
        [HttpPost]
        [ValidateAntiForgeryToken] // Ngăn chặn tấn công CSRF (đã giải thích trước)
        public async Task<IActionResult> MajorCreate(Major major)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to create major: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra dữ liệu đầu vào (logic tương tự các POST khác)
            if (ModelState.IsValid)
            {
                try
                {
                    // Tạo ngành học mới
                    var createdMajor = await _majorService.CreateMajorAsync(major);
                    _singleton.Log($"Major {createdMajor.MajorName} (ID: {createdMajor.MajorID}) created by admin");
                    TempData["SuccessMessage"] = "Major created successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex) // Xử lý lỗi chung (đã giải thích trước)
                {
                    _singleton.Log($"Failed to create major {major.MajorName}: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while creating the major.");
                }
            }
            else
            {
                // Ghi log lỗi ModelState (tương tự các controller khác)
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to create major: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            return View(major); // Trả về form nếu có lỗi
        }

        // Hành động MajorEdit: Hiển thị form chỉnh sửa ngành học
        public async Task<IActionResult> MajorEdit(int id)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Edit Major: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin ngành học theo ID (logic tương tự các Edit khác)
            var major = await _majorService.GetMajorByIdAsync(id);
            if (major == null)
            {
                _singleton.Log($"Failed to edit major with ID {id}: Major not found");
                return NotFound();
            }

            return View(major); // Trả về form chỉnh sửa
        }

        // Hành động POST MajorEdit: Xử lý cập nhật ngành học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MajorEdit(int id, Major major)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to edit major: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra ID có khớp không (logic tương tự các Edit khác)
            if (id != major.MajorID)
            {
                _singleton.Log($"Invalid major edit attempt: ID mismatch for MajorID {id}");
                return NotFound();
            }

            // Kiểm tra dữ liệu đầu vào
            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật ngành học
                    await _majorService.UpdateMajorAsync(major);
                    _singleton.Log($"Major {major.MajorName} (ID: {major.MajorID}) updated by admin");
                    TempData["SuccessMessage"] = "Major updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex) // Xử lý lỗi chung
                {
                    _singleton.Log($"Failed to update major {major.MajorName}: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the major.");
                }
            }
            else
            {
                // Ghi log lỗi ModelState
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to update major: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            return View(major); // Trả về form nếu có lỗi
        }

        // Hành động POST MajorDelete: Xử lý xóa ngành học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MajorDelete(int id)
        {
            // Kiểm tra quyền truy cập
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to delete major: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Xóa ngành học và nhận kết quả (logic tương tự các Delete khác)
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