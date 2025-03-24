using SIMS_ASM.Models;
namespace SIMS_ASM.Factory.Interfaces
{
    public interface IFactory<T> where T : class
    {
        T Create();
        bool Update(T entity);
        bool Delete(int id);
        T GetById(int id);
        IEnumerable<T> GetAll();
    }
}
