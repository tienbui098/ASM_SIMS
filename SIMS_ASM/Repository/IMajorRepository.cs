using SIMS_ASM.Models;

namespace SIMS_ASM.Factory
{
    public interface IMajorRepository : IRepository<Major>
    {
        IEnumerable<Major> GetMajorsWithCourses();
    }
}
