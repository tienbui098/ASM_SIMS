using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;
using SIMS_ASM.Singleton;

namespace SIMS_ASM.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContex _context;
        private readonly AccountSingleton _singleton;

        public CourseService(ApplicationDbContex context)
        {
            _context = context;
            _singleton = AccountSingleton.Instance;
        }

        // Lấy tất cả các khóa học từ database
        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Major) // Bao gồm thông tin ngành học
                .ToListAsync();
        }

        // Lấy thông tin khóa học theo ID
        public async Task<Course> GetCourseByIdAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Major) // Bao gồm thông tin ngành học
                .FirstOrDefaultAsync(c => c.CourseID == id);
        }

        // Lấy danh sách khóa học theo ngành học
        public async Task<IEnumerable<Course>> GetCoursesByMajorAsync(int majorId)
        {
            return await _context.Courses
                .Where(c => c.MajorID == majorId) // Lọc theo MajorID
                .Include(c => c.Major) // Bao gồm thông tin ngành học
                .ToListAsync();
        }

        // Tạo mới một khóa học
        public async Task<Course> CreateCourseAsync(Course course)
        {
            // Kiểm tra ngành học có tồn tại không
            var major = await _context.Majors.FindAsync(course.MajorID);
            if (major == null)
                throw new ArgumentException("Invalid Major ID");

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return course;
        }

        // Cập nhật thông tin khóa học
        public async Task UpdateCourseAsync(Course course)
        {
            // Kiểm tra ngành học có tồn tại không
            var major = await _context.Majors.FindAsync(course.MajorID);
            if (major == null)
                throw new ArgumentException("Invalid Major ID");

            _context.Entry(course).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Xóa khóa học với thông báo chi tiết
        public async Task<(bool Success, string Message)> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.ClassCourseFaculties) // Bao gồm thông tin lớp học và giảng viên liên quan
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
                return (false, "Course not found.");

            // Kiểm tra nếu khóa học đang được sử dụng trong các lớp học
            if (course.ClassCourseFaculties.Any())
                return (false, "Cannot delete course because it is associated with classes and faculty.");

            _context.Courses.Remove(course);

            try
            {
                await _context.SaveChangesAsync();
                return (true, "Course deleted successfully!");
            }
            catch (DbUpdateException ex)
            {
                // Ghi log lỗi để kiểm tra nguyên nhân
                _singleton.Log($"Failed to delete course with ID {id}: {ex.Message}");

                return (false, "Cannot delete course due to database constraints. It may still be associated with other entities.");
            }
        }

        // Kiểm tra xem khóa học có liên kết với lớp học nào không
        public async Task<bool> HasAssociatedClassesAsync(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.ClassCourseFaculties) // Bao gồm thông tin lớp học
                .FirstOrDefaultAsync(c => c.CourseID == courseId);
            return course != null && course.ClassCourseFaculties.Any();
        }
    }
}