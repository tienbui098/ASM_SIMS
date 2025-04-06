using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;

namespace SIMS_ASM.Factory
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(ApplicationDbContex context) : base(context)
        {
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
