using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;

namespace SIMS_ASM.Factory
{
    public class MajorRepository : Repository<Major>
    {
        private ApplicationDbContex _context;

        public MajorRepository(ApplicationDbContex context) : base(context)
        {
            this._context = context;
        }

        // Additional major-specific methods
        public IEnumerable<Major> GetMajorsWithCourses()
        {
            return _context.Majors
                .Include(m => m.Courses)
                .ToList();
        }
    }
}
