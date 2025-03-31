using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public interface IStudentClassService
    {
        Task<IEnumerable<StudentClass>> GetAllStudentClassesAsync();
        Task<StudentClass> GetStudentClassByIdAsync(int id);
        Task AddStudentClassAsync(StudentClass studentClass);
        Task UpdateStudentClassAsync(StudentClass studentClass);
        Task DeleteStudentClassAsync(int id);
        Task<bool> IsStudentAlreadyInClassAsync(int userId, int classId);
        Task<IEnumerable<int>> GetClassIdsByStudentAsync(int userId); // Thêm phương thức để lấy danh sách ClassID của sinh viên
        Task<bool> HasAssociatedEnrollmentsAsync(int userID, int classID);
        Task AddMultipleStudentsToClassAsync(List<int> studentIds, int classId);
        Task<List<User>> GetStudentsInClassAsync(int classId);
        Task RemoveAllStudentsFromClassAsync(int classId);
    }
}
