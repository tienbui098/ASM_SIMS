using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public interface IClassCourseFacultyService
    {
        // Lấy tất cả các bản ghi ClassCourseFaculty từ hệ thống
        Task<IEnumerable<ClassCourseFaculty>> GetAllClassCourseFacultiesAsync();

        // Lấy thông tin ClassCourseFaculty theo ID
        Task<ClassCourseFaculty> GetClassCourseFacultyByIdAsync(int id);

        // Thêm mới một bản ghi ClassCourseFaculty vào hệ thống
        Task AddClassCourseFacultyAsync(ClassCourseFaculty classCourseFaculty);

        // Cập nhật thông tin của một ClassCourseFaculty
        Task UpdateClassCourseFacultyAsync(ClassCourseFaculty classCourseFaculty);

        // Xóa một ClassCourseFaculty theo ID
        Task DeleteClassCourseFacultyAsync(int id);

        // Kiểm tra xem một bản ghi ClassCourseFaculty đã tồn tại hay chưa
        Task<bool> IsClassCourseFacultyExistsAsync(int classId, int courseId, int userId);

        // Lấy danh sách ClassCourseFaculty theo UserID (Faculty)
        // Dùng để xem các lớp-môn mà giảng viên phụ trách
        Task<IEnumerable<ClassCourseFaculty>> GetClassCourseFacultiesByUserId(int userId);
    }
}