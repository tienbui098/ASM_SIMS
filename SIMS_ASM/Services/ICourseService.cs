using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetAllCoursesAsync();
        Task<Course> GetCourseByIdAsync(int id);
        Task<IEnumerable<Course>> GetCoursesByMajorAsync(int majorId);
        Task<Course> CreateCourseAsync(Course course);
        Task UpdateCourseAsync(Course course);
        Task<bool> DeleteCourseAsync(int id); // Trả về bool để báo hiệu thành công/thất bại
        Task<bool> HasAssociatedClassesAsync(int courseId); // Kiểm tra khóa học có liên kết với ClassCourseFaculty không
    }
}
