using SIMS_ASM.Models;

namespace SIMS_ASM.Factory
{
    public interface ICourseRepository : IRepository<Course>
    {
        Course GetCourseWithDetails(int courseId);
    }
}
