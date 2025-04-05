using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;

namespace SIMS_ASM.Factory
{
    public class ClassRepository : Repository<Class>, IClassRepository
    {
        public ClassRepository(ApplicationDbContex context) : base(context)
        {
        }

        public IEnumerable<Class> GetClassesByMajor(int majorId)
        {
            return _context.Classes
                .Where(c => c.MajorID == majorId)
                .ToList();
        }

        public Class GetClassWithMajor(int id)
        {
            return _context.Classes
                .Include(c => c.Major)
                .FirstOrDefault(c => c.ClassID == id);
        }

        public IQueryable<Class> GetAllQueryable()
        {
            return _context.Classes.AsQueryable();
        }
    }
}
