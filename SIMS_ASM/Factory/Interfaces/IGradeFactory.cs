using SIMS_ASM.Models;
namespace SIMS_ASM.Factory.Interfaces
{
    public interface IGradeFactory : IFactory<Grade>
    {
        IEnumerable<Grade> GetGradesByStudentId(int studentId);
        IEnumerable<Grade> GetGradesByCourseId(int courseId);
        Grade GetGradeByStudentAndCourse(int studentId, int courseId);
    }
}
