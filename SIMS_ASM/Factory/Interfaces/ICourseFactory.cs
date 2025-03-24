using SIMS_ASM.Models;
namespace SIMS_ASM.Factory.Interfaces
{
    public interface ICourseFactory : IFactory<Course>
    {
        bool AssignCourseToStudent(int courseId, int studentId);
        bool RemoveStudentFromCourse(int courseId, int studentId);
        IEnumerable<Course> GetCoursesByStudentId(int studentId);
        IEnumerable<User> GetStudentsByCourseId(int courseId);
    }
}
