using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public interface ICourseService
    {
        // Lấy danh sách tất cả các khóa học
        Task<IEnumerable<Course>> GetAllCoursesAsync();

        // Lấy thông tin chi tiết của một khóa học theo ID
        Task<Course> GetCourseByIdAsync(int id);

        // Lấy danh sách các khóa học thuộc một ngành học (major) cụ thể
        Task<IEnumerable<Course>> GetCoursesByMajorAsync(int majorId);

        // Tạo mới một khóa học
        // Trả về khóa học vừa được tạo
        Task<Course> CreateCourseAsync(Course course);

        // Cập nhật thông tin của một khóa học
        Task UpdateCourseAsync(Course course);

        // Xóa một khóa học theo ID
        // Trả về tuple gồm:
        // - bool: thành công hay không
        // - string: thông báo kết quả
        Task<(bool Success, string Message)> DeleteCourseAsync(int id);

        // Kiểm tra xem khóa học có liên kết với bất kỳ lớp học nào không
        Task<bool> HasAssociatedClassesAsync(int courseId);
    }
}