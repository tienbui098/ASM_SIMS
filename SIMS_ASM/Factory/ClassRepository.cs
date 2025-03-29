using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Models;

namespace SIMS_ASM.Factory
{
    public class ClassRepository : Repository<Class>
    {
        private ApplicationDbContex _context;
        public ClassRepository(ApplicationDbContex context) : base(context)
        {
            this._context = context;
        }

        // Additional class-specific methods
        public IEnumerable<Class> GetClassesByMajor(int majorId)
        {
            return _context.Classes
                .Where(c => c.MajorID == majorId)
                .ToList();
        }

        public IQueryable<Class> GetAll()
        {
            return _context.Classes.AsQueryable();
        }

        public Class GetById(int id)
        {
            return _context.Classes.Include(c => c.Major).FirstOrDefault(c => c.ClassID == id);
        }
    }
}
