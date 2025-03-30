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

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Major)
                .ToListAsync();
        }

        public async Task<Course> GetCourseByIdAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Major)
                .FirstOrDefaultAsync(c => c.CourseID == id);
        }

        public async Task<IEnumerable<Course>> GetCoursesByMajorAsync(int majorId)
        {
            return await _context.Courses
                .Where(c => c.MajorID == majorId)
                .Include(c => c.Major)
                .ToListAsync();
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            var major = await _context.Majors.FindAsync(course.MajorID);
            if (major == null)
                throw new ArgumentException("Invalid Major ID");

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task UpdateCourseAsync(Course course)
        {
            var major = await _context.Majors.FindAsync(course.MajorID);
            if (major == null)
                throw new ArgumentException("Invalid Major ID");

            _context.Entry(course).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        //public async Task<bool> DeleteCourseAsync(int id)
        //{
        //    var course = await _context.Courses
        //        .Include(c => c.ClassCourseFaculties)
        //        .FirstOrDefaultAsync(c => c.CourseID == id);

        //    if (course == null)
        //        return false;

        //    if (course.ClassCourseFaculties.Any())
        //        return false;

        //    _context.Courses.Remove(course);
        //    await _context.SaveChangesAsync();
        //    return true;
        //}

        public async Task<(bool Success, string Message)> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.ClassCourseFaculties)
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
                return (false, "Course not found.");

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

        public async Task<bool> HasAssociatedClassesAsync(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.ClassCourseFaculties)
                .FirstOrDefaultAsync(c => c.CourseID == courseId);
            return course != null && course.ClassCourseFaculties.Any();
        }
    }
}
