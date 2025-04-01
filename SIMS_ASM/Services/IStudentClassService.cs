using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public interface IStudentClassService
    {
        // Lấy danh sách tất cả quan hệ sinh viên - lớp học
        Task<IEnumerable<StudentClass>> GetAllStudentClassesAsync();

        // Lấy thông tin quan hệ sinh viên - lớp học theo ID
        Task<StudentClass> GetStudentClassByIdAsync(int id);

        // Thêm mới một quan hệ sinh viên vào lớp học
        Task AddStudentClassAsync(StudentClass studentClass);

        // Cập nhật thông tin quan hệ sinh viên - lớp học
        Task UpdateStudentClassAsync(StudentClass studentClass);

        // Xóa một quan hệ sinh viên - lớp học theo ID
        Task DeleteStudentClassAsync(int id);

        // Kiểm tra sinh viên đã tồn tại trong lớp học chưa
        Task<bool> IsStudentAlreadyInClassAsync(int userId, int classId);

        // Lấy danh sách ID các lớp mà sinh viên tham gia
        Task<IEnumerable<int>> GetClassIdsByStudentAsync(int userId);

        // Kiểm tra quan hệ sinh viên - lớp có liên kết với bảng đăng ký học phần không
        Task<bool> HasAssociatedEnrollmentsAsync(int userID, int classID);

        // Thêm nhiều sinh viên vào một lớp cùng lúc
        // studentIds: Danh sách ID các sinh viên cần thêm
        // classId: ID lớp học cần thêm vào
        Task AddMultipleStudentsToClassAsync(List<int> studentIds, int classId);

        // Lấy danh sách sinh viên trong một lớp cụ thể
        Task<List<User>> GetStudentsInClassAsync(int classId);

        // Xóa tất cả sinh viên khỏi một lớp học
        Task RemoveAllStudentsFromClassAsync(int classId);
    }
}