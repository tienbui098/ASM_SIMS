using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public interface IEnrollmentService
    {
        // Lấy danh sách tất cả các đăng ký học phần
        Task<IEnumerable<Enrollment>> GetAllEnrollmentsAsync();

        // Lấy thông tin chi tiết một đăng ký học phần theo ID
        Task<Enrollment> GetEnrollmentByIdAsync(int id);

        // Thêm mới một đăng ký học phần
        // Kiểm tra các ràng buộc trước khi thêm
        Task AddEnrollmentAsync(Enrollment enrollment);

        // Cập nhật thông tin đăng ký học phần
        Task UpdateEnrollmentAsync(Enrollment enrollment);

        // Xóa một đăng ký học phần theo ID
        Task DeleteEnrollmentAsync(int id);

        // Kiểm tra sinh viên đã đăng ký học phần này chưa
        Task<bool> IsStudentAlreadyEnrolledAsync(int userId, int classCourseFacultyId);

        // Kiểm tra đăng ký học phần có điểm số liên quan hay không
        Task<bool> HasAssociatedGradesAsync(int enrollmentId);

        // Lấy danh sách đăng ký học phần của một sinh viên theo UserID
        Task<IEnumerable<Enrollment>> GetEnrollmentsByUserIdAsync(int userId);
    }
}