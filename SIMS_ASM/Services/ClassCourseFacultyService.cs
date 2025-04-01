using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public class ClassCourseFacultyService : IClassCourseFacultyService
    {
        private readonly ApplicationDbContex _context;

        public ClassCourseFacultyService(ApplicationDbContex context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ClassCourseFaculty>> GetAllClassCourseFacultiesAsync()
        {
            return await _context.ClassCourseFaculties
                .Include(ccf => ccf.Class)
                .Include(ccf => ccf.Course)
                .Include(ccf => ccf.User)
                .ToListAsync();
        }

        public async Task<ClassCourseFaculty> GetClassCourseFacultyByIdAsync(int id)
        {
            return await _context.ClassCourseFaculties
                .Include(ccf => ccf.Class)
                .Include(ccf => ccf.Course)
                .Include(ccf => ccf.User)
                .FirstOrDefaultAsync(ccf => ccf.ClassCourseFacultyID == id);
        }

        public async Task AddClassCourseFacultyAsync(ClassCourseFaculty classCourseFaculty)
        {
            // Kiểm tra xem Class, Course, Faculty đã được gán chưa
            if (await IsClassCourseFacultyExistsAsync(classCourseFaculty.ClassID, classCourseFaculty.CourseID, classCourseFaculty.UserID))
            {
                throw new InvalidOperationException("This Class, Course, and Faculty combination already exists.");
            }

            // Kiểm tra xem User có role "Lecturer" không
            var user = await _context.Users.FindAsync(classCourseFaculty.UserID);
            if (user == null || user.Role != "Lecturer")
            {
                throw new InvalidOperationException("User not found or not a lecturer.");
            }

            // Kiểm tra xem Class và Course có tồn tại không
            var classEntity = await _context.Classes.FindAsync(classCourseFaculty.ClassID);
            var course = await _context.Courses.FindAsync(classCourseFaculty.CourseID);
            if (classEntity == null || course == null)
            {
                throw new InvalidOperationException("Class or Course not found.");
            }

            _context.ClassCourseFaculties.Add(classCourseFaculty);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateClassCourseFacultyAsync(ClassCourseFaculty classCourseFaculty)
        {
            _context.Entry(classCourseFaculty).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteClassCourseFacultyAsync(int id)
        {
            var classCourseFaculty = await _context.ClassCourseFaculties.FindAsync(id);
            if (classCourseFaculty != null)
            {
                _context.ClassCourseFaculties.Remove(classCourseFaculty);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsClassCourseFacultyExistsAsync(int classId, int courseId, int userId)
        {
            return await _context.ClassCourseFaculties
                .AnyAsync(ccf => ccf.ClassID == classId && ccf.CourseID == courseId && ccf.UserID == userId);
        }

        public async Task<IEnumerable<ClassCourseFaculty>> GetClassCourseFacultiesByUserId(int userId)
        {
            // Giả định rằng ClassCourseFaculty có thuộc tính UserId để xác định giảng viên nào dạy lớp nào
            return await _context.ClassCourseFaculties
                .Include(ccf => ccf.Class)
                .Include(ccf => ccf.Course)
                .Include(ccf => ccf.User) // Ngoài ra, có thể bao gồm thông tin người dùng
                .Where(ccf => ccf.UserID == userId) // Lọc theo userId
                .ToListAsync();
        }
    }
}
