using SIMS_ASM.Models;
namespace SIMS_ASM.Factory.Interfaces
{
    public interface IUserFactory : IFactory<User>
    {
        IEnumerable<User> GetStudents();
        IEnumerable<User> GetLecturers();
        IEnumerable<User> GetAdministrators();
    }
}
