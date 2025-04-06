using SIMS_ASM.Models;

namespace SIMS_ASM.Factory
{
    public interface IClassRepository : IRepository<Class>
    {
        IEnumerable<Class> GetClassesByMajor(int majorId);
        Class GetClassWithMajor(int id);
        IQueryable<Class> GetAllQueryable();
    }
}
