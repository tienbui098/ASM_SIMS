using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;

namespace SIMS_ASM.Factory
{
    public class CourseRepository : Repository<Course>
    {
        private ApplicationDbContex _context;
        public CourseRepository(ApplicationDbContex context) : base(context)
        {
            this._context = context;
        }

        // Additional course-specific methods
        public IEnumerable<Course> GetActiveCourses()
        {
            return _context.Courses
                .Where(c => c.CourseEndDate > DateTime.Now)
                .ToList();
        }

        public Course GetCourseWithDetails(int courseId)
        {
            return _context.Courses
                .Include(c => c.Major)
                .Include(c => c.ClassCourseFaculties)
                .FirstOrDefault(c => c.CourseID == courseId);
        }
    }
}
