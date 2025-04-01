using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public interface IGradeService
    {
        // Lấy danh sách tất cả các bản ghi điểm trong hệ thống
        Task<IEnumerable<Grade>> GetAllGradesAsync();

        // Lấy thông tin điểm cụ thể theo ID
        Task<Grade> GetGradeByIdAsync(int id);

        // Thêm mới một bản ghi điểm vào hệ thống
        Task AddGradeAsync(Grade grade);

        // Cập nhật thông tin điểm
        Task UpdateGradeAsync(Grade grade);

        // Xóa một bản ghi điểm theo ID
        Task DeleteGradeAsync(int id);

        // Kiểm tra sự tồn tại của bản ghi điểm theo ID
        Task<bool> GradeExistsAsync(int id);

        // Lấy danh sách điểm theo ID đăng ký học phần (Enrollment)
        Task<IEnumerable<Grade>> GetGradesByEnrollmentAsync(int enrollmentId);

        // Lấy danh sách điểm của một sinh viên cụ thể theo UserID
        Task<IEnumerable<Grade>> GetGradesByUserId(int userId);
    }
}