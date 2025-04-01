using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public class ClassCourseFacultyService : IClassCourseFacultyService
    {
        // Khai báo context để truy cập cơ sở dữ liệu
        private readonly ApplicationDbContex _context;

        // Constructor: Inject ApplicationDbContex (đã giải thích ở các dịch vụ trước)
        public ClassCourseFacultyService(ApplicationDbContex context)
        {
            _context = context;
        }

        // Lấy tất cả bản ghi ClassCourseFaculty kèm thông tin liên quan
        public async Task<IEnumerable<ClassCourseFaculty>> GetAllClassCourseFacultiesAsync()
        {
            return await _context.ClassCourseFaculties
                .Include(ccf => ccf.Class) // Bao gồm thông tin lớp học
                .Include(ccf => ccf.Course) // Bao gồm thông tin khóa học
                .Include(ccf => ccf.User) // Bao gồm thông tin giảng viên
                .ToListAsync(); // Trả về danh sách bất đồng bộ
        }

        // Lấy thông tin chi tiết ClassCourseFaculty theo ID
        public async Task<ClassCourseFaculty> GetClassCourseFacultyByIdAsync(int id)
        {
            return await _context.ClassCourseFaculties
                .Include(ccf => ccf.Class) // Bao gồm thông tin lớp học
                .Include(ccf => ccf.Course) // Bao gồm thông tin khóa học
                .Include(ccf => ccf.User) // Bao gồm thông tin giảng viên
                .FirstOrDefaultAsync(ccf => ccf.ClassCourseFacultyID == id); // Trả về bản ghi đầu tiên hoặc null
        }

        // Thêm mới một bản ghi ClassCourseFaculty
        public async Task AddClassCourseFacultyAsync(ClassCourseFaculty classCourseFaculty)
        {
            // Kiểm tra xem tổ hợp Class, Course, Faculty đã tồn tại chưa
            if (await IsClassCourseFacultyExistsAsync(classCourseFaculty.ClassID, classCourseFaculty.CourseID, classCourseFaculty.UserID))
            {
                throw new InvalidOperationException("This Class, Course, and Faculty combination already exists.");
                // Ném lỗi nếu tổ hợp đã tồn tại
            }

            // Kiểm tra vai trò của User (phải là Lecturer)
            var user = await _context.Users.FindAsync(classCourseFaculty.UserID);
            if (user == null || user.Role != "Lecturer")
            {
                throw new InvalidOperationException("User not found or not a lecturer.");
                // Ném lỗi nếu User không tồn tại hoặc không phải giảng viên
            }

            // Kiểm tra sự tồn tại của Class và Course
            var classEntity = await _context.Classes.FindAsync(classCourseFaculty.ClassID);
            var course = await _context.Courses.FindAsync(classCourseFaculty.CourseID);
            if (classEntity == null || course == null)
            {
                throw new InvalidOperationException("Class or Course not found.");
                // Ném lỗi nếu Class hoặc Course không tồn tại
            }

            // Thêm bản ghi mới vào cơ sở dữ liệu
            _context.ClassCourseFaculties.Add(classCourseFaculty);
            await _context.SaveChangesAsync(); // Lưu thay đổi bất đồng bộ
        }

        // Cập nhật thông tin ClassCourseFaculty
        public async Task UpdateClassCourseFacultyAsync(ClassCourseFaculty classCourseFaculty)
        {
            // Đánh dấu bản ghi là đã sửa đổi
            _context.Entry(classCourseFaculty).State = EntityState.Modified;
            await _context.SaveChangesAsync(); // Lưu thay đổi bất đồng bộ
        }

        // Xóa một bản ghi ClassCourseFaculty theo ID
        public async Task DeleteClassCourseFacultyAsync(int id)
        {
            // Tìm bản ghi theo ID
            var classCourseFaculty = await _context.ClassCourseFaculties.FindAsync(id);
            if (classCourseFaculty != null)
            {
                // Xóa bản ghi nếu tìm thấy
                _context.ClassCourseFaculties.Remove(classCourseFaculty);
                await _context.SaveChangesAsync(); // Lưu thay đổi bất đồng bộ
            }
            // Nếu không tìm thấy, không làm gì (không ném lỗi)
        }

        // Kiểm tra xem tổ hợp Class, Course, Faculty đã tồn tại chưa
        public async Task<bool> IsClassCourseFacultyExistsAsync(int classId, int courseId, int userId)
        {
            return await _context.ClassCourseFaculties
                .AnyAsync(ccf => ccf.ClassID == classId && ccf.CourseID == courseId && ccf.UserID == userId);
            // Trả về true nếu tổ hợp tồn tại, false nếu không
        }

        // Lấy danh sách ClassCourseFaculty theo UserID (giảng viên)
        public async Task<IEnumerable<ClassCourseFaculty>> GetClassCourseFacultiesByUserId(int userId)
        {
            return await _context.ClassCourseFaculties
                .Include(ccf => ccf.Class) // Bao gồm thông tin lớp học
                .Include(ccf => ccf.Course) // Bao gồm thông tin khóa học
                .Include(ccf => ccf.User) // Bao gồm thông tin giảng viên
                .Where(ccf => ccf.UserID == userId) // Lọc theo UserID (giảng viên)
                .ToListAsync(); // Trả về danh sách bất đồng bộ
        }
    }
}