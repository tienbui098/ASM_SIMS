using Microsoft.EntityFrameworkCore;
using SIMS_ASM.Data;
using SIMS_ASM.Factory;
using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public class ClassService
    {

        private RepositoryFactory _repositoryFactory;

        public ClassService(ApplicationDbContex context)
        {
            _repositoryFactory = new RepositoryFactory(context);
        }

        // Class Management Methods
        public IEnumerable<Class> GetAllClasses()
        {
            var classRepo = _repositoryFactory.GetSpecificClassRepository();
            return classRepo.GetAll().Include(c => c.Major).ToList(); // Thêm Include
        }

        public Class GetClassDetails(int classId)
        {
            var classRepo = _repositoryFactory.GetSpecificClassRepository();
            return classRepo.GetById(classId);
        }

        public IEnumerable<Class> GetClassesByMajor(int majorId)
        {
            var classRepo = _repositoryFactory.GetSpecificClassRepository();
            return classRepo.GetClassesByMajor(majorId);
        }

        public void CreateClass(Class newClass)
        {
            var classRepo = _repositoryFactory.GetClassRepository();
            classRepo.Insert(newClass);
            classRepo.Save();
        }

        public void UpdateClass(Class updatedClass)
        {
            var classRepo = _repositoryFactory.GetClassRepository();
            classRepo.Update(updatedClass);
            classRepo.Save();
        }

        public void DeleteClass(int classId)
        {
            var classRepo = _repositoryFactory.GetClassRepository();
            classRepo.Delete(classId);
            classRepo.Save();
        }

        // Major Management Methods
        public IEnumerable<Major> GetAllMajors()
        {
            var majorRepo = _repositoryFactory.GetSpecificMajorRepository();
            return majorRepo.GetAll();
        }
    }
}
