using Microsoft.AspNetCore.Mvc;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using SIMS_ASM.Singleton;
using Microsoft.EntityFrameworkCore;


namespace SIMS_ASM.Controllers
{
    public class CourseController :Controller
    {
        // Khai báo DbContext để truy cập dữ liệu từ cơ sở dữ liệu
        private readonly ApplicationDbContex _context;
        // Khai báo singleton để ghi log
        private readonly AccountSingleton _singleton;

        // Constructor để khởi tạo context và singleton
        public CourseController(ApplicationDbContex context)
        {
            _context = context;
            _singleton = AccountSingleton.Instance;
        }

        // Kiểm tra quyền Admin
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role"); // Lấy quyền từ Session
            return role == "Administrator"; // Kiểm tra quyền Admin
        }

        // Hiển thị danh sách khóa học
        public async Task<IActionResult> Index()
        {
            // Kiểm tra quyền Admin
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Course Management: User not an admin");
                return RedirectToAction("Login", "Account");
            }
            // Lấy danh sách khóa học từ cơ sở dữ liệu
            var courses = await _context.Courses.Include(c => c.User).ToListAsync();
            return View(courses);
        }

        // Thêm khóa học: Hiển thị form
        public async Task<IActionResult> CourseCreate()
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Create Course: User not an admin");
                return RedirectToAction("Login", "Account");
            }
            // Lấy danh sách giáo viên từ cơ sở dữ liệu
            ViewBag.Teacher = await _context.Users
                .Where(u => u.Role == "Teacher")
                .ToListAsync();
            return View();
        }

        //Phương thức POST để tạo khóa học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseCreate(Course course)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to create course: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid) // Nếu dữ liệu trong model hợp lệ
            {
                try
                {
                    _context.Courses.Add(course); // Thêm khóa học vào DbContext
                    await _context.SaveChangesAsync();// Lưu thay đổi vào cơ sở dữ liệu
                    _singleton.Log($"Course {course.CourseName} (ID: {course.CourseID}) created by admin");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    //Ghi log lỗi nếu có ngoại lệ
                    _singleton.Log($"Failed to create course {course.CourseName}: {ex.Message}");
                    // Thêm thông báo lỗi vào ModelState
                    ModelState.AddModelError("", "An error occurred while creating the course.");
                }
            }
            else
            {
                //Nếu model không hợp lệ, ghi log các lỗi
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to create course: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            //Lấy lại danh sách giáo viên để hiện thị trong view khi gặp lỗi
            ViewBag.Teacher = await _context.Users
                .Where(u => u.Role == "Teacher")
                .ToListAsync();
            return View(course);// Trả về View và giữ lại khóa học để sửa lỗi
        }

        //Phương thức hiển thị trang chỉnh sửa khóa học
        public async Task<IActionResult> CourseEdit(int id)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized access to Edit Course: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Tìm khóa học theo ID
            var course = await _context.Courses.FindAsync(id);
            if (course == null)// Nếu không tìm thấy khóa học
            {
                _singleton.Log($"Failed to edit course with ID {id}: Course not found");
                return NotFound();// Trả về lỗi 404
            }

            // Lấy danh sách giáo viên để hiển thị trong view
            ViewBag.Teacher = await _context.Users
                .Where(u => u.Role == "Teacher")
                .ToListAsync();
            return View(course);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseEdit(int id, Course course)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to edit course: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra xem ID khóa học có khớp với ID trong model không
            if (id != course.CourseID)
            {
                _singleton.Log($"Invalid course edit attempt: ID mismatch for CourseID {id}");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);// Cập nhật thông tin khóa học trong DbContext
                    await _context.SaveChangesAsync();//Lưu thay đổi vào cơ sở dữ liệu
                    _singleton.Log($"Course {course.CourseName} (ID: {course.CourseID}) updated by admin");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _singleton.Log($"Failed to update course {course.CourseName}: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the course.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _singleton.Log($"Failed to update course: Invalid model state. Errors: {string.Join(", ", errors)}");
            }

            ViewBag.Teacher = await _context.Users
                .Where(u => u.Role == "Teacher")
                .ToListAsync();
            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> CourseDelete(int id)
        {
            if (!IsAdmin())
            {
                _singleton.Log("Unauthorized attempt to delete course: User not an admin");
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                _singleton.Log($"Course {course.CourseName} (ID: {id}) deleted by admin");
            }
            else
            {
                _singleton.Log($"Failed to delete course with ID {id}: Course not found");
            }
            return RedirectToAction("Index");
        }
    }
}
