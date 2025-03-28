using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContex _context;

        public CourseService(ApplicationDbContex context)
        {
            _context = context;
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

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.ClassCourseFaculties)
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
                return false;

            if (course.ClassCourseFaculties.Any())
                return false;

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
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
