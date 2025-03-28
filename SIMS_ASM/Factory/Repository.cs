using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;

namespace SIMS_ASM.Factory
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private ApplicationDbContex _context;
        private DbSet<T> table;

        public Repository(ApplicationDbContex context)
        {
            this._context = context;
            this.table = context.Set<T>();
        }

        public IEnumerable<T> GetAll()
        {
            return table.ToList();
        }

        public T GetById(int id)
        {
            return table.Find(id);
        }

        public void Insert(T obj)
        {
            table.Add(obj);
        }

        public void Update(T obj)
        {
            table.Attach(obj);
            _context.Entry(obj).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            T existing = table.Find(id);
            table.Remove(existing);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
