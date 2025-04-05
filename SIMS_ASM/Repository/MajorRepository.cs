using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;

namespace SIMS_ASM.Factory
{
    public class MajorRepository : Repository<Major>, IMajorRepository
    {
        public MajorRepository(ApplicationDbContex context) : base(context)
        {
        }

        public IEnumerable<Major> GetMajorsWithCourses()
        {
            return _context.Majors
                .Include(m => m.Courses)
                .ToList();
        }
    }
}
