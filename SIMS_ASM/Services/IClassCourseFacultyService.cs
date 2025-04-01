using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public interface IClassCourseFacultyService
    {
        Task<IEnumerable<ClassCourseFaculty>> GetAllClassCourseFacultiesAsync();
        Task<ClassCourseFaculty> GetClassCourseFacultyByIdAsync(int id);
        Task AddClassCourseFacultyAsync(ClassCourseFaculty classCourseFaculty);
        Task UpdateClassCourseFacultyAsync(ClassCourseFaculty classCourseFaculty);
        Task DeleteClassCourseFacultyAsync(int id);
        Task<bool> IsClassCourseFacultyExistsAsync(int classId, int courseId, int userId);
        Task<IEnumerable<ClassCourseFaculty>> GetClassCourseFacultiesByUserId(int userId);
    }
}
