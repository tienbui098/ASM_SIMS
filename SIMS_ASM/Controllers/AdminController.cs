using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Singleton;
using SIMS_ASM.Models;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;
using SIMS_ASM.Services;

namespace SIMS_ASM.Controllers
{
    public class AdminController : Controller
    {
        //Context để tương tác với database
        private readonly ApplicationDbContex _context;
        private readonly IUserService _userService;
        private readonly AccountSingleton _singleton;

        // Constructor sử dụng dependency injection để khởi tạo context và dịch vụ
        public AdminController(ApplicationDbContex context, IUserService userService)
        {
            // Khởi tạo context
            _context = context;
            // Khởi tạo userService
            _userService = userService;
            //Lấy instance của Singleton
            _singleton = AccountSingleton.Instance;
        }

        // Kiểm tra quyền Admin
        private bool IsAdmin()
        {
            // Lấy role từ Session
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // Trang chính cho quản trị viên
        public async Task<IActionResult> Index()
        {
            // Kiểm tra quyền Admin
            if (!IsAdmin())
            {
                //Ghi log khi không phải admin
                _singleton.Log("Unauthorized access to Class Management: User not an admin");
                return RedirectToAction("Login", "Account");
            }
            // Lấy danh sách all người dùng
            var users = await _context.Users.ToListAsync();
            // Đếm số lượng người dùng và khóa học
            ViewBag.StudentCount = users.Count(u => u.Role == "Student");
            ViewBag.LecturerCount = users.Count(u => u.Role == "Lecturer");
            ViewBag.CourseCount = await _context.Courses.CountAsync();
            return View(users);
        }

        // Hiển thị danh sách Admin
        public async Task<IActionResult> ManageAdmin()
        {
            //lấy all user
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // Hiển thị form thêm Admin
        public IActionResult AddAdmin()
        {
            //đặt tên hệ thống cho ViewBag để hiển thị trên giao diện
            ViewBag.SystemName = "Student Information Management System";
            return View();
        }

        // Xử lý thêm Admin
        [HttpPost]
        // Bảo vệ khỏi tấn công CSRF
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAdmin(User user)
        {
            // Kiểm tra quyền Admin
            if (!ModelState.IsValid)
            {
                //Duyệt qua các lỗi của ModelState và ghi log
                foreach (var entry in ModelState)
                {
                    //tên thuộc tính
                    var key = entry.Key;
                    //danh sách lỗi
                    var errors = entry.Value.Errors.Select(e => e.ErrorMessage);
                    //ghi log lại chi tiết lỗi
                    _singleton.Log($"Property: {key}, Errors: {string.Join(", ", errors)}");
                }
                ViewBag.SystemName = "Student Information Management System";
                return View(user);
            }

            try
            {
                // Thêm người dùng với role Admin
                await _userService.AddUserAsync(user, "Admin");
                _singleton.Log($"User {user.Username} added successfully with role Admin");
                return RedirectToAction("ManageAdmin");
            }
            //xử lý lỗi tham số không hợp lệ
            catch (ArgumentException ex)
            {
                //thêm lỗi vào ModelState
                ModelState.AddModelError("Username", ex.Message);
                _singleton.Log($"Failed adding attempt: {ex.Message}");
            }
            // Xử lý lỗi thao tác không hợp lệ
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                _singleton.Log($"Failed adding attempt: {ex.Message}");
            }
            // Xử lý các lỗi không xác định khác
            catch (Exception ex)
            {
                _singleton.Log($"Failed to add user {user.Username}: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while adding. Please try again.");
            }

            ViewBag.SystemName = "Student Information Management System";
            return View(user);
        }

        // Xử lý xóa người dùng
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            // Kiểm tra quyền Admin
            if (!IsAdmin())
            {
                //Ghi log khi không phải admin
                _singleton.Log("Unauthorized attempt to delete major: User not an admin");
                return RedirectToAction("Login", "Account");
            }
            // Xóa người dùng
            var success = await _userService.DeleteUserAsync(id);
            //nếu xóa thất bại
            if (!success)
            {
                _singleton.Log($"Failed to delete user with ID {id}: User not found or has associated StudentClass/ClassCourseFaculty/Enrollment");
                TempData["ErrorMessage"] = "Cannot delete user because it is associated with Class Courses or Student Classes or Enrollments, or it was not found.";
            }
            //nếu xóa thành công
            else
            {
                _singleton.Log($"User with ID {id} deleted by admin");
                TempData["SuccessMessage"] = "User deleted successfully!";
            }

            return RedirectToAction(nameof(ManageAdmin));
        }

        // Hiển thị form cập nhật Admin
        [HttpGet]
        public async Task<IActionResult> UpdateAdmin(int id)
        {
            //Tìm user theo id
            var user = await _context.Users.FindAsync(id);
            if (user == null)//nếu không tìm thấy user
            {
                return NotFound();//trả về trang 404 Not Found
            }

            return View(user);
        }

        // Xử lý cập nhật thông tin Admin (HTTP POST)
        [HttpPost]
        // Bảo vệ chống tấn công CSRF
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAdmin(int id, User updatedUser)
        {
            if (id != updatedUser.UserID)// Kiểm tra ID có khớp không
            {
                // Trả về lỗi 404 nếu không khớp
                return NotFound();
            }
            // Kiểm tra tính hợp lệ của dữ liệu nhập vào
            if (ModelState.IsValid)
            {
                try
                {
                    //tìm người dùng hiện tại
                    var existingUser = await _context.Users.FindAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các trường thông tin, giữ nguyên thông tin nhạy cảm như mật khẩu
                    existingUser.FullName = updatedUser.FullName;
                    existingUser.Email = updatedUser.Email;
                    existingUser.Date_of_birth = updatedUser.Date_of_birth;
                    existingUser.Address = updatedUser.Address;
                    existingUser.Phone_number = updatedUser.Phone_number;
                    existingUser.Gender = updatedUser.Gender;

                    //Lưu thay đổi vào database
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ManageAdmin));
                }
                // Xử lý lỗi xung đột khi cập nhật
                catch (DbUpdateConcurrencyException)
                {
                    // Kiểm tra người dùng có tồn tại không
                    if (!UserExists(id))
                    {
                        // Trả về lỗi 404 nếu không tồn tại
                        return NotFound();
                    }
                    else
                    {
                        // Ném lỗi nếu có lỗi khác
                        throw;
                    }
                }
            }

            return View(updatedUser);
        }

        // Kiểm tra sự tồn tại của người dùng theo ID
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }

    }
}
