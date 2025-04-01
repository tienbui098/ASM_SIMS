using SIMS_ASM.Models;

namespace SIMS_ASM.Services
{
    public interface IUserService
    {
        // Lấy thông tin người dùng theo ID
        Task<User> GetUserByIdAsync(int id);

        // Cập nhật thông tin người dùng
        Task UpdateUserAsync(User user);

        // Thêm người dùng mới vào hệ thống với vai trò (role) được chỉ định
        Task AddUserAsync(User user, string role);

        // Xóa người dùng theo ID
        // Trả về true nếu xóa thành công, false nếu thất bại
        Task<bool> DeleteUserAsync(int id);

        // Lấy danh sách tất cả giảng viên trong hệ thống
        Task<IEnumerable<User>> GetLecturersAsync();

        // Lấy danh sách tất cả sinh viên trong hệ thống
        Task<IEnumerable<User>> GetStudentsAsync();

        // Tìm kiếm người dùng theo tên đăng nhập (username)
        Task<User> GetUserByUsernameAsync(string username);
    }
}