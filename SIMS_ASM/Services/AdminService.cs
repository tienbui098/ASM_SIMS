using SIMS_ASM.Data;
using SIMS_ASM.Factory;
using SIMS_ASM.Models;
using Microsoft.EntityFrameworkCore;


namespace SIMS_ASM.Services
{
    public class AdminService
    {
        private RepositoryFactory _repositoryFactory;

        public AdminService(ApplicationDbContex context)
        {
            _repositoryFactory = new RepositoryFactory(context);
        }

        // Course Management
        public IEnumerable<Course> GetAllCourses()
        {
            var courseRepo = _repositoryFactory.GetSpecificCourseRepository();
            return courseRepo.GetActiveCourses();
        }

        public Course GetCourseDetails(int courseId)
        {
            var courseRepo = _repositoryFactory.GetSpecificCourseRepository();
            return courseRepo.GetCourseWithDetails(courseId);
        }

        public void CreateCourse(Course course)
        {
            var courseRepo = _repositoryFactory.GetCourseRepository();
            courseRepo.Insert(course);
            courseRepo.Save();
        }

        public void UpdateCourse(Course course)
        {
            var courseRepo = _repositoryFactory.GetCourseRepository();
            courseRepo.Update(course);
            courseRepo.Save();
        }

        public void DeleteCourse(int courseId)
        {
            var courseRepo = _repositoryFactory.GetCourseRepository();
            courseRepo.Delete(courseId);
            courseRepo.Save();
        }


        

        public Major GetMajorDetails(int majorId)
        {
            var majorRepo = _repositoryFactory.GetSpecificMajorRepository();
            return majorRepo.GetById(majorId);
        }

        public IEnumerable<Major> GetMajorsWithCourses()
        {
            var majorRepo = _repositoryFactory.GetSpecificMajorRepository();
            return majorRepo.GetMajorsWithCourses();
        }

        public void CreateMajor(Major newMajor)
        {
            var majorRepo = _repositoryFactory.GetMajorRepository();
            majorRepo.Insert(newMajor);
            majorRepo.Save();
        }

        public void UpdateMajor(Major updatedMajor)
        {
            var majorRepo = _repositoryFactory.GetMajorRepository();
            majorRepo.Update(updatedMajor);
            majorRepo.Save();
        }

        public void DeleteMajor(int majorId)
        {
            var majorRepo = _repositoryFactory.GetMajorRepository();
            majorRepo.Delete(majorId);
            majorRepo.Save();
        }

       
    }
}
