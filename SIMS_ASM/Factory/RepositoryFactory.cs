using SIMS_ASM.Data;
using SIMS_ASM.Models;

namespace SIMS_ASM.Factory
{
    public class RepositoryFactory
    {
        private ApplicationDbContex _context;

        public RepositoryFactory(ApplicationDbContex context)
        {
            _context = context;
        }

        public IRepository<Course> GetCourseRepository()
        {
            return new CourseRepository(_context);
        }

        public IRepository<Major> GetMajorRepository()
        {
            return new MajorRepository(_context);
        }

        public IRepository<Class> GetClassRepository()
        {
            return new ClassRepository(_context);
        }

        // Specific repository getters with additional methods
        public CourseRepository GetSpecificCourseRepository()
        {
            return new CourseRepository(_context);
        }

        public MajorRepository GetSpecificMajorRepository()
        {
            return new MajorRepository(_context);
        }

        public ClassRepository GetSpecificClassRepository()
        {
            return new ClassRepository(_context);
        }
    }
}
